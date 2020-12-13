using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtonsScript : MonoBehaviour
{
    public GameObject ProfileManager;
    private FPSNetworkCode.NetworkManager NwManager;

    void Start()
    {
        NwManager = GameObject.Find("NetworkManager").GetComponent<FPSNetworkCode.NetworkManager>();
        ProfileManager = GameObject.Find("ProfileMgr");
    }

    public void TestProfile()
    {
        if (NwManager) 
        {
            NwManager.SendMessage("RequestProfileData", ProfileManager.GetComponent<ProfileMgr>()._Username);
        }
        
    }
    public void TestMM()
    {
        if (!NwManager) {
             NwManager = GameObject.Find("NetworkManager").GetComponent<FPSNetworkCode.NetworkManager>();
        }

        if (NwManager) 
            {
            NwManager.SendMessage("QueueMatchMaking");
        }
    }

    public void LeaveMatch()
    {
        if (NwManager) 
        {
            NwManager.SendMessage("LeaveMatchMakingQueue");
        }
    }
}
