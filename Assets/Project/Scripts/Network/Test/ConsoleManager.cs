/*
 * Author: Joseph Malibiran
 * Last Updated: December 2, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FPSNetworkCode {

    //This is a test script to test network code
    public class ConsoleManager : MonoBehaviour{

        [SerializeField] private bool bDebug = true;
        [SerializeField] private bool bDebugToConsole = false;

        [SerializeField] private NetworkManager netManagerRef;
        [SerializeField] private Text m_messageFeedRef;
        [SerializeField] private InputField m_inputFieldRef;

        [SerializeField] private string m_localUsername = "default"; //TODO remove
        [SerializeField] private int m_maxLines = 12;

        private Queue<string> msgQueue = new Queue<string>();

        private void Start() {
            m_localUsername = "user" + Random.Range(1, 9999).ToString(); //TODO remove
            ClearLocalChatBox();
        }

        //Updates local chat box with the given string argument
        public void UpdateChat(string getMsg) {
        
            string msgBoxMessageFull = "";

            if (bDebug) { Debug.Log("[Debug] Adding new message to chat message queue."); }
            if (bDebugToConsole) { UpdateChat("[Debug] Adding new message to chat message queue.");}
            msgQueue.Enqueue(getMsg);

            if (msgQueue.Count > m_maxLines) {
                msgQueue.Dequeue();
            }

            foreach (string message in msgQueue){
                msgBoxMessageFull = msgBoxMessageFull + "\n" + message;
            }

            m_messageFeedRef.text = msgBoxMessageFull;
            if (bDebug) { Debug.Log(getMsg); }
            if (bDebugToConsole) { UpdateChat(getMsg);}
        }

        //Called when local user presses enter on the text field referenced in m_inputFieldRef. 
        public void NewChatMessage(string getMsg) {

            string newMessage;

            if (!m_inputFieldRef) {
                Debug.LogError("[Error] Missing InputField reference! This function is called from a different location.");
                return;
            }

            if (m_inputFieldRef.text == "") {
                if (bDebug) { Debug.Log("[Debug] There is nothing in the chat input field; message sending operation is not executed."); }
                if (bDebugToConsole) { UpdateChat("[Debug] There is nothing in the chat input field; message sending operation is not executed.");}

                return;
            }

            newMessage = "[" + m_localUsername + "] " + getMsg;

            if (bDebug) { Debug.Log("[Debug] You have created a new message. "); }
            if (bDebugToConsole) { UpdateChat("[Debug] You have created a new message. ");}
            
            if (!CommandReader(getMsg)) {
                UpdateChat(newMessage);
            }
            ClearLocalChatBoxInputField();
        }

        public bool CommandReader(string getMsg) {

            string usernameHolder = "";
            string passwordHolder = ""; //temp plain text
            int passwordStartIndex = -1;

            if (getMsg[0] != '/') { //Only process commands with '/' prefix
                return false;
            }

            if (!netManagerRef) {
                Debug.LogError("[Error] Network Manager reference missing! Aborting operation... ");
                if (bDebugToConsole) { UpdateChat("[Error] Network Manager reference missing! Aborting operation... ");}
                return false;
            }

            if (getMsg == "/connect") {
                UpdateChat("[Console] Connecting to server... ");
                netManagerRef.ConnectToServer();
                return true;
            }

            if (getMsg.Length > 7) {
                if (getMsg.Substring(1,6).ToLower() == "login ") {
                    //Get username from syntax. its the first word after '/login '
                    for (int i = 7; i < getMsg.Length; i++) {
                        if (getMsg[i] == ' ') {
                            passwordStartIndex = i + 1;
                            break;
                        }
                        usernameHolder = usernameHolder + getMsg[i];
                    }

                    if (passwordStartIndex == -1 || passwordStartIndex > getMsg.Length) {
                        UpdateChat("[Console] Invalid syntax.");
                        return false;
                    }

                    //Get password from syntax. its the word after the username
                    for (int i = passwordStartIndex; i < getMsg.Length; i++) {
                        passwordHolder = passwordHolder + getMsg[i];
                    }

                    netManagerRef.AttemptLogin(usernameHolder, passwordHolder);
                    return true;
                }
            }

            if (getMsg.Length > 9){
                if (getMsg.Substring(1,9).ToLower() == "register ") {
                    //Get username from syntax. its the first word after '/login '
                    for (int i = 10; i < getMsg.Length; i++) {
                        if (getMsg[i] == ' ') {
                            passwordStartIndex = i + 1;
                            break;
                        }
                        usernameHolder = usernameHolder + getMsg[i];
                    }

                    if (passwordStartIndex == -1 || passwordStartIndex >= getMsg.Length) {
                        UpdateChat("[Console] Invalid syntax.");
                        return false;
                    }

                    //Get password from syntax. its the word after the username
                    for (int i = passwordStartIndex; i < getMsg.Length; i++) {
                        passwordHolder = passwordHolder + getMsg[i];
                    }

                    netManagerRef.AttemptAccountCreation(usernameHolder, passwordHolder);
                    return true;
                }
            }

            if (getMsg.Length > 7){
                if (getMsg.Substring(1,6).ToLower() == "fetch ") {
                    if (getMsg.Substring(7,14).ToLower() == "profile ") {
                        for (int i = 15; i < getMsg.Length; i++) {
                            if (getMsg[i] == ' ') {
                                passwordStartIndex = i + 1;
                                break;
                            }
                            usernameHolder = usernameHolder + getMsg[i];
                        }
                        netManagerRef.RequestProfileData(usernameHolder);
                    }
                    else if (getMsg.Substring(7,14).ToLower() == "clientdata ") {
                        //TODO
                    }
                    return true;
                }
            }

            if (getMsg == "/join any" || getMsg == "/join matchmaking" || getMsg == "/join") {
                UpdateChat("[Console] Joining matchmaking... ");
                netManagerRef.QueueMatchMaking();
                return true;
            }

            if (getMsg == "/leave" || getMsg == "/leave lobby") {
                netManagerRef.LeaveMatchMakingQueue();
                return true;
            }

            if (getMsg == "/disconnect") {
                netManagerRef.Disconnect();
                return true;
            }

            Debug.Log("[Notice] Invalid Command. ");
            UpdateChat("[Console] Invalid Command. ");
            return false;
        }

        public void ClearLocalChatBox() {
            msgQueue.Clear();
            m_messageFeedRef.text = "";
        }

        public void ClearLocalChatBoxInputField() {
            m_inputFieldRef.text = "";
        }

    }

}
