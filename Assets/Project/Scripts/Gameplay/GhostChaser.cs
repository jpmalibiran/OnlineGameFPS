using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostChaser : MonoBehaviour{

    [Header("References")]
    [SerializeField] private Transform targetGhost;

    [Header("Settings")]
    [SerializeField] private float speed = 1;
    private bool isFollowingGhost = true;

    private void Update() {
        FollowGhost();
    }

    private void FollowGhost() {
        if (targetGhost && isFollowingGhost) {
            transform.position = Vector3.MoveTowards(this.transform.position, targetGhost.position, speed * Time.deltaTime);
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
