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
        private Vector3 m_moveDirection;

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
