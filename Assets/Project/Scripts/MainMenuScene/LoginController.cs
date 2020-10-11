using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public GameObject LoginButton, LoginButtonCover, UserLoginText, PasswordText, TitleText, 
                      HeadingPanel, HeadingText, LoginButtonText, FadeoutPanel;
    private int fakeLoginCount;
    public bool loggingIn, loggedIn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!loggingIn && !loggedIn)
        {
            if (UserLoginText.GetComponent<TMP_InputField>().text != "")
                if (PasswordText.GetComponent<TMP_InputField>().text != "")
                {
                    LoginButton.SetActive(true);
                    LoginButtonCover.SetActive(false);
                }
            if (UserLoginText.GetComponent<TMP_InputField>().text == "")
                if (PasswordText.GetComponent<TMP_InputField>().text == "")
                {
                    LoginButton.SetActive(false);
                    LoginButtonCover.SetActive(true);
                }
        }
        
        if(loggingIn)
        {
            
            fakeLoginCount++;
            if(fakeLoginCount > 1800)
            {
                LoggedIn();
                loggingIn = false;
                fakeLoginCount = 0;
            }
        }
    }

    public void LoggingIn()
    { 
        loggingIn = true;
        LoginButtonText.GetComponent<TextMeshProUGUI>().fontSize = 15.0f;
        LoginButtonText.GetComponent<TextMeshProUGUI>().text = "Connecting...";
    }

    public void LoggedIn()
    {
        LoginButtonText.GetComponent<TextMeshProUGUI>().fontSize = 23.28f;
        LoginButtonText.GetComponent<TextMeshProUGUI>().text = "Connect";
        loggedIn = true;
        Destroy(LoginButton);
        LoginButton.SetActive(false);
        LoginButtonCover.SetActive(false);
        UserLoginText.SetActive(false);
        PasswordText.SetActive(false);
        TitleText.SetActive(false);
        HeadingPanel.SetActive(false);
        HeadingText.SetActive(false);
        FadeoutPanel.SetActive(true);
    }

    public void MoveToSelectionScene()
    {

    }
}
