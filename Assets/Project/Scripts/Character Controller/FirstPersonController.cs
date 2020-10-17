/*
 * Author: Joseph Malibiran
 * Last Updated: October 15, 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCharController {

    [RequireComponent(typeof(Rigidbody))]
    public class FirstPersonController : MonoBehaviour{

        [Header("Debug Settings")]
        [SerializeField] private bool bDebug = true;
        [SerializeField] private bool bVerboseDebug = false;

        [Header("Debug Read-Only")]
        [SerializeField] private Vector3 m_showMoveDir;

        [Header("References")]
        [SerializeField] private AudioSource m_audioSrcRef;
        [SerializeField] public AmmoController m_ammoCtrlRef; //TODO revert to private later
        [SerializeField] private Transform m_worldBodyRef;
        [SerializeField] private Transform m_localCamRef;
        [SerializeField] private Camera m_mainCamRef; //TODO hide in inspector and use this instead of Camera.main!

        private Rigidbody m_rbRef;
        private Vector3 m_moveDirection = Vector3.zero;
        private Vector3 m_PrevMoveDirection = Vector3.zero;
        private RaycastHit hit;
        private Ray ray;

        private bool bAllowMoveUpdate = false;
        private bool bGrounded = true;

        private void Start() {

            if (GetComponent<Rigidbody>()) {
                m_rbRef = GetComponent<Rigidbody>();
            }
            if (!m_rbRef) {
                this.gameObject.AddComponent<Rigidbody>();
            }

            StartCoroutine(DelayMoveUpdate());
        }

        //Fixed update is in line with physics
        private void FixedUpdate() {
            MoveUpdate();
        }

        //Moves the player character
        private void MoveUpdate() {
            //Allows for delays before player can move their character. 
            if (!bAllowMoveUpdate) {
                return;
            }

            //Zero vector implies no movement, thus no need to apply movement to rigidbody
            if (m_moveDirection == Vector3.zero) {
                return;
            }

            //Apply movement towards a direction. Note: speed is applied in UpdateMoveVector(), but it was decided in LocalClientInput.cs
            m_rbRef.MovePosition(this.transform.position + m_moveDirection * Time.fixedDeltaTime);
            
        }

        //Updates the movement vector of this character. This is updated by LocalClientINput.cs
        public void UpdateMoveVector(Vector3 getVector, float getSpeed) {

            m_moveDirection = m_worldBodyRef.TransformDirection(getVector).normalized * getSpeed;
            m_showMoveDir = m_moveDirection;
        }

        //TODO move rotation calculation here from LocalClientInput
        public void UpdateFPSView(float getYaw, float getPitch, float getPitchClamp) {
            float pitchCalc;
            float yawCalc;

            pitchCalc = m_localCamRef.localEulerAngles.x - (getPitch * Time.deltaTime);

            //This is a custom clamp because Mathf.Clamp() is fucking things up. 
            //The issue was: when using euler angles, degrees below zero can also be represented as a value subracted from 360. Thus, When the euler angle goes below zero it also exceeds the maximum clamp. 
            //The solution below basically checks if the pitch euler angle does into an angle it's not supposed to be, then it moves it to the upper or lower bound of the clamp, whichever is closer.

            //Calculate and apply pitch rotation (rotation upon the local x axis)
            if (pitchCalc > getPitchClamp && pitchCalc <= 180) {
                pitchCalc = getPitchClamp;
            }
            else if (pitchCalc > 180 && pitchCalc < (360 - getPitchClamp)) {
                pitchCalc = (360 - getPitchClamp);
            }
            if (pitchCalc < -1) {
                pitchCalc = 0;
            }
            m_localCamRef.localEulerAngles = new Vector3(pitchCalc, m_localCamRef.localEulerAngles.y, m_localCamRef.localEulerAngles.z); //Apply pitch euler angle to camera x axis

            yawCalc = m_worldBodyRef.eulerAngles.y + (getYaw * Time.deltaTime);
            m_worldBodyRef.eulerAngles = new Vector3(m_worldBodyRef.eulerAngles.x, yawCalc, m_worldBodyRef.eulerAngles.z); //Apply yaw euler angle to body y axis

        }

        //TODO do a proper grounded check
        public bool IsGrounded() {
            return bGrounded;
        }

        //Activates a jump via physics impulse. Right now it doesn't check if the player is grounded. Right now it just has a cooldown.
        public void CommenceJump(float getForce) {
            if (!bGrounded) {
                return;
            }
            bGrounded = false;
            StartCoroutine(JumpRoutine(getForce));
        }

        //Does a raycast from the center of the screen directly outward. Plays audio, subtracts ammo, check if the raycast hits any object colliders.
        //Note: UI layer muzzle flash and gunfire cooldown is handled in LocalClientInput.cs
        public HitData FireWeapon() {
            Vector3 mousePosition;
            HitData newHitData;

            if (m_audioSrcRef) {
                m_audioSrcRef.Play();
            }

            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

            if (m_ammoCtrlRef) {
                m_ammoCtrlRef.Fired(); //Ticks down ammo count
            }

            //int layerMask = 1 << 5;
            //layerMask = ~layerMask;

            if (Physics.Raycast(mousePosition, Camera.main.transform.TransformDirection(Vector3.forward), out hit)){
                //Debug.DrawRay(mousePosition, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                
                if (hit.transform.tag == "Player") {
                    if (bVerboseDebug) { Debug.Log("[Notice] Player hit!"); }
                    newHitData = new HitData(true, 0, 0, 1, hit.distance);
                    hit.transform.gameObject.SendMessage("GotHit", 20);
                }
                else {
                    if (bVerboseDebug) { Debug.Log("[Notice] Object hit!"); }
                    newHitData = new HitData(false, 0, 0, 1, hit.distance);
                }
            }
            else{
                //Debug.DrawRay(mousePosition, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance * 1000, Color.yellow);
                if (bVerboseDebug) { Debug.Log("[Notice] No Objects hit."); }

                newHitData = new HitData(false, 0, 0, 1, hit.distance);
            }

            return newHitData; //TODO make this useful
        }

        //Delay before movement can occur
        IEnumerator DelayMoveUpdate() {
            yield return new WaitForSeconds(0.6f);
            bAllowMoveUpdate = true;
        }

        //Jump cooldown
        IEnumerator JumpRoutine(float getForce) {
            m_rbRef.AddForce(Vector3.up * getForce, ForceMode.Impulse);
            yield return new WaitForSeconds(0.6f);
            bGrounded = true;
        }
    }
}
