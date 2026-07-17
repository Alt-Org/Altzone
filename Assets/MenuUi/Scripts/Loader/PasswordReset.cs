using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using MenuUI.Scripts;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordReset : MonoBehaviour
{
    [SerializeField] private GameObject _passwordResetPanel;

    [SerializeField] private GameObject _passwordResetAccountNameSection;
    [SerializeField] private TMP_InputField _accountNameField;
    [SerializeField] private Button _accountNameButton;

    [SerializeField] private GameObject _passwordResetQuestionSection;
    [SerializeField] private TextLanguageSelectorCaller _questionLabel;
    [SerializeField] private TMP_InputField _answerField;
    [SerializeField] private Button _answerButton;

    [SerializeField] private GameObject _passwordResetInputSection;
    [SerializeField] private TMP_InputField _passwordResetField;
    [SerializeField] private Button _passwordResetButton;

    private string _resetToken = null;

    private const string ERROR_DEFAULT_FI = "Jotain meni pieleen!";
    private const string ERROR_DEFAULT_EN = "Something went wrong!";
    private const string ERROR400_FI = "Väärä vastaus!";
    private const string ERROR400_EN = "Wrong Answer!";
    private const string ERROR401_FI = "Virhellinen authentikointi!";
    private const string ERROR401_EN = "Invalid Authentication!";
    private const string ERROR404CONNECTION_FI = "Serveriyhteyttä ei voitu luoda! Tarkista internet-yhteksesi.";
    private const string ERROR404CONNECTION_EN = "Address not found! Check your internet connection.";
    private const string ERROR404MISSINGUSER_FI = "Haettua käyttäjää ei löydetty serveriltä!";
    private const string ERROR404MISSINGUSER_EN = "Desired user not found on server!";
    private const string ERROR500_FI = "Serverivirhe!";
    private const string ERROR500_EN = "Server Error!";

    public static string ERROR_DEFAULT
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR_DEFAULT_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR_DEFAULT_EN;
            else return ERROR_DEFAULT_FI;
        }
    }

    public static string ERROR400
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR400_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR400_EN;
            else return ERROR400_FI;
        }
    }

    public static string ERROR401
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR401_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR401_EN;
            else return ERROR401_FI;
        }
    }
    public static string ERROR404CONNECTION
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR404CONNECTION_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR404CONNECTION_EN;
            else return ERROR404CONNECTION_FI;
        }
    }
    public static string ERROR404MISSINGUSER
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR404MISSINGUSER_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR404MISSINGUSER_EN;
            else return ERROR404MISSINGUSER_FI;
        }
    }
    public static string ERROR500
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) return ERROR500_FI;
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) return ERROR500_EN;
            else return ERROR500_FI;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        _accountNameButton.onClick.AddListener(() => FetchSecurityQuestion(_accountNameField.text));
        _answerButton.onClick.AddListener(() => CheckAnswer(_accountNameField.text, _answerField.text));
        _passwordResetButton.onClick.AddListener(() => SendNewPassword(_passwordResetField.text));
    }

    private void OnEnable()
    {
        Reset();
    }

    private void Reset()
    {
        _passwordResetAccountNameSection.SetActive(true);
        _passwordResetQuestionSection.SetActive(false);
        _passwordResetInputSection.SetActive(false);

        _accountNameField.text = string.Empty;
        _questionLabel.SetText(string.Empty);
        _answerField.text = string.Empty;
        _passwordResetField.text = string.Empty;

        _resetToken = string.Empty;
    }

    private void FetchSecurityQuestion(string name)
    {
        string body = JObject.FromObject(
            new
            {
                username = name,
            }
        ).ToString();

        StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "profile/securityquestion", body, null, request =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorString = string.Empty;

                switch (request.responseCode)
                {
                    default:
                        errorString = ERROR_DEFAULT;
                        break;
                    case 400:
                        errorString = ERROR400;
                        break;
                    case 404:
                        JObject result = JObject.Parse(request.downloadHandler.text);
                        if (result["errors"] == null)
                            errorString = ERROR404CONNECTION;
                        else if (result["errors"][0]["reason"].ToString().Equals("NOT_FOUND")) 
                            errorString = ERROR404MISSINGUSER;
                        else errorString = ERROR_DEFAULT;
                        break;
                    case 500:
                        errorString = ERROR500;
                        break;
                }

                ShowMessage(errorString, Color.red);
            }
            else
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                _questionLabel.SetText(result["data"]["Profile"]["securityQuestion"].ToString());

                _passwordResetAccountNameSection.SetActive(false);
                _passwordResetQuestionSection.SetActive(true);
            }
        }));
    }

    private void CheckAnswer(string name, string answer)
    {
        string body = JObject.FromObject(
            new
            {
                username = name,
                securityAnswer = answer,
            }
        ).ToString();

        StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "profile/securityanswer", body, null, request =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorString = string.Empty;

                switch (request.responseCode)
                {
                    default:
                        errorString = ERROR_DEFAULT;
                        break;
                    case 400:
                        errorString = ERROR400;
                        break;
                    case 404:
                        JObject result = JObject.Parse(request.downloadHandler.text);
                        if (result["errors"] == null)
                            errorString = ERROR404CONNECTION;
                        else if (result["errors"][0]["reason"].ToString().Equals("NOT_FOUND"))
                            errorString = ERROR404MISSINGUSER;
                        else errorString = ERROR_DEFAULT;
                        break;
                    case 500:
                        errorString = ERROR500;
                        break;
                }

                ShowMessage(errorString, Color.red);
            }
            else
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                _resetToken = result["data"]["Profile"]["resetToken"].ToString();

                _passwordResetQuestionSection.SetActive(false);
                _passwordResetInputSection.SetActive(true);
            }
        }));
    }

    private void SendNewPassword(string password)
    {
        string body = JObject.FromObject(
            new
            {
                resetToken = _resetToken,
                newPassword = password,
            }
        ).ToString();

        StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "profile/resetpassword", body, null, request =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorString = string.Empty;

                switch (request.responseCode)
                {
                    default:
                        errorString = ERROR_DEFAULT;
                        break;
                    case 400:
                        errorString = ERROR400;
                        break;
                    case 404:
                        JObject result = JObject.Parse(request.downloadHandler.text);
                        if (result["errors"] == null)
                            errorString = ERROR404CONNECTION;
                        else if (result["errors"][0]["reason"].ToString().Equals("NOT_FOUND"))
                            errorString = ERROR404MISSINGUSER;
                        else errorString = ERROR_DEFAULT;
                        break;
                    case 500:
                        errorString = ERROR500;
                        break;
                }

                ShowMessage(errorString, Color.red);
            }
            else
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                _passwordResetQuestionSection.SetActive(false);

                _resetToken = string.Empty;
                ClosePopup();
            }
        }));
    }

    public void OpenPopUp()
    {
        _passwordResetPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        _passwordResetPanel.SetActive(false);
    }

    private void ShowMessage(string message, Color textColor)
    {
        SignalBus.OnChangePopupInfoSignal(message);
    }
}
