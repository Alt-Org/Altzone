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
    private SettingsCarrier carrier = SettingsCarrier.Instance;



    public GameObject parentalControlPanel;
    public GameObject activateLogInPanel;
    public GameObject passwordPanel;
    public GameObject parentalControlSettings;
    public TMP_Text description;

    //public InputField passwordInput;
    public TMP_InputField passwordInput;
    public Toggle logInPasswordVisibilityToggle;

    public TMP_InputField popupPasswordInput;
    public TMP_InputField confirmPasswordInput;
    //public Text messageText;
    public TMP_Text messageText;
    public TMP_Text messageTextPopUpLength;
    public TMP_Text messageTextPopUpWrongPassword;
    public Toggle controlToggle;
    public Toggle testToggle;
    

    //the password will be set in the pop up
    public GameObject parentalControlPopup;
    public GameObject parentalControlPopupButton;
    public Button popupEye;

    public Button confirmEye;

    public GameObject controlEnabledButton;

    //boolean to check if the ParentalControl is on
    public bool parentalControl;

    private string sessionPassword = "";

    //the password already set in PlayerPrefs
    public string presetPassword;
    public string setPasswordInput;
    public string setConfirmPasswordInput;

    public Button eyeButton;
    public Sprite eyeOpen;
    public Sprite eyeClosed;



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

    public GameObject settingsSavedPopUp;
    




    void Start()
    {
        parentalControlSettings.SetActive(false);
        CheckControl();
        GetPassword(); //gets the password that is already set in PlayerPrefs
        LoadSettings();
        Debug.Log("got the password " + presetPassword);
        Debug.Log("control status " + parentalControl);

        parentalControlPanel.SetActive(true);
        //passwordPanel.SetActive(true);
        messageText.enabled = false;

        messageTextPopUpWrongPassword.enabled = false;
        messageTextPopUpLength.enabled = false;

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
        CheckControl();
        GetPassword();
        
    }
    


    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        messageText.text = "";
    }

    public void CheckPassword() //this function is not in use currently
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


    public void SetPasswordVisibilityState(TMP_InputField passwordInputField, bool value) //is this currently in use?
    {
        if (value)
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
        else
            passwordInputField.contentType = TMP_InputField.ContentType.Password;

        passwordInputField.ForceLabelUpdate();
    }

    public void LogIn()
    {


        if (parentalControl == true)
        {
            if (passwordInput.text == presetPassword)
            {
                Debug.Log("correct password, login allowed");
                //messageText.text = "Kirjauduttu sisään";
                messageText.enabled = false;
                parentalControlSettings.SetActive(true);
                passwordInput.text = "";
                passwordPanel.SetActive(false);
                activateLogInPanel.SetActive(false);
                //description.enabled = false;


            }

            else if (passwordInput.text != presetPassword)
            {
                //TODO message: please input correct password
                //messageText.text = "Väärä salasana";
                messageText.enabled = true;
            }

        }
        else
        {
            //TODO message: can't log in if ParentalControl not activated
            //messageText.text = "Vanhempien kontrolli ei ole päällä";

        }


    }

    public void LogOut()
    {
        parentalControlSettings.SetActive(false);
        passwordPanel.SetActive(true);
        activateLogInPanel.SetActive(true);
        //description.enabled = true;


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


        Debug.Log(setPasswordInput);

        if (setPasswordInput.Length < 8)            
        {
            messageTextPopUpLength.enabled = true;
            messageTextPopUpWrongPassword.enabled = false;
            return;
        }

        if (setPasswordInput.Equals(setConfirmPasswordInput)) {


            Debug.Log("password is set to PlayerPrefs " + setPasswordInput);
            PlayerPrefs.SetString("password", setPasswordInput);

            //the int will indicate if Parental Control is on, 1 means that it is set
            //boolean is not allowed in PlayerPrefs, otherwise it would be that
            PlayerPrefs.SetInt("parentalcontrol", 1);
            parentalControlPopupButton.SetActive(false);
            controlEnabledButton.SetActive(true);

            Debug.Log(PlayerPrefs.GetString("password"));
            Debug.Log(PlayerPrefs.GetInt("parentalcontrol"));

            //messageText.text = "Password set!";

            //TODO: Pop up when password is set?
            ClearPasswordFields();
            parentalControlPopup.SetActive(false);
            CheckControl();
            GetPassword();
            parentalControlSettings.SetActive(true);
            passwordPanel.SetActive(false);
            activateLogInPanel.SetActive(false);

        } else
        {
           // messageText.text = "Passwords do not match";
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


    private void ShowSettings()
    {
        passwordInput.text = "";
        confirmPasswordInput.text = "";
        messageText.text = "";
        passwordPanel.SetActive(false);
        parentalControlPanel.SetActive(true);
    }

    public void CloseParentalControl() //currently not in use?
    {
        parentalControlPanel.SetActive(false);
        sessionPassword = "";
    }


    public void ToggleParentalControl(bool isEnabled) //currently not in use
    {
        PlayerPrefs.SetInt("ParentalControl", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void DisableParentalControl()
    {
        //ParentalControl is active only if PlayerPrefs "parentalcontrol" equals 1
        PlayerPrefs.SetInt("parentalcontrol", 0);
        PlayerPrefs.SetString("password","");
        
        PlayerPrefs.SetInt("AllowLinks", 0);
        carrier.AllowLinks = false;
        PlayerPrefs.SetInt("AllowChat", 0);
        carrier.ChatMessages = false;
        Debug.Log("Carrier's chat messages is set to" + carrier.ChatMessages);
        PlayerPrefs.SetInt("AllowEmojis", 0);
        carrier.AllowEmojis = false;
        PlayerPrefs.SetInt("AllowTreasureHunt", 0);
        carrier.AllowTreasureHunt = false;
        PlayerPrefs.SetFloat("MonthlyLimit", 0);
        //TODO carrier
        PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
        PlayerPrefs.SetInt("ActivatePurchasesSeparately", 0);
        carrier.ActivatePurchasesSeparately = false;
        PlayerPrefs.SetInt("MaxPlayTime", 0);
        //TODO carrier
        PlayerPrefs.SetInt("DailyTimeLimit", 0);  
        PlayerPrefs.SetInt("EndMidMatch", 0);
        carrier.EndMidMatch = false;
        PlayerPrefs.SetInt("EndAfterMatch", 1);
        carrier.EndAfterMatch = true;
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
        //messageText.text = "";
     


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


    //Example, how to set values / toggles, is taken from SettingEditor, ShowButtonLabels

    public void SetInternetLinks ()
    {
        if (internetLinksToggle.isOn) {
            PlayerPrefs.SetInt("AllowLinks", 1);
            

        } else
        {
            PlayerPrefs.SetInt("AllowLinks", 0);
        }

        carrier.AllowLinks = internetLinksToggle.isOn;
        Debug.Log("Carrier got the value " + carrier.AllowLinks + " for AllowLinks");
        Debug.Log("internetLinks, set PlayerPrefs to " + PlayerPrefs.GetInt("AllowLinks"));

    }

    public void SetInternetLinksToggle() {
        //internetLinksToggle.isOn = (PlayerPrefs.GetInt("AllowLinks", 0) != 0);

        //get function has been moved to SettingsCarrier
        internetLinksToggle.isOn = carrier.AllowLinks;

        
        //Debug.Log("internetLinksToggle is set");

       
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
        carrier.ChatMessages = chatMessagesToggle.isOn;
        Debug.Log("Carrier got the value " + carrier.ChatMessages + " for ChatMessages");
    }


    public void SetChatMessagesToggle()
    {
        //chatMessagesToggle.isOn = (PlayerPrefs.GetInt("AllowChat", 0) != 0);
        chatMessagesToggle.isOn = carrier.ChatMessages;
        //Debug.Log("chat, got value" + PlayerPrefs.GetInt("AllowChat"));
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
        carrier.AllowEmojis = emojiCommentsToggle.isOn;


    }
    public void SetEmojisToggle()
    {
        //emojiCommentsToggle.isOn = (PlayerPrefs.GetInt("AllowEmojis", 0) != 0);
        emojiCommentsToggle.isOn = carrier.AllowEmojis;



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

        carrier.AllowTreasureHunt = treasureHuntToggle.isOn;
    }
    public void SetTreasureHuntToggle()
    {
        //treasureHuntToggle.isOn = (PlayerPrefs.GetInt("AllowTreasureHunt", 0) != 0);
        treasureHuntToggle.isOn = carrier.AllowTreasureHunt;

    }





    public void SaveSettings()
    {
        PlayerPrefs.Save();
        //TODO pop up when settings are saved
        settingsSavedPopUp.SetActive(true);
        CloseSettingsPopUp();
        //messageText.text = "Asetukset tallennettu";

    }

    public void LoadSettings()
    {
        //TODO how to load all settings from PlayerPrefs
        //maybe it would be practical to load them all in the same method, maybe not

    }

    public void SetMonthlyLimit()
    {
        //money control: monthly spending limit
        //Debug.Log(monthlyLimitInput.GetType() + " type of monthly limit input field");
        string money = monthlyLimitInput.text;
        //Debug.Log(money.GetType() + "type of money string");
        float moneyFloat = float.Parse(monthlyLimitInput.text);
        //Debug.Log("Got money limit " + moneyFloat);

        /*
        if (float.TryParse(monthlyLimitInput.text, out float limit))
        {
            moneyFloat = limit;
            Debug.Log("Tried float.TryParse for monthly limit got value " + limit);
        }
        else {
            moneyFloat = 0;
        }
        */
        /*
        if (Single.TryParse(monthlyLimitInput.text, out float limit2))
        {
            moneyFloat = limit2;
            Debug.Log("Tried Single.TryParse for monthly limit");
        }
        else
        {
            moneyFloat = 0;
        }
        */

         



        if (money.IsNullOrEmpty())
        {
            PlayerPrefs.SetFloat("MonthlyLimit", 0);
            PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
            PlayerPrefs.Save();


        }
        else if (moneyFloat < 0)
        {
            PlayerPrefs.SetFloat("MonthlyLimit", 0);
            PlayerPrefs.SetFloat("MonthlySpendingLimit", 0);
            Debug.Log("Negative money amount, value thus set to 0");
            //TODO some kind of message, please input a positive value??
            PlayerPrefs.Save();

        }
        else //if (moneyFloat >= 0) 
        {
            PlayerPrefs.SetFloat("MonthlyLimit", float.Parse(money));
            PlayerPrefs.SetFloat("MonthlySpendingLimit", float.Parse(money));
            Debug.Log("Set money limit to" + float.Parse(money));
            PlayerPrefs.Save();


        }
        //monthlyLimitInput.Select();
        

    }

    public void GetMonthlyLimit()
    {
        float getInput = PlayerPrefs.GetFloat("MonthlyLimit");
        monthlyLimitInput.text = getInput.ToString();
        //Debug.Log("Got monthly limit" + getInput);

        //float getInput2 = PlayerPrefs.GetFloat("MonthlySpendingLimit");
       // monthlyLimitInput.text = getInput2.ToString();
        

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

        carrier.ActivatePurchasesSeparately = independentSpendingActivationToggle.isOn;
    }
    public void SetIndependentSpendingActivationToggle()
    {
        //independentSpendingActivationToggle.isOn = (PlayerPrefs.GetInt("ActivatePurchasesSeparately", 0) != 0);
        independentSpendingActivationToggle.isOn = carrier.ActivatePurchasesSeparately;


    }


    public void SetTimeLimit()
    {
        if (float.TryParse(timeLimitInput.text, out float time))
        {
            PlayerPrefs.SetFloat("MaxPlayTime", time);
            PlayerPrefs.Save();
        }

        /*
        else {
            PlayerPrefs.SetFloat("MaxPlayTime", 0);
            PlayerPrefs.Save();
        }
        */
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
        carrier.EndMidMatch = midMatchToggle.isOn;
    }
    public void SetEndMidMatchToggle()
    {
        //midMatchToggle.isOn = (PlayerPrefs.GetInt("EndMidMatch", 0) != 0);
        midMatchToggle.isOn = carrier.EndMidMatch;


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
        carrier.EndAfterMatch = endMatchToggle.isOn;
    }
    public void SetEndAfterMatchToggle()
    {
        //endMatchToggle.isOn = (PlayerPrefs.GetInt("EndAfterMatch", 0) != 0);
        endMatchToggle.isOn = carrier.EndAfterMatch;


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


    public void CloseSettingsPopUp()
    {
        StartCoroutine(RemoveAfterSeconds(2));
    }

    IEnumerator RemoveAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        settingsSavedPopUp.SetActive(false);
    }




}
