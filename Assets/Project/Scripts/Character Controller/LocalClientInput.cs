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
        [SerializeField] private float m_showDeltaTime;
        [SerializeField] private sbyte m_forwardDebug;
        [SerializeField] private sbyte m_backwardDebug;
        [SerializeField] private sbyte m_leftwardDebug;
        [SerializeField] private sbyte m_rightwardDebug;

        [Header("References")]
        [SerializeField] private FirstPersonController m_charCtrlRef;
        [SerializeField] private AmmoController m_ammoCtrlRef;
        [SerializeField] private GameObject m_profileRef;
        [SerializeField] private Transform m_charCamContainerRef; //TODO Hide in inspector
        [SerializeField] private Transform m_charBodyRef; //TODO Hide in inspector
        [SerializeField] private Transform m_muzzleFlashUIRef;
        [SerializeField] private Transform m_charMainCamRef;

        [Header("Settings")]
        [SerializeField] private float m_CamSensVertical = 300;
        [SerializeField] private float m_CamSensHorizontal = 600;
        [SerializeField] private float m_CamClampVertical = 60;
        [SerializeField] private float m_MovementSpeed = 10;
        [SerializeField] private float m_JumpForce = 1;
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

        private bool bCanFire = true;

        private void Awake() {
            m_keybindings = new Dictionary<KeyCode, InputEvent>();

            if (!m_charBodyRef && m_charCtrlRef) {
                m_charBodyRef = m_charCtrlRef.transform;
            }

            if (!m_charCamContainerRef && m_charCtrlRef) {
                m_charCamContainerRef = m_charCtrlRef.transform.GetChild(0);
            }
        }

        private void Start() {
            DefaultKeybinds();

            if (bLockCursorToCenter) {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (m_charMainCamRef && m_charCamContainerRef) {
                m_charMainCamRef.SetParent(m_charCamContainerRef);
                m_charMainCamRef.localPosition = Vector3.zero;
            }

            if (m_charCtrlRef && m_ammoCtrlRef) {
                m_charCtrlRef.m_ammoCtrlRef = this.m_ammoCtrlRef;
            }

            if (m_profileRef) {
                m_profileRef.SetActive(false);
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
            m_keybindings.Add(KeyCode.R, InputEvent.Reload);
            m_keybindings.Add(KeyCode.Y, InputEvent.Profile);
            m_keybindings.Add(KeyCode.Space, InputEvent.Jump);
            m_keybindings.Add(KeyCode.Mouse0, InputEvent.Shoot);
        }

        //Checks for user input and activates assigned events
        private void ListenOnUserInput() {
            //Check if specific keys were pressed down
            foreach(KeyValuePair<KeyCode, InputEvent> entry in m_keybindings){
                if (Input.GetKeyDown(entry.Key)) {
                    TriggerInputEvent(m_keybindings[entry.Key], true);
                    if (bVerboseDebug) { Debug.Log("[Notice] Key pressed: " + entry.Key); }
                }
            }

            //Check if specific keys were released
            foreach(KeyValuePair<KeyCode, InputEvent> entry in m_keybindings){
                if (Input.GetKeyUp(entry.Key)) {
                    TriggerInputEvent(m_keybindings[entry.Key], false);
                    if (bVerboseDebug) { Debug.Log("[Notice] Key released: " + entry.Key); }
                }
            }
        }

        //Allows a first person view set up where player camera is controlled by mouse movement
        private void TrackMouseMovement() {
            if (!bUseFPSMouseCtrl) {
                return;
            }
            if (!m_charCamContainerRef) {
                Debug.LogError("[Error] Missing reference to m_charCameraRef! Aborting operation...");
                return;
            }

            m_mouseInputX = Input.GetAxis("Mouse X");
            m_mouseInputY = Input.GetAxis("Mouse Y");

            

            m_tempAngleHolderX = m_charCamContainerRef.localEulerAngles.x - (m_mouseInputY * m_CamSensVertical * Time.deltaTime);

            //Custom clamp because Mathf.Clamp() is fucking things up. 
            //This is because when using euler angles, degrees below zero can be represented as a value subracted from 360. Thus, When the euler angle goes below zero it also exceeds the maximum clamp. 
            if (m_tempAngleHolderX > m_CamClampVertical && m_tempAngleHolderX <= 180) {
                m_tempAngleHolderX = m_CamClampVertical;
            }
            else if (m_tempAngleHolderX > 180 && m_tempAngleHolderX < (360 - m_CamClampVertical)) {
                m_tempAngleHolderX = (360 - m_CamClampVertical);
            }
            else if (m_tempAngleHolderX < -1) {
                m_tempAngleHolderX = 0;
            }
            m_charCamContainerRef.localEulerAngles = new Vector3(m_tempAngleHolderX, m_charCamContainerRef.localEulerAngles.y, m_charCamContainerRef.localEulerAngles.z);

            m_tempAngleHolderY = m_charBodyRef.eulerAngles.y + (m_mouseInputX * m_CamSensHorizontal * Time.deltaTime);
            m_charBodyRef.eulerAngles = new Vector3(m_charBodyRef.eulerAngles.x, m_tempAngleHolderY, m_charBodyRef.eulerAngles.z);

            if (bDebug) {
                m_charViewAngleDebug.x = m_charCamContainerRef.localEulerAngles.x;
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
                    if (isPressed) {
                        if (m_charCtrlRef && bCanFire) {
                            if (m_ammoCtrlRef) {
                                if (m_ammoCtrlRef.GetAmmoAmount() > 0) {
                                    StartCoroutine(Gunfire());
                                }
                                //Doesn't fire without ammo
                            }
                            else { //Fires anyway if m_ammoCtrlRef missing
                                StartCoroutine(Gunfire());
                            }
                        }
                    }
                    else if (!isPressed) {
                            
                    }
                    break;
                case InputEvent.Reload:
                    if (isPressed) {

                    }
                    else if (!isPressed) {

                    }
                    break;
                case InputEvent.Jump:
                    if (isPressed) {
                        if (m_charCtrlRef){
                            m_charCtrlRef.CommenceJump(m_JumpForce);
                        }
                    }
                    else if (!isPressed) {
                            
                    }
                    break;
                case InputEvent.Profile:
                    if (isPressed) {
                        if (m_profileRef) {
                            m_profileRef.SetActive(true);
                        }
                    }
                    else if (!isPressed) {
                        if (m_profileRef) {
                            m_profileRef.SetActive(false);
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
                Debug.LogError("[Error] Missing reference to m_charCtrlRef! Aborting operation...");
                return;
            }

            m_charCtrlRef.UpdateMoveVector(m_charMoveDir.normalized, m_MovementSpeed);

        }

        //Allows you to set custom keybindings
        public void SetKeybind(KeyCode getKey, InputEvent getEvent) {
            //TODO Incomplete
        }

        IEnumerator Gunfire() {

            bCanFire = false;
            if (m_charCtrlRef) {
                m_charCtrlRef.FireWeapon();
            }
            if (m_muzzleFlashUIRef) {
                m_muzzleFlashUIRef.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.04f);

            if (m_muzzleFlashUIRef) {
                m_muzzleFlashUIRef.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.3f);
            bCanFire = true;
        }

    }

}


