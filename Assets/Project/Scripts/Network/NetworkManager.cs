/*
 * Author: Joseph Malibiran
 * Last Updated: October 22, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using FPSNetworkCode;

namespace FPSNetworkCode {

    public class NetworkManager : MonoBehaviour {
        public UdpClient udp;

        [Header("Debug Settings")] //Helps in enabling and disabling debug messages from this script should a programmer want to focus on other console messages.
        [SerializeField] private bool bDebug = true;
        [SerializeField] private bool bVerboseDebug = false;

        [Header("Debug Read-Only")] //Values shown for debugging. Although they can be edited in the inspector, it does not change the actual values being used by code.
        [SerializeField] private string showClientPublicIP;
        [SerializeField] private string showClientPort;
        [SerializeField] private string showPing; //Ping displays the average time between a 'ping' message to the server and when it receives a 'pong' message from the server.
        [SerializeField] private bool showIfConnected;

        [Header("References")]
        [SerializeField] private ConsoleManager consoleRef;

        [Header("UDP Settings")] //These values must be filled in the inspector
        [SerializeField] private string remoteEndpointAddress;
        [SerializeField] private int remoteEndpointPort;

        //private Dictionary<int, Transform> playerReferences; //Contains references of gameObjects as Transform representing the characters of clients connected to an online match
        private Dictionary<int, PlayerData> clientDataDict;
        private List<int> lobbyPlayerIDList;
        private Queue<string> dataQueue; //data received from the server is queued so they are only processed once chronologically

        // Refers to *this* client
        private string clientIP;
        private string clientPort;
        private string clientUsername;

        private bool isConnected = false;
        private bool isInLobby = false;
        private bool isLoggedIn = false;

        private void Awake() {
            dataQueue = new Queue<string>();
            clientUsername = "default";
        }

        // Start is called before the first frame update
        //private void Start() {
        //    NetworkSetUp();
        //}

        private void OnDestroy() {
            if (bDebug){ Debug.Log("[Notice] Cleaning up UDP Client...");}
            udp.Dispose();
            if (dataQueue.Count > 0) {
                dataQueue.Clear();
            }
            isConnected = false;
            showIfConnected = isConnected;
        }

        private void FixedUpdate() {
            ProcessServerMessages();
        }

        //Receives messages from the server
        private void OnReceived(IAsyncResult result) {
            // this is what had been passed into BeginReceive as the second parameter:
            UdpClient socket = result.AsyncState as UdpClient;
        
            // points towards whoever had sent the message:
            IPEndPoint source = new IPEndPoint(0, 0);

            // get the actual message and fill out the source:
            byte[] message = socket.EndReceive(result, ref source);
        
            // do what you'd like with `message` here:
            string returnData = Encoding.ASCII.GetString(message);

            // Stores data in a queue to be processed later.
            dataQueue.Enqueue(returnData);

            if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Received Data: " + returnData); }

            // schedule the next receive operation once reading is done:
            socket.BeginReceive(new AsyncCallback(OnReceived), socket);
        }

        //Attempt to make a connection to server at given ip 'remoteEndpointAddress' and port 'remoteEndpointPort'
        private void NetworkSetUp() {
            string msgJson;
            Byte[] sendBytes;
            ConnectNetMsg connectMsg; //Note: FlagNetworkMsg is a class that only has a flag enum property. The term 'flag' in this context identifies the type of a network message.

            if (bDebug){ Debug.Log("[Notice] Setting up client...");}

            udp = new UdpClient();
            clientDataDict = new Dictionary<int, PlayerData>();
            lobbyPlayerIDList = new List<int>();
            dataQueue = new Queue<string>();
            connectMsg = new ConnectNetMsg(GameVersion.GetVersion(), clientUsername); 

            if (bDebug){ Debug.Log("[Notice] Client connecting to Server...");}

            //Attempt to connect to server
            try{
                udp.Connect(remoteEndpointAddress,remoteEndpointPort);
            }
            catch (Exception e ) {
                Debug.LogWarning("[Warning] Failed to connect to server. " + e.ToString() + " Aborting operation...");
                return;
            }

            //Send flag 'connect' to server
            msgJson = JsonUtility.ToJson(connectMsg); 
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);

            if (bDebug){ Debug.Log("[Notice] Client is now listening for server messages...");}

            //Allow the client to receive network messages from the server
            udp.BeginReceive(new AsyncCallback(OnReceived), udp);

            //if (bDebug){ Debug.Log("[Notice] Routinely pinging server...");}

            //InvokeRepeating("RoutinePing", 1, 1); // Sends 1 ping message to server every 1 second.
        }

        private void SendFlagMessage(Flag getFlag) {
            if (!isConnected) {
                return;
            }

            string msgJson;
            FlagNetMsg flagMsg;
            Byte[] sendBytes;

            if (bDebug && bVerboseDebug){ Debug.Log("[Notice] Sending flag " + getFlag.ToString() +" to server..."); }

            //Send flag to server
            flagMsg = new FlagNetMsg(getFlag);
            msgJson = JsonUtility.ToJson(flagMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);
        }

        //Sends a 'pong' message to the server
        //TODO this method might be replaced by SendFlagMessage()
        private void ResponsePong() {
            if (!isConnected) {
                return;
            }

            string msgJson;
            FlagNetMsg pingFlagMsg;
            Byte[] sendBytes;

            if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Sending flag to server: pong"); }

            //Send flag 'ping' to server
            pingFlagMsg = new FlagNetMsg(Flag.PONG);
            msgJson = JsonUtility.ToJson(pingFlagMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);

        }

        //Sends this game's version to the server
        private void ResponseVersion() {
            if (!isConnected) {
                return;
            }

            string msgJson;
            StringNetMsg versionNetMsg;
            Byte[] sendBytes;

            Debug.Log("[Notice] Sending version to server...");
            versionNetMsg = new StringNetMsg(Flag.VERSION);
            versionNetMsg.message = GameVersion.GetVersion();

            msgJson = JsonUtility.ToJson(versionNetMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);
        }

        //Process the network messages sent by the server. We stored them in a queue during OnReceived(). TODO incomplete
        private void ProcessServerMessages() {
            //Only process this function if there is data in queue
            if (dataQueue.Count <= 0) {
                return;
            }

            if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.CONNECT) {
                isConnected = true;
                showIfConnected = isConnected;
                if (bDebug) { Debug.Log("[Notice] Client connection established with " + udp.Client.RemoteEndPoint.ToString() + "."); }
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Client connection established with server.");
                }
                dataQueue.Dequeue();
            }
            else if (!isConnected) {
                dataQueue.Clear();
                return;
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.CREATE_ACCOUNT) {
                if (bDebug){ Debug.Log("[Notice] New account created."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Account created successfully.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_ACCOUNT_CREATION) {
                if (bDebug){ Debug.Log("[Notice] Failed to create new account."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Failed to create account; Invalid username or password.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.LOGIN_ACCOUNT) {
                isLoggedIn = true;

                if (bDebug){ Debug.Log("[Notice] Client has successfully logged in."); }

                //TODO: Exit login screen; Load next scene 

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Successfully logged in.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_LOGIN) {
                if (bDebug){ Debug.Log("[Notice] Failed to log in."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Login failed; Invalid username or password.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.PING) { //Received a ping from the server
                if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Received flag from server: ping"); }
                ResponsePong(); //Send a response pong message
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.VERSION) { //Receives a version request from server. Note: Not used in this case as the client sends the version with the first CONNECT message
                if (bDebug){ Debug.Log("[Routine] Received flag from server: version"); }
                ResponseVersion();
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.INVALID_VERSION) { //Receives an invalid version message from server
                if (bDebug){ Debug.Log("[Notice] Received flag from server: invalid version"); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Incompatible client version; disconnecting from server...");
                }

                Disconnect();
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.JOINED_LOBBY) { //TODO untested
                isInLobby = true;
                Debug.Log("[Notice] You have joined a lobby.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You have joined a lobby.");
                }
                LobbyUpdate lobbyClientList = JsonUtility.FromJson<LobbyUpdate>(dataQueue.Peek());

                //TODO untested
                //Save client info. just username and id for now

                foreach (LobbyClient client in lobbyClientList.clients) {
                    PlayerData newPlayerData = new PlayerData();
                    newPlayerData.username = client.username;
                    clientDataDict.Add(client.id, newPlayerData);
                    lobbyPlayerIDList.Add(client.id);
                }

                dataQueue.Dequeue();

                PrintLobbyClients(); //TEMP
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.LEAVE_LOBBY) {
                isInLobby = false;
                Debug.Log("[Notice] You have left the lobby.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You have left the lobby.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.UPDATE_LOBBY) { //TODO untested
                LobbyNewClientUpdate newClient;
                isInLobby = false;

                newClient = JsonUtility.FromJson<LobbyNewClientUpdate>(dataQueue.Peek());

                Debug.Log("[Notice] A new client has joined the lobby.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] " + newClient.username + " has joined the lobby.");
                }
                
                PlayerData newPlayerData = new PlayerData();
                newPlayerData.username = newClient.username;
                clientDataDict.Add(newClient.id, newPlayerData);
                lobbyPlayerIDList.Add(newClient.id);
                dataQueue.Dequeue();

                PrintLobbyClients(); //TEMP
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.CREATED_LOBBY) {
                isInLobby = true;
                Debug.Log("[Notice] A new lobby has been created.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] A new lobby has been created.");
                }

                dataQueue.Dequeue();
            }
        }

        public void ConnectToServer() {
            if (isConnected) {
                Debug.Log("[Notice] You are already connected to server; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You are already connected to server; aborting operation...");
                }
                return;
            }

            NetworkSetUp();
        }

        public void AttemptAccountCreation(string username, string password) {

            string msgJson;
            LoginMsg acctCreateMsg;
            Byte[] sendBytes;

            Debug.Log("[Notice] Sending registration info to server...");
            acctCreateMsg = new LoginMsg(Flag.CREATE_ACCOUNT);
            acctCreateMsg.version = GameVersion.GetVersion();
            acctCreateMsg.username = username;
            acctCreateMsg.password = password;

            msgJson = JsonUtility.ToJson(acctCreateMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);
        }

        public void AttemptLogin(string username, string password) {

            string msgJson;
            LoginMsg acctCreateMsg;
            Byte[] sendBytes;

            Debug.Log("[Notice] Sending login info to server...");
            acctCreateMsg = new LoginMsg(Flag.LOGIN_ACCOUNT);
            acctCreateMsg.version = GameVersion.GetVersion();
            acctCreateMsg.username = username;
            acctCreateMsg.password = password;

            msgJson = JsonUtility.ToJson(acctCreateMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);
        }

        public void CommenceMatchmaking() {
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot join matchmaking; user is not connected to server.");
                }
                return;
            }

            if (isInLobby) {
                Debug.Log("[Notice] You are already in a game lobby; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot join matchmaking; you are already in a game lobby...");
                }
                return;
            }

            SendFlagMessage(Flag.QUEUE_MATCHMAKING);
        }

        public void LeaveCurrentLobby() {
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot join matchmaking; user is not connected to server.");
                }
                return;
            }

            if (isInLobby == false) {
                Debug.Log("[Notice] You are not in a game lobby; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You are not in a game lobby; aborting operation...");
                }
                return;
            }

            SendFlagMessage(Flag.LEAVE_LOBBY);
        }

        public void Disconnect() {
            if (!isConnected) {
                Debug.LogWarning("[Notice] You are not connected to a server; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You are not connected to a server; aborting operation...");
                }
                return;
            }

            udp.Dispose();
            isConnected = false;
            showIfConnected = isConnected;
            Debug.Log("[Notice] Disconnected from server.");
            if (consoleRef) {
                consoleRef.UpdateChat("[Console] Disconnected from server.");
            }
        }

        //Prints the list of players that are in the same lobby as this client
        public void PrintLobbyClients() {

            if (lobbyPlayerIDList.Count <= 0) {
                Debug.Log("[Notice] Lobby player list is empty.");
                return;
            }

            Debug.Log("[Notice] Current Lobby player list:");
            if (consoleRef) {
                consoleRef.UpdateChat("[Notice] Current Lobby player list:");
            }

            foreach (int clientKey in lobbyPlayerIDList) {
                Debug.Log("    - " + clientDataDict[clientKey].username + "(" + clientKey.ToString() + ")");
                if (consoleRef) {
                    consoleRef.UpdateChat("    - " + clientDataDict[clientKey].username + "(" + clientKey.ToString() + ")");
                }
            }
        }
    }

}
