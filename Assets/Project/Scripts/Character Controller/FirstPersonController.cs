/*
 * Author: Joseph Malibiran
 * Last Updated: October 11, 2020
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

        private void Update() {
            MoveUpdate();
        }

        private void MoveUpdate() {
            if (!bAllowMoveUpdate) {
                return;
            }

            //Not a new movement vector direction, therefore no need to update
            if (m_PrevMoveDirection == m_moveDirection) {
                return;
            }

            m_PrevMoveDirection = m_moveDirection;
            m_rbRef.MovePosition(m_moveDirection);
            
        }

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

            //if (Physics.Raycast(mousePosition, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity)){
            if (Physics.Raycast(mousePosition, Camera.main.transform.TransformDirection(Vector3.forward), out hit)){
                //Debug.DrawRay(mousePosition, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                
                if (hit.transform.tag == "Player") {
                    if (bVerboseDebug) { Debug.Log("[Notice] Player hit!"); }
                    newHitData = new HitData(true, 0, 0, 1, hit.distance);
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

        public void UpdateMoveVector(Vector3 getVector) {

            m_moveDirection = this.transform.TransformPoint(getVector);
        }

        //TODO move rotation calculation here from LocalClientInput
        public void UpdateFPSView(Vector3 getWorldBodyRotation, Vector3 getLocalCamRotation) {
            m_worldBodyRef.eulerAngles = getWorldBodyRotation;
            m_localCamRef.localEulerAngles = getLocalCamRotation;
        }

        //TODO do a proper grounded check
        public bool IsGrounded() {
            return bGrounded;
        }

        public void CommenceJump(float getForce) {
            if (!bGrounded) {
                return;
            }
            bGrounded = false;
            StartCoroutine(JumpRoutine(getForce));
        }

        IEnumerator DelayMoveUpdate() {
            yield return new WaitForSeconds(0.6f);
            bAllowMoveUpdate = true;
        }

        IEnumerator JumpRoutine(float getForce) {
            Debug.Log("Here");
            m_rbRef.AddForce(Vector3.up * getForce, ForceMode.Impulse);
            yield return new WaitForSeconds(0.6f);
            bGrounded = true;
        }
    }
}
