﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public GameObject LoginButton, LoginButtonCover, UserLoginText, PasswordText, TitleText, 
                      HeadingPanel, HeadingText, LoginButtonText, FadeoutPanel, RegisterButton,
                      RegisterName, RegisterPassword, RegisterLocation, RegisterAge, SubmitCoverButton, SubmitButton;
    private int fakeLoginCount;
    public bool loggingIn, loggedIn, Registering;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!loggingIn && !loggedIn && !Registering)
        {
            if (UserLoginText.GetComponent<TMP_InputField>().text != "" &&
                PasswordText.GetComponent<TMP_InputField>().text != "")
            {
                    LoginButton.SetActive(true);
                    LoginButtonCover.SetActive(false);
            }        
            else 
            {  
                LoginButton.SetActive(false);
                LoginButtonCover.SetActive(true);  
            }             
        }
        
        if(Registering)
        {

            if(RegisterName.GetComponent<TMP_InputField>().text !="" &&
               RegisterPassword.GetComponent<TMP_InputField>().text != "" &&
               RegisterAge.GetComponent<TMP_InputField>().text != "" &&
               RegisterLocation.GetComponent<TMP_InputField>().text != "")
            {
                SubmitCoverButton.SetActive(false);
                SubmitButton.SetActive(true);
            }
            else { SubmitCoverButton.SetActive(true); SubmitButton.SetActive(false); }


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
        RegisterButton.SetActive(false);
    }

    public void MoveToSelectionScene()
    {
        SceneManager.LoadScene(2);
    }

    public void StartRegistration() 
    {
        LoginButton.SetActive(false);
        LoginButtonCover.SetActive(false);
        UserLoginText.SetActive(false);
        PasswordText.SetActive(false);
        HeadingText.GetComponent<TextMeshProUGUI>().text = "Register New Profile";
        RegisterButton.SetActive(false);
        //RegisterName, RegisterPassword, RegisterLocation, RegisterAge, SubmitCoverButton, SubmitButton;
        RegisterName.SetActive(true);
        RegisterPassword.SetActive(true);
        RegisterLocation.SetActive(true);
        RegisterAge.SetActive(true);
        SubmitCoverButton.SetActive(true);
        Registering = true;
    }

    public void ReturnFromRegistration()
    {
        HeadingText.GetComponent<TextMeshProUGUI>().text = "User Login";
        LoginButton.SetActive(true);
        LoginButtonCover.SetActive(true);
        UserLoginText.SetActive(true);
        PasswordText.SetActive(true);
        RegisterButton.SetActive(true);
        //RegisterName, RegisterPassword, RegisterLocation, RegisterAge, SubmitCoverButton, SubmitButton;
        RegisterName.SetActive(false);
        RegisterPassword.SetActive(false);
        RegisterLocation.SetActive(false);
        RegisterAge.SetActive(false);
        SubmitCoverButton.SetActive(false);
        SubmitButton.SetActive(false);

        Registering = false;
    }

    public void CompleteRegistration()
    {


    }    
}