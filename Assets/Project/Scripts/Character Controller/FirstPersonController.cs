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

        private Rigidbody m_rbRef;
<<<<<<< Updated upstream
        private Vector3 m_moveDirection;
=======
        private Vector3 m_moveDirection = Vector3.zero;
        private Vector3 m_PrevMoveDirection = Vector3.zero;
        private RaycastHit hit;
        private Ray ray;

        private bool bAllowMoveUpdate = false;
        private bool bGrounded = true;
       
>>>>>>> Stashed changes

        private void Start() {

            if (GetComponent<Rigidbody>()) {
                m_rbRef = GetComponent<Rigidbody>();
            }
            if (!m_rbRef) {
                this.gameObject.AddComponent<Rigidbody>();
            }
        }

        private void Update() {
            MoveUpdate();
        }

        private void MoveUpdate() {
            
            m_rbRef.MovePosition(m_moveDirection);
        }

        public void UpdateMoveVector(Vector3 getVector) {

            m_moveDirection = this.transform.TransformPoint(getVector);
        }
    }
}
