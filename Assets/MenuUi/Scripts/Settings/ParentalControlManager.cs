using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Altzone.Scripts.AzDebug;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ParentalControlManager : MonoBehaviour
{
    private SettingsCarrier _carrier = SettingsCarrier.Instance; //SettingsCarrier carries all the settings between scenes etc. 



    public GameObject parentalControlPanel;
    public GameObject activateLogInPanel; //The panel when you can activate Parental Control or log in if Parental Control is already activated
    public GameObject passwordPanel;
    public GameObject parentalControlSettings; //The panel that contains all the settings for Parental Control
    public TMP_Text description; //The text that describes what Parental Control does

    public TMP_InputField passwordInput; //The password input field in the log in panel 
    public Toggle logInPasswordVisibilityToggle; //The "eye" to show or hide the password input

    public TMP_InputField popupPasswordInput; //The field for setting the password in the pop up
    public TMP_InputField confirmPasswordInput;
    
    public TMP_Text messageText; //Message text when trying to input a wrong password
    public TMP_Text messageTextPopUpLength; //Message text when trying to set a password that is too short in the pop up
    public TMP_Text messageTextPopUpWrongPassword; //Message text when trying to set a password that doesn't match the confirmation
    //public Toggle controlToggle; //Toggle to enable or disable Parental Control, currently not in use


    //the password will be set in the pop up
    public GameObject parentalControlPopup; //The actual pop up for setting the password
    public GameObject parentalControlPopupButton; //The button that opens the pop up for setting the password
    public Button popupEye; //The "eye" for the popupPasswordInput field 

    public Button confirmEye; //The "eye" for the confirmPasswordInput field

    public GameObject controlEnabledButton; //No current functionality, just a visual indicator that Parental Control is enabled

    //boolean to check if the ParentalControl is on
    public bool parentalControl;

    //private string sessionPassword = ""; //currently not in use

    public string presetPassword; //the password already set in PlayerPrefs
    public string setPasswordInput; //the password input in the pop up
    public string setConfirmPasswordInput;

    public Button eyeButton; //The "eye" for the password fields
    public Sprite eyeOpen;
    public Sprite eyeClosed;



    //the ParentalControl settings
    //SocialControls
    
    public Toggle internetLinksToggle; 
    public Toggle chatMessagesToggle;
    public Toggle emojiCommentsToggle;
    public Toggle treasureHuntToggle;

    //MoneyControls
    
    public TMP_InputField monthlyLimitInput;
    public Toggle independentSpendingActivationToggle;

    //TimeControls
     
    public TMP_InputField timeLimitInput;
    //public TMP_InputField timeLimitAccuracyInput; //currently not in use, was in previous plans
    public Toggle midMatchToggle;
    public Toggle endMatchToggle;

    public GameObject settingsSavedPopUp; //The pop up that is shown shortly when settings are saved


    void Start()
    {
        parentalControlSettings.SetActive(false);
        CheckControl(); //checks if Parental Control is on
        GetPassword(); //gets the password that is already set in PlayerPrefs
        LoadSettings(); //currently not in use, but could be used to load all the settings from PlayerPrefs at the same time, now they are loaded one by one
        
        parentalControlPanel.SetActive(true);
        //passwordPanel.SetActive(true);
        messageText.enabled = false;

        messageTextPopUpWrongPassword.enabled = false;
        messageTextPopUpLength.enabled = false;

        //controlToggle.isOn = true; // PlayerPrefs.GetInt("ParentalControl", 0) == 1; //The checkmark that is currently not in use
        
        internetLinksToggle.onValueChanged.AddListener(_ => SetInternetLinks()); //Listeners are added for all the settings
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
        SetInternetLinksToggle(); //Sets all the settings
        SetChatMessagesToggle();
        SetEmojisToggle();
        SetTreasureHuntToggle();
        GetMonthlyLimit();
        SetIndependentSpendingActivationToggle();
        GetTimeLimit();
        SetEndMidMatchToggle();
        SetEndAfterMatchToggle();
        CheckControl();
        GetPassword();
        
    }
    

    public void GetPassword() {

        presetPassword = PlayerPrefs.GetString("password");


             }

        

    public void LogIn()
    {


        if (parentalControl == true)
        {
            if (passwordInput.text == presetPassword)
            {
       
                messageText.enabled = false;
                parentalControlSettings.SetActive(true);
                passwordInput.text = "";
                passwordPanel.SetActive(false);
                activateLogInPanel.SetActive(false);

            }

            else if (passwordInput.text != presetPassword)
            {

                //messageText will show the message that the password is wrong
                messageText.enabled = true;
            }

        }
        else
        {


            //Could show a message that you can't log in because Parental Control is not activated
            //Or that you can change the settings only after log in
            //Maybe not needed because you can't interact with the "ParentalControl is on"-button. It is only a visual indicator that Parental Control is on
            //And you can't see the log in password field if Parental Control is not activated, so you can't log in anyway


        }


    }

    public void LogOut()
    {
        parentalControlSettings.SetActive(false);
        passwordPanel.SetActive(true);
        activateLogInPanel.SetActive(true);
        


    }

    public void SetPasswordInput()
    {
        
        setPasswordInput = popupPasswordInput.text;
       
    }


    public void SetConfirmPasswordInput()
    {
        
        setConfirmPasswordInput = confirmPasswordInput.text;
        
    }



    public void SetPassword() {
        //this is done in the pop-up
                     

        if (setPasswordInput.Length < 8)            
        {
            messageTextPopUpLength.enabled = true; //messageTextPopUpLength will show the message that the password is too short
            messageTextPopUpWrongPassword.enabled = false;
            return;
        }

        if (setPasswordInput.Equals(setConfirmPasswordInput)) {
                        
            PlayerPrefs.SetString("password", setPasswordInput);

            //the int will indicate if Parental Control is on, 1 means that it is set
            //boolean is not allowed in PlayerPrefs, otherwise it would be that
            PlayerPrefs.SetInt("parentalcontrol", 1);
            parentalControlPopupButton.SetActive(false);
            controlEnabledButton.SetActive(true); //just a visual indicator that Parental Control is on, no functionality

            ClearPasswordFields();
            parentalControlPopup.SetActive(false);
            CheckControl();
            GetPassword();
            parentalControlSettings.SetActive(true);
            passwordPanel.SetActive(false);
            activateLogInPanel.SetActive(false);

        } else
        {
            //messageTextPopUpWrongPassword will show the message that the password and confirmation do not match
            messageTextPopUpWrongPassword.enabled = true;
            messageTextPopUpLength.enabled = false;
        }

        
    }

    public void ClearPasswordFields()
    {
        //all the three password input fields will be cleared
        popupPasswordInput.text = "";
        confirmPasswordInput.text = "";
        passwordInput.text = "";


    }

    public void CheckControl()
    {
        //at start is used to check if Parental Control is on
        int checkControl = PlayerPrefs.GetInt("parentalcontrol");

        if (checkControl == 1) {
            parentalControl = true;
            parentalControlPopupButton.SetActive(false);
            passwordPanel.SetActive(true);
            controlEnabledButton.SetActive(true);

        } else
        {
            parentalControl = false;
            passwordPanel.SetActive(false);
            controlEnabledButton.SetActive(false);

        }

    }
        
    public void DisableParentalControl()
    {
        //ParentalControl is active only if PlayerPrefs "parentalcontrol" equals 1
        PlayerPrefs.SetInt("parentalcontrol", 0);
        PlayerPrefs.SetString("password","");
        
        PlayerPrefs.SetInt("AllowLinks", 0);
        _carrier.AllowLinks = false;
        PlayerPrefs.SetInt("AllowChat", 0);
        _carrier.ChatMessages = false;
        PlayerPrefs.SetInt("AllowEmojis", 0);
        _carrier.AllowEmojis = false;
        PlayerPrefs.SetInt("AllowTreasureHunt", 0);
        _carrier.AllowTreasureHunt = false;
        PlayerPrefs.SetFloat("MonthlyLimit", 0);
        _carrier.MonthlyLimit = 0;
        PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
        PlayerPrefs.SetInt("ActivatePurchasesSeparately", 0);
        _carrier.ActivatePurchasesSeparately = false;
        PlayerPrefs.SetFloat("MaxPlayTime", 1); //There will always be a minimum time limit of 1 hour. In case the user forgets the password, they can still play a bit
        PlayerPrefs.SetFloat("DailyTimeLimit", 1); //for some reason there has also been a setting that is called DailyTimeLimit, current use unknown
        _carrier.MaxPlayTime = 1;  
        PlayerPrefs.SetInt("EndMidMatch", 0);
        _carrier.EndMidMatch = false;
        PlayerPrefs.SetInt("EndAfterMatch", 1);
        _carrier.EndAfterMatch = true;
        PlayerPrefs.Save();
        SetInternetLinksToggle();
        SetChatMessagesToggle();
        SetEmojisToggle();
        SetTreasureHuntToggle();
        GetMonthlyLimit();
        SetIndependentSpendingActivationToggle();
        GetTimeLimit();
        SetEndMidMatchToggle();
        SetEndAfterMatchToggle();

        parentalControlPopupButton.SetActive(true);
        controlEnabledButton.SetActive(false);
        activateLogInPanel.SetActive(true);
          
    }


    
    //Example, how to set values / toggles, is taken from SettingEditor, ShowButtonLabels

    public void SetInternetLinks ()
    {
        if (internetLinksToggle.isOn) {
            PlayerPrefs.SetInt("AllowLinks", 1);
            

        } else
        {
            PlayerPrefs.SetInt("AllowLinks", 0);
        }

        _carrier.AllowLinks = internetLinksToggle.isOn;
        
    }

    public void SetInternetLinksToggle() {
        
        internetLinksToggle.isOn = _carrier.AllowLinks;
               
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
        _carrier.ChatMessages = chatMessagesToggle.isOn;
        
    }


    public void SetChatMessagesToggle()
    {
        
        chatMessagesToggle.isOn = _carrier.ChatMessages;
        
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
        _carrier.AllowEmojis = emojiCommentsToggle.isOn;


    }
    public void SetEmojisToggle()
    {
        
        emojiCommentsToggle.isOn = _carrier.AllowEmojis;

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

        _carrier.AllowTreasureHunt = treasureHuntToggle.isOn;
    }
    public void SetTreasureHuntToggle()
    {
        
        treasureHuntToggle.isOn = _carrier.AllowTreasureHunt;

    }





    public void SaveSettings()
    {
        PlayerPrefs.Save();
        //A pop up will be shown for a short time to indicate that the settings have been saved
        settingsSavedPopUp.SetActive(true);
        CloseSettingsPopUp();

    }

    public void LoadSettings()
    {
        //TODO how to load all settings from PlayerPrefs? Currently they are loaded one by one
        //maybe it would be practical to load them all in the same method, maybe not

    }

    public void SetMonthlyLimit()
    {
        //money control: monthly spending limit
        string money = monthlyLimitInput.text;
        float moneyFloat = float.Parse(monthlyLimitInput.text); //maybe not needed, because the field is set to accept only floats in Unity's side
              

        if (money.IsNullOrEmpty())
        {
            PlayerPrefs.SetFloat("MonthlyLimit", 0);
            PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
            _carrier.MonthlyLimit = 0;
            PlayerPrefs.Save();
            GetMonthlyLimit();


        }
        else if (moneyFloat < 0) //you can't set a negative value for the money limit
        {
            PlayerPrefs.SetFloat("MonthlyLimit", 0);
            PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
            _carrier.MonthlyLimit = 0;
            //TODO some kind of message, please input a positive value?
            PlayerPrefs.Save();
            GetMonthlyLimit();

        }
        else //if (moneyFloat >= 0) 
        {
            PlayerPrefs.SetFloat("MonthlyLimit", float.Parse(money));
            PlayerPrefs.SetFloat("MonthlySpendingLimit", float.Parse(money));
            _carrier.MonthlyLimit = float.Parse(money);
            PlayerPrefs.Save();
            GetMonthlyLimit();


        }
           
    }

    public void GetMonthlyLimit()
    {
        
        monthlyLimitInput.text = _carrier.MonthlyLimit.ToString();
        
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

        _carrier.ActivatePurchasesSeparately = independentSpendingActivationToggle.isOn;
    }
    public void SetIndependentSpendingActivationToggle()
    {
        
        independentSpendingActivationToggle.isOn = _carrier.ActivatePurchasesSeparately;

    }


    public void SetTimeLimit()
    {

        string timeInput = timeLimitInput.text;
        float timeFloat = float.Parse(timeLimitInput.text);
        
        if (float.TryParse(timeLimitInput.text, out float time))
        {


            if (time > 1 && time <= 24) //The time limit is daily, so it can't go below 1 or above 24 hours. If the user inputs a value below 1, it will be set to 1, if above 24, it will be set to 24
            {
                PlayerPrefs.SetFloat("MaxPlayTime", time);
                PlayerPrefs.SetFloat("DailyTimeLimit", time);
                _carrier.MaxPlayTime = time;
                PlayerPrefs.Save();
                GetTimeLimit();

            }
            else if (time > 24) {
                PlayerPrefs.SetFloat("MaxPlayTime", 24);
                PlayerPrefs.SetFloat("DailyTimeLimit", 24);
                _carrier.MaxPlayTime = 24;
                PlayerPrefs.Save();
                GetTimeLimit();

            }
            else
            { //There is a minimum time limit of 1 hour per day

                PlayerPrefs.SetFloat("MaxPlayTime", 1);
                PlayerPrefs.SetFloat("DailyTimeLimit", time);
                _carrier.MaxPlayTime = 1;
                PlayerPrefs.Save();
                GetTimeLimit();
            }
            
            
        }
        
    }

    public void GetTimeLimit()
    {
        
        timeLimitInput.text = _carrier.MaxPlayTime.ToString();
        
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
        _carrier.EndMidMatch = midMatchToggle.isOn;
    }
    public void SetEndMidMatchToggle()
    {
        midMatchToggle.isOn = _carrier.EndMidMatch;

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
        _carrier.EndAfterMatch = endMatchToggle.isOn;
    }
    public void SetEndAfterMatchToggle()
    {
        endMatchToggle.isOn = _carrier.EndAfterMatch;

    }

    public void ChangePasswordVisibility()
    {
        //changes the eye icon to open eye and password to visible text
        if (passwordInput.contentType == TMP_InputField.ContentType.Password)
        {
            eyeButton.image.sprite = eyeOpen;
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        } else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            eyeButton.image.sprite = eyeClosed;
        }

        passwordInput.Select();

    }



    public void ChangePopUpPasswordVisibility()
    {
        //changes the eye icon to open eye and password to visible text
        if (popupPasswordInput.contentType == TMP_InputField.ContentType.Password)
        {
            popupEye.image.sprite = eyeOpen;
            popupPasswordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            popupPasswordInput.contentType = TMP_InputField.ContentType.Password;
            popupEye.image.sprite = eyeClosed;
        }

        popupPasswordInput.Select();

    }

    public void ChangePopUpConfirmPasswordVisibility()
    {
        //changes the eye icon to open eye and password to visible text
        if (confirmPasswordInput.contentType == TMP_InputField.ContentType.Password)
        {
            confirmEye.image.sprite = eyeOpen;
            confirmPasswordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
            confirmEye.image.sprite = eyeClosed;
        }

        confirmPasswordInput.Select();

    }


    public void CloseSettingsPopUp() //The settingsSavedPopUp will be closed after 2 seconds
    {
        StartCoroutine(RemoveAfterSeconds(2));
    }

    IEnumerator RemoveAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        settingsSavedPopUp.SetActive(false);
    }


}
