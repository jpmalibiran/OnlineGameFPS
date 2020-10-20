/*
 * Author: Joseph Malibiran
 * Last Updated: October 8, 2020
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

        [Header("UDP Settings")] //These values must be filled in the inspector
        [SerializeField] private string remoteEndpointAddress;
        [SerializeField] private int remoteEndpointPort;

        private Dictionary<int, Transform> playerReferences; //Contains references of gameObjects as Transform representing the characters of clients connected to an online match
        private Queue<string> dataQueue; //data received from the server is queued so they are only processed once chronologically

        // Refers to *this* client's address
        private string clientIP;
        private string clientPort;

        private bool isConnected = false;

        private void Awake() {
            dataQueue = new Queue<string>();
        }

        // Start is called before the first frame update
        void Start() {
            //NetworkSetUp();
        }

        void OnDestroy() {
            if (bDebug){ Debug.Log("[Notice] Cleaning up UDP Client...");}
            udp.Dispose();
            dataQueue.Clear();
        }

        void FixedUpdate() {
            ProcessServerMessages();
        }

        //Receives messages from the server
        void OnReceived(IAsyncResult result) {
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
            FlagNetworkMsg connectFlagMsg; //Note: FlagNetworkMsg is a class that only has a flag enum property. The term 'flag' in this context identifies the type of a network message.

            if (bDebug){ Debug.Log("[Notice] Setting up client...");}

            udp = new UdpClient();
            playerReferences = new Dictionary<int, Transform>();
            dataQueue = new Queue<string>();
            connectFlagMsg = new FlagNetworkMsg(flag.CONNECT); 

            if (bDebug){ Debug.Log("[Notice] Client connecting to Server...");}

            //Attempt to connect to server
            try{
                udp.Connect(remoteEndpointAddress,remoteEndpointPort);
            }
            catch (Exception e ) {
                Debug.LogWarning("[Warning] Failed to connect to server. " + e.ToString() + " Aborting operation...");
                return;
            }
            isConnected = true;
            showIfConnected = isConnected;

            //Send flag 'connect' to server
            msgJson = JsonUtility.ToJson(connectFlagMsg); 
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);

            if (bDebug){ Debug.Log("[Notice] Client connection established with " + udp.Client.RemoteEndPoint.ToString() + ".");}
            if (bDebug){ Debug.Log("[Notice] Client is now listening for server messages...");}

            //Allow the client to receive network messages from the server
            udp.BeginReceive(new AsyncCallback(OnReceived), udp);

            //if (bDebug){ Debug.Log("[Notice] Routinely pinging server...");}

            //InvokeRepeating("RoutinePing", 1, 1); // Sends 1 ping message to server every 1 second.
        }

        //Sends a 'pong' message to the server
        private void ResponsePong() {
            if (!isConnected) {
                return;
            }

            string msgJson;
            FlagNetworkMsg pingFlagMsg;
            Byte[] sendBytes;

            if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Sending flag to server: pong"); }

            //Send flag 'ping' to server
            pingFlagMsg = new FlagNetworkMsg(flag.PONG);
            msgJson = JsonUtility.ToJson(pingFlagMsg);
            sendBytes = Encoding.ASCII.GetBytes(msgJson);
            udp.Send(sendBytes, sendBytes.Length);

        }

        //Process the network messages sent by the server. We stored them in a queue during OnReceived(). TODO incomplete
        private void ProcessServerMessages() {
            //Only process this function if there is data in queue
            if (dataQueue.Count <= 0) {
                return;
            }

            if (JsonUtility.FromJson<FlagNetworkMsg>(dataQueue.Peek()).flag == flag.PING) { //Received a ping from the server
                if (bDebug && bVerboseDebug){ Debug.Log("[Routine] Received flag from server: ping"); }
                ResponsePong(); //Send a response pong message
                dataQueue.Dequeue();
            }

        }

        public void ConnectToServer() {
            NetworkSetUp();
        }
    }

}
