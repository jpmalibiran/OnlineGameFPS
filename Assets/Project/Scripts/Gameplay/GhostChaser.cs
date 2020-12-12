using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostChaser : MonoBehaviour{

    [Header("References")]
    [SerializeField] private Transform targetGhost;

    [Header("Settings")]
    //[SerializeField] private float speed = 1;
    [SerializeField] private float smoothTime = 0.1F;
    
    private Vector3 velocity = Vector3.zero;
    private bool isFollowingGhost = true;

    private void Update() {
        FollowGhost();
    }

    private void FollowGhost() {
        if (targetGhost && isFollowingGhost) {
            //transform.position = Vector3.MoveTowards(this.transform.position, targetGhost.position, speed * Time.deltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, targetGhost.transform.position, ref velocity, smoothTime);
        }
        else {
            Debug.LogWarning("[Warning] Remote prefab missing ghost reference; cannot follow ghost.");
        }
    }

    public void AssignGhost(Transform insertGhostRef) {
        targetGhost = insertGhostRef;
    }

    public void SetFollow(bool set) {
        isFollowingGhost = set;
    }
}
