using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour{

    [Header("Debug Settings")] //Helps in enabling and disabling debug messages from this script should a programmer want to focus on other console messages.
    [SerializeField] private bool bDebug = true;
    [SerializeField] private bool bVerboseDebug = false;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab; 
    [SerializeField] private GameObject remotePlayerPrefab; 
    [SerializeField] private GameObject remotePlayerGhostPrefab; 

    [SerializeField] private FPSCharController.LocalClientInput inputCtrlRef;

    private FPSNetworkCode.NetworkManager netManager;
    private ProfileMgr profileManager;

    // Start is called before the first frame update
    void Start(){
        if (GameObject.Find("NetworkManager")) {
            netManager = GameObject.Find("NetworkManager").GetComponent<FPSNetworkCode.NetworkManager>();
            netManager.GameSceneStart(this);
        }

        if (GameObject.Find("ProfileManager")) {
            profileManager = GameObject.Find("ProfileManager").GetComponent<ProfileMgr>();
        }

        if (!inputCtrlRef) {
            if (GameObject.Find("Local Input Manager")) {
                inputCtrlRef = GameObject.Find("Local Input Manager").GetComponent<FPSCharController.LocalClientInput>();
            }
        }
    }

    public void SpawnPlayer(string getUsername, bool spawnLocalClient) {
        GameObject newObj;

        if (!playerPrefab) {
            Debug.LogError("[Error] Player prefab missing; cannot instantiate player.");
            return;
        }

        newObj = Instantiate(playerPrefab, new Vector3(0,10,0), Quaternion.identity);

        netManager.AddPlayerCharRef(getUsername, newObj.transform);

        if (spawnLocalClient) {
            if (inputCtrlRef) {
                inputCtrlRef.AssignCharacterToControl(newObj.GetComponent<FPSCharController.FirstPersonController>());
            }
            else {
                Debug.LogError("[Error] Missing inputCtrlRef; cannot assign character control.");
            }
        }
    }

}
