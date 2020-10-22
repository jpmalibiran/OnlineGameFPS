/*
 * Author: Joseph Malibiran
 * Last Updated: October 21, 2020
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

        [SerializeField] private string m_localUsername = "default";
        [SerializeField] private int m_maxLines = 12;

        private Queue<string> msgQueue = new Queue<string>();

        private void Start() {
            m_localUsername = "user" + Random.Range(1, 9999).ToString();
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

            if (getMsg[0] != '/') { //Only process commands with '/' prefix
                return false;
            }

            if (getMsg == "/connect") {
                if (netManagerRef) {
                    UpdateChat("[Console] Connecting to server... ");
                    netManagerRef.ConnectToServer();
                }
                else {
                    Debug.LogError("[Error] Network Manager reference missing! Aborting operation... ");
                    if (bDebugToConsole) { UpdateChat("[Error] Network Manager reference missing! Aborting operation... ");}
                }
            }
            else if (getMsg == "/join any" || getMsg == "/join matchmaking") {
                if (netManagerRef) {
                    UpdateChat("[Console] Joining matchmaking... ");
                    netManagerRef.CommenceMatchmaking();
                }
                else {
                    Debug.LogError("[Error] Network Manager reference missing! Aborting operation... ");
                    if (bDebugToConsole) { UpdateChat("[Error] Network Manager reference missing! Aborting operation... ");}
                }
            }
            else if (getMsg.Substring(0,5) == "/join ") { //read first 6 letters, check if it is a join command

                switch (getMsg.Substring(6)) {
                    case "1":

                        break;
                    case "2":

                        break;
                    case "3":

                        break;
                    case "4":

                        break;
                    default:
                        break;
                }
            }
            else {
                Debug.Log("[Notice] Invalid Command. ");
                UpdateChat("[Console] Invalid Command. ");
            }
            
            return true;
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
