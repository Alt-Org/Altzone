using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentalControlManager : MonoBehaviour
{
    public GameObject parentalControlPanel;
    public GameObject passwordPanel;
    //public InputField passwordInput;
    public TMP_InputField passwordInput;
    public TMP_InputField popupPasswordInput;
    public TMP_InputField confirmPasswordInput;
    //public Text messageText;
    public TMP_Text messageText;
    public Toggle controlToggle;
    public TMP_InputField timeLimitInput;
    //the password will be set in the pop up
    public GameObject parentalControlPopup;

    public bool parentalControl;

    private string sessionPassword = "";

    //the password already set in PlayerPrefs
    public string presetPassword;
    public string setPasswordInput;
    public string setConfirmPasswordInput;

    void Start()
    {

        CheckControl();
        GetPassword(); //gets the password that is already set in PlayerPrefs
        Debug.Log("got the password " + presetPassword);
        Debug.Log("control status " + parentalControl);

        parentalControlPanel.SetActive(true);
        passwordPanel.SetActive(true);

        controlToggle.isOn = true; // PlayerPrefs.GetInt("ParentalControl", 0) == 1;
        timeLimitInput.text = 10f.ToString(); //PlayerPrefs.GetFloat("MaxPlayTime", 2f).ToString();

    }

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        messageText.text = "";
    }

    public void CheckPassword()
    {
        if (sessionPassword == "")
        {
            if (passwordInput.text.Length < 4-8)            // < -4 ??
            {
                messageText.text = "Password must be at least 4-8 characters";
                return;
            }

            if (passwordInput.text != confirmPasswordInput.text)
            {
                messageText.text = "Passwords do not match";
                return;
            }

            sessionPassword = passwordInput.text;
            messageText.text = "Password set!";
            Invoke("ShowSettings", 1.0f);
        }
        else if (passwordInput.text == sessionPassword)
        {
            messageText.text = "Access granted!";
            Invoke("ShowSettings", 0.5f);
        }
        else
        {
            messageText.text = "Incorrect password!";
        }
    }

    public void GetPassword() {

        presetPassword = PlayerPrefs.GetString("password");


             }

    public void LogIn() {

        if (passwordInput.text == presetPassword)
        {
            Debug.Log("correct password, login allowed");
            //messageText.text = "Access granted!";
            Invoke("ShowSettings", 0.5f);
            

        }

    }

    public void SetPasswordInput()
    {
        
        setPasswordInput = popupPasswordInput.text;
        Debug.Log("to password input " + setPasswordInput);


    }


    public void SetConfirmPasswordInput()
    {
        
        setConfirmPasswordInput = confirmPasswordInput.text;
        Debug.Log("to confirm password input " + confirmPasswordInput.text);


    }



    public void SetPassword() {
        //this is done in the pop-up
        //todo: password criteria-check
        Debug.Log(setPasswordInput);


        if (setPasswordInput.Equals(setConfirmPasswordInput)) {


            Debug.Log("password is set to PlayerPrefs " + setPasswordInput);
            PlayerPrefs.SetString("password", setPasswordInput);

            //the int will indicate if Parental Control is on, 1 means that it is set
            //boolean is not allowed in PlayerPrefs, otherwise it would be that
            PlayerPrefs.SetInt("parentalcontrol", 1);
            Debug.Log(PlayerPrefs.GetString("password"));
            Debug.Log(PlayerPrefs.GetInt("parentalcontrol"));

            //messageText.text = "Password set!";

        } else
        {
           // messageText.text = "Passwords do not match";
        }

        
    }

    public void CheckControl()
    {
        //at start is used to check if Parental Control is on
        int checkControl = PlayerPrefs.GetInt("parentalcontrol");

        if (checkControl == 1) {
            parentalControl = true;
        } else
        {
            parentalControl = false;

        }

    }


    private void ShowSettings()
    {
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        messageText.text = "";
        passwordPanel.SetActive(false);
        parentalControlPanel.SetActive(true);
    }

    public void CloseParentalControl()
    {
        parentalControlPanel.SetActive(false);
        sessionPassword = "";
    }


    public void ToggleParentalControl(bool isEnabled)
    {
        PlayerPrefs.SetInt("ParentalControl", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetTimeLimit()
    {
        if (float.TryParse(timeLimitInput.text, out float time))
        {
            PlayerPrefs.SetFloat("MaxPlayTime", time);
            PlayerPrefs.Save();
        }
    }
}
