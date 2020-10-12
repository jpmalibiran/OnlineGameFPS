/*
 * Author: Joseph Malibiran
 * Last Updated: October 11, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCharController {

    public class LocalClientInput : MonoBehaviour{

        [Header("Debug Settings")]
        [SerializeField] private bool bDebug = true;
        [SerializeField] private bool bVerboseDebug = false;

        [Header("Debug Read-Only")]
        [SerializeField] private Vector3 m_charMoveDirDebug;
        [SerializeField] private Vector3 m_charViewAngleDebug;
        [SerializeField] private float m_mouseInputXDebug;
        [SerializeField] private float m_mouseInputYDebug;
        [SerializeField] private sbyte m_forwardDebug;
        [SerializeField] private sbyte m_backwardDebug;
        [SerializeField] private sbyte m_leftwardDebug;
        [SerializeField] private sbyte m_rightwardDebug;

        [Header("References")]
        [SerializeField] private FirstPersonController m_charCtrlRef;
        [SerializeField] private Transform m_charCameraRef;
        [SerializeField] private Transform m_charBodyRef;

        [Header("Settings")]
        [SerializeField] private float m_CamSensVertical = 300;
        [SerializeField] private float m_CamSensHorizontal = 600;
        [SerializeField] private float m_CamClampVertical = 60;
        [SerializeField] private float m_MovementSpeed = 20;
        [SerializeField] private bool bUseDefaultKeybind = true;
        [SerializeField] private bool bUseFPSMouseCtrl = true;
        [SerializeField] private bool bUseFourDirectionalMovement = true;
        [SerializeField] private bool bLockCursorToCenter = true;

        private Dictionary<KeyCode, InputEvent> m_keybindings;

        private Vector3 m_charMoveDir = Vector3.zero;

        private float m_tempAngleHolderX = 0;
        private float m_tempAngleHolderY = 0;
        private float m_mouseInputX;
        private float m_mouseInputY;

        private sbyte m_forward = 0;
        private sbyte m_backward = 0;
        private sbyte m_leftward = 0;
        private sbyte m_rightward = 0;

        private void Awake() {
            m_keybindings = new Dictionary<KeyCode, InputEvent>();
        }

        private void Start() {
            DefaultKeybinds();

            if (bLockCursorToCenter) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void Update() {
            TrackMouseMovement();
            ListenOnUserInput();
            UpdateDirVector();
            UpdateCharMovement();
        }

        //Sets up default keybindings; WASD movement and left-click to shoot
        private void DefaultKeybinds() {
            if (!bUseDefaultKeybind) {
                return;
            }

            m_keybindings.Add(KeyCode.W, InputEvent.Forward);
            m_keybindings.Add(KeyCode.A, InputEvent.Leftward);
            m_keybindings.Add(KeyCode.S, InputEvent.Backward);
            m_keybindings.Add(KeyCode.D, InputEvent.Rightward);
            m_keybindings.Add(KeyCode.Mouse0, InputEvent.Shoot);
        }

        //Checks for user input and activates assigned events
        private void ListenOnUserInput() {
            //Check if specific keys were pressed down
            foreach(KeyValuePair<KeyCode, InputEvent> entry in m_keybindings){
                if (Input.GetKeyDown(entry.Key)) {
                    TriggerInputEvent(m_keybindings[entry.Key], true);
                }
            }

            //Check if specific keys were released
            foreach(KeyValuePair<KeyCode, InputEvent> entry in m_keybindings){
                if (Input.GetKeyUp(entry.Key)) {
                    TriggerInputEvent(m_keybindings[entry.Key], false);
                }
            }
        }

        //Allows a first person view set up where player camera is controlled by mouse movement
        private void TrackMouseMovement() {
            if (!bUseFPSMouseCtrl) {
                return;
            }
            if (!m_charCameraRef) {
                Debug.LogError("[Error] Missing reference to m_charCameraRef! Aborting operation...");
                return;
            }

            m_mouseInputX = Input.GetAxis("Mouse X");
            m_mouseInputY = Input.GetAxis("Mouse Y");

            m_tempAngleHolderX = m_charCameraRef.localEulerAngles.x - (m_mouseInputY * m_CamSensVertical * Time.deltaTime);

            //Custom clamp because Mathf.Clamp() is fucking things up. 
            //This is because when using euler angles, degrees below zero can be represented as a value subracted from 360. Thus, When the euler angle goes below zero it also exceeds the maximum clamp. 
            if (m_tempAngleHolderX > m_CamClampVertical && m_tempAngleHolderX <= 180) {
                m_tempAngleHolderX = m_CamClampVertical;
            }
            else if (m_tempAngleHolderX > 180 && m_tempAngleHolderX < (360 - m_CamClampVertical)) {
                m_tempAngleHolderX = (360 - m_CamClampVertical);
            }

            m_charCameraRef.localEulerAngles = new Vector3(m_tempAngleHolderX, m_charCameraRef.localEulerAngles.y, m_charCameraRef.localEulerAngles.z);
            m_tempAngleHolderY = m_charBodyRef.eulerAngles.y + (m_mouseInputX * m_CamSensHorizontal * Time.deltaTime);
            m_charBodyRef.eulerAngles = new Vector3(m_charBodyRef.eulerAngles.x, m_tempAngleHolderY, m_charBodyRef.eulerAngles.z);

            if (bDebug) {
                m_charViewAngleDebug.x = m_charCameraRef.localEulerAngles.x;
                m_charViewAngleDebug.y = m_charBodyRef.eulerAngles.y;
                m_charViewAngleDebug.z = 0;
                m_mouseInputXDebug = m_mouseInputX;
                m_mouseInputYDebug = m_mouseInputY;
            }

        }

        //Triggers the events assigned to key presses and/or key release
        private void TriggerInputEvent(InputEvent getEvent, bool isPressed) {
            switch (getEvent) {
                case InputEvent.Forward:
                    if (bUseFourDirectionalMovement) {
                        if (isPressed) {
                            m_forward = 1;
                        }
                        else if (!isPressed) {
                            m_forward = 0;
                        }
                    }
                    break;
                case InputEvent.Leftward:
                    if (bUseFourDirectionalMovement) {
                        if (isPressed) {
                            m_leftward = -1;
                        }
                        else if (!isPressed) {
                            m_leftward = 0;
                        }
                    }
                    break;
                case InputEvent.Backward:
                    if (bUseFourDirectionalMovement) {
                        if (isPressed) {
                            m_backward = -1;
                        }
                        else if (!isPressed) {
                            m_backward = 0;
                        }
                    }
                    break;
                case InputEvent.Rightward:
                    if (bUseFourDirectionalMovement) {
                        if (isPressed) {
                            m_rightward = 1;
                        }
                        else if (!isPressed) {
                            m_rightward = 0;
                        }
                    }
                    break;
                case InputEvent.Shoot:
                    if (m_charCtrlRef) {
                        if (isPressed) {
                            m_charCtrlRef.FireWeapon();
                        }
                        else if (!isPressed) {
                            
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //Calculate the directional vector such that pressing keys that denote motion in opposite directions yields no movement on that axis.
        //eg. if forward and backward keys are pressed, it simply goes neither forward nor backward in the local z axis.
        private void UpdateDirVector() {

            m_charMoveDir = new Vector3((float)(m_leftward + m_rightward), 0, (float)(m_forward + m_backward));

            if (bDebug) {
                m_forwardDebug = m_forward;
                m_backwardDebug = m_backward;
                m_leftwardDebug = m_leftward;
                m_rightwardDebug = m_rightward;
                m_charMoveDirDebug = m_charMoveDir;
            }
            
        }

        private void UpdateCharMovement() {
            if (!m_charCtrlRef) {
                Debug.LogError("[Error] Mising reference to m_charCtrlRef! Aborting oepration...");
                return;
            }

            m_charCtrlRef.UpdateMoveVector(m_charMoveDir * m_MovementSpeed * Time.deltaTime);
        }

        //Allows one to set custom keybindings
        public void SetKeybind(KeyCode getKey, InputEvent getEvent) {
            //TODO Incomplete
        }
    }

}


