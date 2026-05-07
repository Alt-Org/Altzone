using System;
using System.Runtime.CompilerServices;
using Altzone.Scripts.AzDebug;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

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
    public Toggle testToggle;
    

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
    //public bool allowLinks;
    public Toggle internetLinksToggle; 
    //public bool chatMessages;
    public Toggle chatMessagesToggle;
    //public bool allowEmojis;
    public Toggle emojiCommentsToggle;
    //public bool allowTreasureHunt;
    public Toggle treasureHuntToggle;

    //MoneyControls
    //public float monthlyLimit;
    public TMP_InputField monthlyLimitInput;
    //public bool activatePurchasesSeparately;
    public Toggle independentSpendingActivationToggle;

    //TimeControls
    public int dailyLimit;
    public TMP_InputField timeLimitInput;
    public int timeLimitAccuracy;
    public TMP_InputField timeLimitAccuracyInput;
    //public bool EndMidMatch;
    public Toggle midMatchToggle;
    //public bool EndAfterMatch;
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

        testToggle.onValueChanged.AddListener(_ => SetTestToggle());
        internetLinksToggle.onValueChanged.AddListener(_ => SetInternetLinks());
        chatMessagesToggle.onValueChanged.AddListener(_ => SetChatMessages());
        emojiCommentsToggle.onValueChanged.AddListener(_ => SetEmojis());
        treasureHuntToggle.onValueChanged.AddListener(_ => SetTreasureHunt());
        monthlyLimitInput.onValueChanged.AddListener(_ => SetMonthlyLimit());
        independentSpendingActivationToggle.onValueChanged.AddListener(_ => SetIndependentSpendingActivation());
        timeLimitInput.onValueChanged.AddListener(_ => SetTimeLimit());
        midMatchToggle.onValueChanged.AddListener(_ => SetEndMidMatch());
        endMatchToggle.onValueChanged.AddListener(_ => SetEndAfterMatch());
    }

    
    private void OnEnable()
    {
        SetInternetLinksToggle();
        SetChatMessagesToggle();
        SetTestToggleToggle();
        SetEmojisToggle();
        SetTreasureHuntToggle();
        GetMonthlyLimit();
        SetIndependentSpendingActivationToggle();
        GetTimeLimit();
        SetEndMidMatchToggle();
        SetEndAfterMatchToggle();
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


    public void SetTestToggle()
    {
        if (testToggle.isOn)
        {
            PlayerPrefs.SetInt("testToggle", 1);


        }
        else
        {
            PlayerPrefs.SetInt("testToggle", 0);
        }

    }

    public void SetTestToggleToggle() {
        testToggle.isOn = (PlayerPrefs.GetInt("testToggle", 0) !=0);

    }
    public void SetInternetLinks ()
    {
        if (internetLinksToggle.isOn) {
            PlayerPrefs.SetInt("AllowLinks", 1);
            

        } else
        {
            PlayerPrefs.SetInt("AllowLinks", 0);
        }

    }

    public void SetInternetLinksToggle() {
        internetLinksToggle.isOn = (PlayerPrefs.GetInt("AllowLinks", 0) != 0);
        Debug.Log("internetLinks, got value" + PlayerPrefs.GetInt("AllowLinks"));
        Debug.Log("internetLinksToggle is set");

       
    }

    public void SetChatMessages()
    {
        if (chatMessagesToggle.isOn)
        {
            PlayerPrefs.SetInt("AllowChat", 1);
        }
        else
        {
            PlayerPrefs.SetInt("AllowChat", 0);
        }

    }


    public void SetChatMessagesToggle()
    {
        chatMessagesToggle.isOn = (PlayerPrefs.GetInt("AllowChat", 0) != 0);
        Debug.Log("chat, got value" + PlayerPrefs.GetInt("AllowChat"));
        Debug.Log("chatMessagesToggle is set");

    }


    public void SetEmojis()
    {
        if (emojiCommentsToggle.isOn)
        {
            PlayerPrefs.SetInt("AllowEmojis", 1);
        }
        else
        {
            PlayerPrefs.SetInt("AllowEmojis", 0);
        }

    }
    public void SetEmojisToggle()
    {
        emojiCommentsToggle.isOn = (PlayerPrefs.GetInt("AllowEmojis", 0) != 0);
        

    }


    public void SetTreasureHunt()
    {
        if (treasureHuntToggle.isOn)
        {
            PlayerPrefs.SetInt("AllowTreasureHunt", 1);
        }
        else
        {
            PlayerPrefs.SetInt("AllowTreasureHunt", 0);
        }

    }
    public void SetTreasureHuntToggle()
    {
        treasureHuntToggle.isOn = (PlayerPrefs.GetInt("AllowTreasureHunt", 0) != 0);


    }





    public void SaveSettings()
    {
        PlayerPrefs.Save();

    }

    public void LoadSettings()
    {
        //TODO how to load all settings from PlayerPrefs
        //maybe it would be practical to load them all in the same method, maybe not

    }

    public void SetMonthlyLimit()
    {
        //money control: monthly spending limit
        string money = monthlyLimitInput.text;
        if (money.IsNullOrEmpty())
        {
            PlayerPrefs.SetFloat("MonthlyLimit", 0);
        }
        else
        {
            PlayerPrefs.SetFloat("MonthlyLimit", float.Parse(money));
        }
        


    }

    public void GetMonthlyLimit()
    {
        float getInput = PlayerPrefs.GetFloat("MonthlyLimit");
        monthlyLimitInput.text = getInput.ToString();

    }


    public void SetIndependentSpendingActivation()
    {
        if (independentSpendingActivationToggle.isOn)
        {
            PlayerPrefs.SetInt("ActivatePurchasesSeparately", 1);
        }
        else
        {
            PlayerPrefs.SetInt("ActivatePurchasesSeparately", 0);
        }

    }
    public void SetIndependentSpendingActivationToggle()
    {
        independentSpendingActivationToggle.isOn = (PlayerPrefs.GetInt("ActivatePurchasesSeparately", 0) != 0);


    }


    public void SetTimeLimit()
    {
        if (float.TryParse(timeLimitInput.text, out float time))
        {
            PlayerPrefs.SetFloat("MaxPlayTime", time);
            PlayerPrefs.Save();
        }
    }

    public void GetTimeLimit()
    {
        float getTime = PlayerPrefs.GetFloat("MaxPlayTime");
        timeLimitInput.text = getTime.ToString();

    }


    public void SetEndMidMatch()
    {
        if (midMatchToggle.isOn)
        {
            PlayerPrefs.SetInt("EndMidMatch", 1);
        }
        else
        {
            PlayerPrefs.SetInt("EndMidMatch", 0);
        }

    }
    public void SetEndMidMatchToggle()
    {
        midMatchToggle.isOn = (PlayerPrefs.GetInt("EndMidMatch", 0) != 0);


    }

    public void SetEndAfterMatch()
    {
        if (endMatchToggle.isOn)
        {
            PlayerPrefs.SetInt("EndAfterMatch", 1);
        }
        else
        {
            PlayerPrefs.SetInt("EndAfterMatch", 0);
        }

    }
    public void SetEndAfterMatchToggle()
    {
        endMatchToggle.isOn = (PlayerPrefs.GetInt("EndAfterMatch", 0) != 0);


    }


}
