using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
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

                ShowMessage(errorString + "\n" + request.error, Color.red);
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

                ShowMessage(errorString + "\n" + request.error, Color.red);
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
                ShowMessage(errorString + "\n" + request.error, Color.red);
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
