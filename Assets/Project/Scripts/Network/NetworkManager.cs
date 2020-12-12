/*
 * Author: Joseph Malibiran
 * Last Updated: October 22, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using FPSNetworkCode;
using FPSGameplayCode;

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
        [SerializeField] private LoginController loginCtrlRef;
        [SerializeField] private SelectionController selectCtrlRef;
        [SerializeField] private ProfileMgr profileMgrRef;
        [SerializeField] private GameplayManager gameMgrRef;

        [Header("UDP Settings")] //These values must be filled in the inspector
        [SerializeField] private string remoteEndpointAddress;
        [SerializeField] private int remoteEndpointPort;

        //private Dictionary<int, Transform> playerReferences; //Contains references of gameObjects as Transform representing the characters of clients connected to an online match
        private Dictionary<string, PlayerData> clientDataDict;
        //private List<int> lobbyPlayerIDList;
        private Queue<string> dataQueue; //data received from the server is queued so they are only processed once chronologically

        // Refers to *this* client
        private string clientIP;
        private string clientPort;
        private string clientUsername;

        [SerializeField] private bool isConnected = false;
        [SerializeField] private bool isInMatch = false;
        [SerializeField] private bool isLoggedIn = false;
        [SerializeField] private bool isInMMQueue = false;
        private bool loopMoveUpdate = false;

        private void Awake() {
            dataQueue = new Queue<string>();
            clientUsername = "default";
        }

        private void OnDestroy() {
            if (bDebug){ Debug.Log("[Notice] Cleaning up UDP Client...");}

            if (isConnected) {
                udp.Dispose();
            }
            
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
            clientDataDict = new Dictionary<string, PlayerData>();
            //lobbyPlayerIDList = new List<int>();
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
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");
                dataQueue.Dequeue();
            }
            else if (!isConnected) {
                Debug.LogError("[Error] Received message from server, but cannot process it because client is not connected to server."); 
                dataQueue.Clear();
                return;
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.CREATE_ACCOUNT) {
                if (bDebug){ Debug.Log("[Notice] New account created."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Account created successfully.");
                }
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");

                if (loginCtrlRef) {
                    loginCtrlRef.AccountCreated();
                }
                else if (GameObject.Find("Canvas/Login Controller")){
                    loginCtrlRef = GameObject.Find("Canvas/Login Controller").GetComponent<LoginController>();
                    loginCtrlRef.AccountCreated();
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_ACCOUNT_CREATION) {
                if (bDebug){ Debug.Log("[Notice] Failed to create new account."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Failed to create account; Invalid username or password.");
                }

                if (loginCtrlRef) {
                    loginCtrlRef.UserExists();
                }
                else if (GameObject.Find("Canvas/Login Controller")){
                    loginCtrlRef = GameObject.Find("Canvas/Login Controller").GetComponent<LoginController>();
                    loginCtrlRef.UserExists();
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.LOGIN_ACCOUNT) {
                isLoggedIn = true;
                GameStateManager.SetState(State.GAMESELECTSCENE);
                if (bDebug){ Debug.Log("[Notice] Client has successfully logged in."); }

                //TODO: Exit login screen; Load next scene 

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Successfully logged in.");
                }
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");

                if (loginCtrlRef) {
                    loginCtrlRef.ConfirmServerLogin();
                }
                else if (GameObject.Find("Canvas/Login Controller")){
                    loginCtrlRef = GameObject.Find("Canvas/Login Controller").GetComponent<LoginController>();
                    loginCtrlRef.ConfirmServerLogin();
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_LOGIN) {
                if (bDebug){ Debug.Log("[Notice] Failed to log in."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Login failed; Invalid username or password.");
                }

                if (loginCtrlRef) {
                    loginCtrlRef.IncorrectLogin();
                }
                else if (GameObject.Find("Canvas/Login Controller")){
                    loginCtrlRef = GameObject.Find("Canvas/Login Controller").GetComponent<LoginController>();
                    loginCtrlRef.IncorrectLogin();
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.PING) { //Received a ping from the server
                if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Received flag from server: PING."); }
                ResponsePong(); //Send a response pong message
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.VERSION) { //Receives a version request from server. Note: Not used in this case as the client sends the version with the first CONNECT message
                if (bDebug){ Debug.Log("[Routine] Received flag from server: VERSION."); }
                ResponseVersion();
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.INVALID_VERSION) { //Receives an invalid version message from server
                if (bDebug){ Debug.Log("[Notice] Received flag from server: INVALID_VERSION."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Incompatible client version; disconnecting from server...");
                }

                Disconnect();
                dataQueue.Dequeue();
            }
            else if (!isLoggedIn) {
                Debug.LogError("[Error] Received message from server, but cannot process it because client is not logged in."); 
                dataQueue.Dequeue();
                return;
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.QUEUE_MATCHMAKING) {
                isInMMQueue = true;
                if (bDebug){ Debug.Log("[Notice] Received flag from server: QUEUE_MATCHMAKING."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You have joined matchmaking queue.");
                }
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_MMQUEUE) {
                if (bDebug){ Debug.Log("[Notice] Received flag from server: FAILED_MMQUEUE."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You have failed to join matchmaking queue.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.MATCH_START) {
                MoveUpdateData playersData = JsonUtility.FromJson<MoveUpdateData>(dataQueue.Peek());
                isInMatch = true;

                //Receive lobby data
                foreach (PlayerMoveData player in playersData.players) {
                    //Update player position and orientation data, and health 
                    //Adds players to clientDataDict that aren't added yet
                    UpdatePlayer(player.username, player.position.x, player.position.y, player.position.z, player.orientation.yaw, player.orientation.pitch, player.health);
                }

                if (bDebug){ Debug.Log("[Notice] Client's game match has started."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] The match is starting...");
                }
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");

                if (!selectCtrlRef) {
                    if (GameObject.Find("SelectionController")) {
                        selectCtrlRef = GameObject.Find("SelectionController").GetComponent<SelectionController>();
                        selectCtrlRef.MatchmakingComplete();
                    }
                }
                else {
                    selectCtrlRef.MatchmakingComplete();
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.LEAVE_MATCHMAKING) {
                isInMMQueue = false;
                isInMatch = false;

                if (bDebug){ Debug.Log("[Notice] Client has left matchmaking queue."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You have left matchmaking queue.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FETCH_ACCOUNT) {
                if (bDebug){ Debug.Log("[Notice] Retrieved profile data."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Retrieved profile data.");
                }
                Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");

                if (!profileMgrRef) {
                    if (GameObject.Find("ProfileManager")) {
                        profileMgrRef = GameObject.Find("ProfileManager").GetComponent<ProfileMgr>();
                        profileMgrRef.SetProfile("default",1500,0,0,0,0,0,0,0,0,0,0); //TODO TEMP
                        clientUsername = profileMgrRef._Username;
                    }
                }
                else {
                    profileMgrRef.SetProfile("default",1500,0,0,0,0,0,0,0,0,0,0); //TODO TEMP
                    clientUsername = profileMgrRef._Username;
                }

                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.FAILED_FETCH) {
                if (bDebug){ Debug.Log("[Notice] Failed to retrieve profile data."); }

                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Failed to retrieve profile data.");
                }
                dataQueue.Dequeue();
            }
            else if (JsonUtility.FromJson<FlagNetMsg>(dataQueue.Peek()).flag == Flag.MATCH_UPDATE) {
                MoveUpdateData playersData = JsonUtility.FromJson<MoveUpdateData>(dataQueue.Peek());
                if (bDebug && bVerboseDebug){ Debug.Log("[Notice] Received Match Update."); }

                if (bDebug && bVerboseDebug) { Debug.Log("[TEMP TEST] Active Scene is '" + SceneManager.GetActiveScene().name + "'.");}

                //Receive lobby data
                foreach (PlayerMoveData player in playersData.players) {
                    //Update player position and orientation data, and health 
                    //Adds players to clientDataDict that aren't added yet
                    UpdatePlayer(player.username, player.position.x, player.position.y, player.position.z, player.orientation.yaw, player.orientation.pitch, player.health);
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

        public bool IsConnectedToServer() {
            return isConnected;
        }

        public void AttemptAccountCreation(string username, string password) {
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot create account; user is not connected to server.");
                }
                return;
            }

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
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot log in; user is not connected to server.");
                }
                return;
            }

            if (!profileMgrRef) {
                if (GameObject.Find("ProfileManager")) {
                    profileMgrRef = GameObject.Find("ProfileManager").GetComponent<ProfileMgr>();
                    profileMgrRef.SetProfile(username,1500,0,0,0,0,0,0,0,0,0,0); //TODO TEMP
                    clientUsername = username;
                }
            }
            else {
                profileMgrRef.SetProfile(username,1500,0,0,0,0,0,0,0,0,0,0); //TODO TEMP
                clientUsername = username;
            }

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

        public void QueueMatchMaking() {
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot queue matchmaking; user is not connected to server.");
                }
                return;
            }

            if (!isLoggedIn) {
                Debug.Log("[Notice] Client is not logged in; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot queue matchmaking; you are not logged in.");
                }
                return;
            }

            if (isInMatch) {
                Debug.Log("[Notice] Client is in a match; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot queue matchmaking; you are in an ongoing match.");
                }
                return;
            }

            SendFlagMessage(Flag.QUEUE_MATCHMAKING);
        }

        public void LeaveMatchMakingQueue() {
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot leave matchmaking; user is not connected to server.");
                }
                return;
            }

            if (!isLoggedIn) {
                Debug.Log("[Notice] Client is not logged in; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot leave matchmaking queue; you are not logged in.");
                }
                return;
            }

            if (isInMatch) {
                Debug.Log("[Notice] Client is in a match; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot leave matchmaking; you are in an ongoing match.");
                }
                return;
            }
            SendFlagMessage(Flag.LEAVE_MATCHMAKING);
        }

        public void RequestProfileData(string insertUsername){
            if (!isConnected) {
                Debug.Log("[Notice] User not connected to server; aborting operation.");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot leave matchmaking; user is not connected to server.");
                }
                return;
            }

            if (!isLoggedIn) {
                Debug.Log("[Notice] Client is not logged in; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] Cannot request profile data; you are not logged in.");
                }
                return;
            }

            string msgJson;
            ProfileDataRequest profileRequestMsg;
            Byte[] sendBytes;

            profileRequestMsg = new ProfileDataRequest(insertUsername);
            msgJson = JsonUtility.ToJson(profileRequestMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);
        }

        public void Disconnect() {
            if (!isConnected) {
                Debug.LogWarning("[Notice] You are not connected to a server; aborting operation...");
                if (consoleRef) {
                    consoleRef.UpdateChat("[Console] You are not connected to a server; aborting operation...");
                }
                return;
            }

            isInMMQueue = false;
            isInMatch = false;
            isLoggedIn = false;
            isConnected = false;
            udp.Dispose();
            showIfConnected = isConnected;
            Debug.Log("[Notice] Disconnected from server.");
            if (consoleRef) {
                consoleRef.UpdateChat("[Console] Disconnected from server.");
            }
        }

        public void AddPlayerCharRef(string getUsername, Transform charReference) {
            if (bDebug) { Debug.Log("[Notice] Adding player character reference to " + getUsername);}
            clientDataDict[getUsername].objReference = charReference;
        }

        public void UpdatePlayer(string getUsername, float getPosX, float getPosY, float getPosZ, float getYaw, float getPitch, int getHealth) {
            if (clientDataDict.ContainsKey(getUsername)) {
                clientDataDict[getUsername].position = new Vector3(getPosX, getPosY, getPosZ);
                clientDataDict[getUsername].yaw = getYaw;
                clientDataDict[getUsername].pitch = getPitch;
                clientDataDict[getUsername].health = getHealth;
            }
            else {

                if (bDebug && bVerboseDebug) { Debug.Log("[Notice] clientDataDict does not contain username key; adding new player to clientDataDict.");}

                PlayerData newLocalPlayer = new PlayerData();
                newLocalPlayer.position = new Vector3(getPosX, getPosY, getPosZ);
                newLocalPlayer.yaw = getYaw;
                newLocalPlayer.pitch = getPitch;
                newLocalPlayer.latency = 0;
                newLocalPlayer.health = getHealth;
                clientDataDict.Add(getUsername, newLocalPlayer);
            }
        }

        public void GameSceneStart(GameplayManager gameMgr) {

            if (bDebug) { Debug.Log("[Notice] Instantiating player prefabs...");}

            foreach (KeyValuePair<string, PlayerData> player in clientDataDict) {
                //Instantiate player prefabs
                if (player.Key == clientUsername) {
                    if (bDebug) { Debug.Log("[Notice] Instantiating local player prefab: " + player.Key);}
                    gameMgr.SpawnPlayer(player.Key, true);
                }
                else {
                    if (bDebug) { Debug.Log("[Notice] Instantiating remote player prefab: " + player.Key);}
                    gameMgr.SpawnPlayer(player.Key, false);
                }
            }

            if (bDebug) { Debug.Log("[Notice] Starting move update routine...");}
            StartCoroutine(MoveUpdateRoutine());
        }

        //Sends local client's position and orientation to server
        private void UpdateServerWithPosition() {
            if (!isInMatch) {
                Debug.LogError("[Error] Client is not in a match; cannot send movement update to server.");
                return;
            }

            if (!clientDataDict.ContainsKey(clientUsername)) {
                Debug.LogWarning("[Warning] Client is not in clientDataDict; cannot send movement update to server.");
                return;
            }

            if (bDebug && bVerboseDebug) { Debug.Log("[Notice] Sending client character move position to server...");}

            string msgJson;
            PlayerMoveData updateMsg;
            Byte[] sendBytes;

            updateMsg = new PlayerMoveData(clientUsername, clientDataDict[clientUsername].position.x, clientDataDict[clientUsername].position.y, clientDataDict[clientUsername].position.z, clientDataDict[clientUsername].yaw, clientDataDict[clientUsername].pitch, clientDataDict[clientUsername].health);

            msgJson = JsonUtility.ToJson(updateMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);

            Debug.Log("[Temp Debug] UpdateServerWithPosition() Sends local client's position and orientation to server:");
            Debug.Log("[Temp Debug] position: " + clientDataDict[clientUsername].position);
            Debug.Log("[Temp Debug] yaw: " + clientDataDict[clientUsername].yaw);
            Debug.Log("[Temp Debug] pitch: " + clientDataDict[clientUsername].pitch);
        }

        //Updates clientDataDict with local client's position and orientation
        private void UpdateClientWithPosition(Transform getObj) {
            if (!clientDataDict.ContainsKey(clientUsername)) {
                Debug.LogWarning("[Warning] Client is not in clientDataDict; cannot send movement update to client dictionary.");
                return;
            }

            clientDataDict[clientUsername].position = getObj.position;
            clientDataDict[clientUsername].yaw = getObj.eulerAngles.y;
            clientDataDict[clientUsername].pitch = getObj.eulerAngles.x;

            Debug.Log("[Temp Debug] UpdateClientWithPosition() Updates clientDataDict:");
            Debug.Log("[Temp Debug] position: " + getObj.position);
            Debug.Log("[Temp Debug] yaw: " + getObj.eulerAngles.y);
            Debug.Log("[Temp Debug] pitch: " + getObj.eulerAngles.x);
        }

        //Updates the character prefabs with new position and orientation
        private void UpdateAllRemoteClientsPos() {
            if (bDebug && bVerboseDebug) { Debug.Log("[Notice] Updating remote player's position...");}

            foreach (KeyValuePair<string, PlayerData> player in clientDataDict) {
                if (player.Key != clientUsername) {
                    if (player.Value.objReference) {
                        player.Value.objReference.position = player.Value.position;
                        player.Value.objReference.eulerAngles = new Vector3(player.Value.pitch, player.Value.yaw);

                        Debug.Log("[Temp Debug] UpdateAllRemoteClientsPos() Updates the character prefab:");
                        Debug.Log("[Temp Debug] username: " + player.Key);
                        Debug.Log("[Temp Debug] position: " + player.Value.objReference.position);
                        Debug.Log("[Temp Debug] eulerAngles: " + player.Value.objReference.eulerAngles);
                    }
                    else {
                        Debug.LogError("[Error] Player object reference missing; cannot update object reference.");
                    }
                }
            }
        }

        private void TriggerRemoteClientGunfire() {

        }

        IEnumerator MoveUpdateRoutine() {
            loopMoveUpdate = true;

            //yield return new WaitForSeconds(6.0f); //Delay before match start


            while (loopMoveUpdate) {
                yield return new WaitForSeconds(0.1f); //10 times a second

                if (clientDataDict.ContainsKey(clientUsername)) {
                    if (clientDataDict[clientUsername].objReference) {
                        UpdateClientWithPosition(clientDataDict[clientUsername].objReference); //Sends local client's position and orientation to server
                    }
                    else {
                        Debug.LogError("[Error] Client player reference missing; cannot update position.");
                    }
                }
                else {
                    Debug.LogWarning("[Warning] Client is not in clientDataDict; cannot update position.");
                }
            
                UpdateServerWithPosition(); //Updates clientDataDict with local client's position and orientation
                UpdateAllRemoteClientsPos(); //Updates the character prefabs with new position and orientation
            }
        }


    }

}
