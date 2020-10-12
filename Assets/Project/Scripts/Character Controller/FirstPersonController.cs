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

        [Header("References")]
        [SerializeField] private Transform m_worldBodyRef;
        [SerializeField] private Transform m_localCamRef;

        private Rigidbody m_rbRef;
        private Vector3 m_moveDirection = Vector3.zero;
        private Vector3 m_PrevMoveDirection = Vector3.zero;
        private RaycastHit hit;
        private Ray ray;

        private bool bAllowMoveUpdate = false;

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

        public void FireWeapon() {
            Vector3 mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
            int layerMask = 1 << 5;
            layerMask = ~layerMask;

            if (Physics.Raycast(mousePosition, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask)){
                Debug.DrawRay(mousePosition, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log("Did Hit");
            }
            else{
                Debug.DrawRay(mousePosition, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow);
                Debug.Log("Did not Hit");
            }

        }

        public void UpdateMoveVector(Vector3 getVector) {

            m_moveDirection = this.transform.TransformPoint(getVector);
        }

        IEnumerator DelayMoveUpdate() {
            yield return new WaitForSeconds(0.4f);
            bAllowMoveUpdate = true;
        }
    }
}
