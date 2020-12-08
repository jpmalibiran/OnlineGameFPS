using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ProfileController : MonoBehaviour
{
    public GameObject ProfileManager;
    public GameObject Title, Kills, Deaths, Matches, Ratio, PrestigeExpBar, PrestigeText;
    // Start is called before the first frame update
    void Start()
    {
        ProfileManager = GameObject.Find("ProfileManager");

        SetValues();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetValues()
    {

    }
    public void SetValues()
    {
        Title.GetComponent<TextMeshProUGUI>().text =
            ProfileManager.GetComponent<ProfileMgr>()._Username;
        
        Kills.GetComponent<TextMeshProUGUI>().text = 
            "Kills: " + ProfileManager.GetComponent<ProfileMgr>()._kills.ToString();
        
        Deaths.GetComponent<TextMeshProUGUI>().text =
            "Deaths: " + ProfileManager.GetComponent<ProfileMgr>()._deaths.ToString();
        
        Matches.GetComponent<TextMeshProUGUI>().text =
            "Matches: " + ProfileManager.GetComponent<ProfileMgr>()._matches.ToString();
        
        Ratio.GetComponent<TextMeshProUGUI>().text =
            "Kill/Death Ratio: " + ProfileManager.GetComponent<ProfileMgr>()._kdleft.ToString() + " / " +
                                   ProfileManager.GetComponent<ProfileMgr>()._kdright.ToString();
        
        PrestigeText.GetComponent<TextMeshProUGUI>().text =
            "Prestige Level " + ProfileManager.GetComponent<ProfileMgr>()._level.ToString();
        
        PrestigeExpBar.GetComponent<Transform>().localScale = new Vector3(ProfileManager.GetComponent<ProfileMgr>()._exp,1,1);



    }
    public void MoveToSelection()
    {
        SceneManager.LoadScene(2);
    }
}
