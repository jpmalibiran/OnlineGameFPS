using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionController : MonoBehaviour
{
    public GameObject QuickPlayButton, BackButton, ProfileButton, HeadingText, FadeoutPaneltoGame, FadeoutPaneltoMain,
                       MusicObject;
    public bool FindingMatch, FoundMatch;
    private int fakeMatchmakingCount;
    // Start is called before the first frame update
    void Start()
    {
        MusicObject = GameObject.Find("MainMenuLoop");
    }

    // Update is called once per frame
    void Update()
    {
        if(FindingMatch)
        {
            fakeMatchmakingCount++;

            if (MusicObject) 
            {
                if (MusicObject.GetComponent<AudioSource>().volume > 0)
                {
                    MusicObject.GetComponent<AudioSource>().volume -= 0.001f;
                }
            }

        }

        if(FoundMatch)
        {
            HeadingText.GetComponent<TextMeshProUGUI>().text = "Match Found, Prepare for Battle";
        }

        if(FoundMatch && fakeMatchmakingCount > 1150)
        {
            FadeoutPaneltoGame.SetActive(true);
        }
    }


    public void MoveBack()
    {
        BackButton.SetActive(false);
        ProfileButton.SetActive(false);
        QuickPlayButton.SetActive(false);
        FadeoutPaneltoMain.SetActive(true);
    }

    public void MoveToMain()
    {
        SceneManager.LoadScene(1);
    }
    public void StartMatchmaking()
    {
        QuickPlayButton.SetActive(false);
        ProfileButton.SetActive(false);
        BackButton.SetActive(false);
        HeadingText.GetComponent<TextMeshProUGUI>().text = "Finding Match";
        FindingMatch = true;
    }

    public void MatchmakingComplete()
    {
        FoundMatch = true;  
        MoveToGame();
    }

    public void MoveToGame()
    {
        Destroy(MusicObject);
        SceneManager.LoadScene(3);
    }
    
    public void MoveToProfile()
    {
        SceneManager.LoadScene(4);
    }
    public void CancelMatchmaking()
    {
        QuickPlayButton.SetActive(true);
        ProfileButton.SetActive(true);
        BackButton.SetActive(true);
        HeadingText.GetComponent<TextMeshProUGUI>().text = "Choose Operation";
        FindingMatch = false;
        MusicObject.GetComponent<AudioSource>().volume = 1.0f;
    }
}
