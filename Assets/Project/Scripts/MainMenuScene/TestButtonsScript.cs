using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtonsScript : MonoBehaviour
{
    public GameObject NwManager, ProfileManager;
    void Start()
    {
        NwManager = GameObject.Find("NetworkManager");
        ProfileManager = GameObject.Find("ProfileMgr");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestProfile()
    {
        NwManager.SendMessage("RequestProfileData", ProfileManager.GetComponent<ProfileMgr>()._Username);
    }
    public void TestMM()
    {
        NwManager.SendMessage("QueueMatchMaking");
    }

    public void LeaveMatch()
    {
        NwManager.SendMessage("LeaveMatchMakingQueue");
    }
}
