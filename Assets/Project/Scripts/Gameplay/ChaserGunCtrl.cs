using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserGunCtrl : MonoBehaviour{
    [SerializeField] private GameObject remoteBulletPrefab;
    [SerializeField] private Transform muzzleFlashRef;
    [SerializeField] private Transform mockGunRef;
    [SerializeField] private AudioSource audioSrcRef;
    //[SerializeField] private LineRenderer lineRenderRef;

    private RaycastHit hit;
    private Ray ray;

    public void PerformGunfire() {

        if (!muzzleFlashRef) {
            muzzleFlashRef = this.transform.GetChild(0);
            if (!muzzleFlashRef) {
                Debug.LogError("[Error] ");
                return;
            }
        }

        if (!mockGunRef) {
            mockGunRef = this.transform.GetChild(0).GetChild(0);
            if (!mockGunRef) {
                Debug.LogError("[Error] ");
                return;
            }
        }

        if (!audioSrcRef) {
            Debug.LogError("[Error] ");
            return;
        }

        StartCoroutine(Gunfire());
    }

    IEnumerator Gunfire() {
        LineRenderer lineRenderRef;
        lineRenderRef = Instantiate(remoteBulletPrefab, mockGunRef.transform.position, mockGunRef.transform.rotation).GetComponent<LineRenderer>();
        lineRenderRef.useWorldSpace = false;

        if (Physics.Raycast(mockGunRef.position, Vector3.forward, out hit)){
            lineRenderRef.SetPosition(0, Vector3.zero);
            lineRenderRef.SetPosition(1, new Vector3 (0,0,Vector3.Distance(mockGunRef.transform.position, hit.transform.position)));
        }
        else {
            lineRenderRef.SetPosition(0, Vector3.zero);
            lineRenderRef.SetPosition(1, new Vector3 (0,0,20));
        }

        audioSrcRef.Play();

        muzzleFlashRef.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.04f);

        muzzleFlashRef.gameObject.SetActive(false);

    }

}
