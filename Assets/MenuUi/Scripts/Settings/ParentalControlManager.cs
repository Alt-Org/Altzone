using System;
using System.Runtime.CompilerServices;
using Altzone.Scripts.AzDebug;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentalControlManager : MonoBehaviour
{
    public GameObject parentalControlPanel;
    public GameObject passwordPanel;
    public GameObject parentalControlSettings;
    //public InputField passwordInput;
    public TMP_InputField passwordInput;
    public TMP_InputField popupPasswordInput;
    public TMP_InputField confirmPasswordInput;
    //public Text messageText;
    public TMP_Text messageText;
    public Toggle controlToggle;
    

    //the password will be set in the pop up
    public GameObject parentalControlPopup;
    public GameObject parentalControlPopupButton;

    //boolean to check if the ParentalControl is on
    public bool parentalControl;

    private string sessionPassword = "";

    //the password already set in PlayerPrefs
    public string presetPassword;
    public string setPasswordInput;
    public string setConfirmPasswordInput;



    //the ParentalControl settings
    //SocialControls
    public bool internetLinks;
    public Toggle internetLinksToggle; 
    public bool chatMessages;
    public Toggle chatMessagesToggle;
    public bool emojiComments;
    public Toggle emojiCommentsToggle;
    public bool treasureHunt;
    public Toggle treasureHuntToggle;

    //MoneyControls
    public float monthlyLimit;
    public TMP_InputField monthlyLimitInput;
    public bool independentSpendingActivation;
    public Toggle independentSpendingActivationToggle;

    //TimeControls
    public int dailyLimit;
    public TMP_InputField timeLimitInput;
    public int timeLimitAccuracy;
    public TMP_InputField timeLimitAccuracyInput;
    public bool midMatch;
    public Toggle midMatchToggle;
    public bool endMatch;
    public Toggle endMatchToggle;


    




    void Start()
    {
        parentalControlSettings.SetActive(false);
        CheckControl();
        GetPassword(); //gets the password that is already set in PlayerPrefs
        LoadSettings();
        Debug.Log("got the password " + presetPassword);
        Debug.Log("control status " + parentalControl);

        parentalControlPanel.SetActive(true);
        passwordPanel.SetActive(true);

        controlToggle.isOn = true; // PlayerPrefs.GetInt("ParentalControl", 0) == 1;
        timeLimitInput.text = 10f.ToString(); //PlayerPrefs.GetFloat("MaxPlayTime", 2f).ToString();

        internetLinksToggle.onValueChanged.AddListener(_ => SetInternetLinks());
        SetInternetLinksToggle();

    }

    private void OnEnable()
    {
        SetInternetLinksToggle();
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
        //next two are to ensure that log in works when control is first disabled and then set anew in the same session
        //TODO a more elegant way to ensure this
        CheckControl();
        GetPassword();
        if (parentalControl == true)
        {
            if (passwordInput.text == presetPassword)
            {
                Debug.Log("correct password, login allowed");
                //messageText.text = "Access granted!";
                parentalControlSettings.SetActive(true);
               
                               



            }

        } else
        {
            //TODO message: can't log in if ParentalControl not activated

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
            parentalControlPopupButton.SetActive(false);

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
            parentalControlPopupButton.SetActive(false);

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

    public void DisableParentalControl()
    {
        //ParentalControl is active only if PlayerPrefs "parentalcontrol" equals 1
        PlayerPrefs.SetInt("parentalcontrol", 0);
        PlayerPrefs.SetString("password","");
        PlayerPrefs.Save();
        parentalControlPopupButton.SetActive(true);


    }

    public void SetInternetLinks ()
    {
        if (internetLinksToggle.isOn) {
            PlayerPrefs.SetInt("internetLinks", 1);
            

        } else
        {
            PlayerPrefs.SetInt("internetLinks", 0);
        }

    }

    public void SetInternetLinksToggle() {

        internetLinksToggle.isOn = (PlayerPrefs.GetInt("internetlinks",0) !=0);

    }


    public void SaveSettings()
    {
        PlayerPrefs.Save();

    }

    public void LoadSettings()
    {
        //TODO how to load settings from PlayerPrefs
        /*
        int checkLinks = PlayerPrefs.GetInt("internetLinks");

        if (checkLinks == 1)
        {
            internetLinks = true;
            Debug.Log("internetLinks is set to true");

            

        }
        else
        {
            internetLinks = false;

            

        }
        internetLinksToggle.isOn = internetLinks;
        */


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
