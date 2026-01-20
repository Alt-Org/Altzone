using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Prg.Scripts.Common.Unity;
using Altzone.Scripts.Config;
using Altzone.Scripts.Language;
using MenuUI.Scripts;

namespace MenuUi.Scripts.Login
{

    public class SignInManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _userNameMinLength;
        [SerializeField] private int _passwordMinLength;

        [Header("Windows")]
        [SerializeField] private GameObject _signInWindow;
        [SerializeField] private GameObject _registerWindow;


        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _logInUsernameInputField;
        [SerializeField] private TMP_InputField _logInPasswordInputField;
        [SerializeField] private TMP_InputField _registerUsernameInputField;
        [SerializeField] private TMP_InputField _registerPasswordInputField;
        [SerializeField] private TMP_InputField _registerPassword2InputField;
        [SerializeField] private Toggle _privacyPolicyAuthToggle;
        [SerializeField] private Toggle _registerAgeVerificationCheckToggle;
        [SerializeField] private Toggle _registerAgeVerificationToggle;
        [SerializeField] private Toggle _registerParentalAuthToggle;
        [SerializeField] private Toggle _informationPolicyAuthToggle;
        [SerializeField] private ToggleGroup _ageAuthToggleGroup;


        [Header("Input Fields Errors")]
        [SerializeField] private Image _logInUsernameInputFieldError;
        [SerializeField] private Image _logInPasswordInputFieldError;
        [SerializeField] private Image _registerUsernameInputFieldError;
        [SerializeField] private Image _registerPasswordInputFieldError;
        [SerializeField] private Image _registerPassword2InputFieldError;
        [SerializeField] private Image _privacyPolicyToggleError;
        [SerializeField] private Image _registerAgeVerificationToggleError;
        [SerializeField] private Image _informationPolicyToggleError;

        [Header("Buttons")]
        [SerializeField] private Button _logInButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _backButton2;
        [SerializeField] private Button _ageAuthButton;

        [Header("Version Toggle")]
        [SerializeField] private ToggleSwitchHandler _autoLoginToggle;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _returnToLogIn;
        [SerializeField] private Button _returnToMainMenuButton;
        [SerializeField] private Button _returnToSignInScreenButton;

        private VersionType _versionType = VersionType.None;

        private const string REGISTERING_SUCCESS = "Rekisteröinti onnistui!";
        private const string ERROR_DEFAULT = "Jotain meni pieleen!";
        private const string ERROR_EMPTY_FIELD = "Kentät eivät voi olla tyhjiä!";
        private const string ERROR_PASSWORD_MISMATCH = "Salasananat eivät täsmää!";
        private const string ERROR_USERNAME_TOO_SHORT = "Käyttäjänimen täytyy olla vähintään 3 merkkiä pitkä!";
        private const string ERROR_PASSWORD_TOO_SHORT = "Salasanan täytyy olla vähintään 5 merkkiä pitkä!";
        private const string ERROR_PRIVACY_CONCENT_NOT_GRANTED = "Et ole hyväksynyt tietosuojaselostetta.";
        private const string ERROR_AGE_CONSENT_NOT_GRANTED = "Et ole vahvistanut olevasi yli 13-vuotias tai että sinulla on huoltajan lupa pelata peliä";
        private const string ERROR_INFORMATION_CONCENT_NOT_GRANTED = "Et ole antanut lupaa tietojen käyttää pelin hallinnoinnnissa.";
        private const string ERROR400 = "Validointivirhe!";
        private const string ERROR401 = "Virheellinen käyttäjänimi tai salasana!";
        private const string ERROR409 = "Käyttäjätili on jo olemassa!";
        private const string ERROR500 = "Serverivirhe!";

        private void OnEnable()
        {
            Reset();
            _signInWindow.SetActive(true);
            _registerWindow.SetActive(false);
            if (ServerManager.Instance.Player == null)
            {
                _backButton.gameObject.SetActive(false);
            }
            else
            {
                _backButton.gameObject.SetActive(true);
            }
            _backButton2.gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(ReturnToLogIn);
            }
            _autoLoginToggle.SetState(PlayerPrefs.GetInt("AutomaticLogin", 0) != 0);


            /*if (GameConfig.Get().GameVersionType is VersionType.Standard or VersionType.None)
            {
                SetVersionState(false);
            }
            else if(GameConfig.Get().GameVersionType is VersionType.Education)
            {
                SetVersionState(true);
            }*/
            _autoLoginToggle.OnToggleStateChanged += SetVersionState;
        }

        public void Reset()
        {
            Debug.Log("resetting");

            ClearMessage();

            _logInUsernameInputField.text = "";
            _logInPasswordInputField.text = "";
            _registerUsernameInputField.text = "";
            _registerPasswordInputField.text = "";
            _registerPassword2InputField.text = "";
        }

        private void OnDisable()
        {
            _autoLoginToggle.OnToggleStateChanged -= SetVersionState;
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        public void LogIn(bool guest)
        {
            string body = "";
            if (guest)
            {
                body = "{\"username\":\"Angel42\",\"password\":\"PRIbXCI9d)Z0UoHP\"}";

            }
            else
            {
                ClearMessage();

                if (_logInUsernameInputField.text == string.Empty || _logInPasswordInputField.text == string.Empty)
                {
                    ShowMessage(ERROR_EMPTY_FIELD, Color.red);
                    if (_logInUsernameInputField.text == string.Empty) _logInUsernameInputFieldError.gameObject.SetActive(true);
                    else _logInPasswordInputFieldError.gameObject.SetActive(true);
                    return;
                }
                ServerLogIn(_logInUsernameInputField.text, _logInPasswordInputField.text);
            }
        }
        private void ServerLogIn(string username, string password) {
            string body = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";

            StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "auth/signIn", body, null, request =>
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
                            _logInUsernameInputFieldError.gameObject.SetActive(true);
                            _logInPasswordInputFieldError.gameObject.SetActive(true);
                            break;
                        case 401:
                            errorString = ERROR401;
                            break;
                        case 500:
                            errorString = ERROR500;
                            break;
                    }

                    ShowMessage(errorString + "\n" + request.error, Color.red);

                }
                else
                {
                // Parses user info and sends it to ServerManager
                Debug.Log("Log in successful!");
                    JObject result = JObject.Parse(request.downloadHandler.text);
                    //Debug.Log(request.downloadHandler.text);
                    if(ServerManager.Instance.isLoggedIn) ServerManager.Instance.LogOut();
                    ServerManager.Instance.SetProfileValues(result);
                    GameConfig.Get().GameVersionType = VersionType.Education;
                    if (_autoLoginToggle.IsOn)
                    {
                        PlayerPrefs.SetInt("AutomaticLogin", 1);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("AutomaticLogin", 0);
                    }
                    _returnToMainMenuButton.onClick.Invoke();
                }

                _logInButton.interactable = true;
            }));
        }
        public void Register()
        {
            ClearMessage();

            string username = _registerUsernameInputField.text;
            string password1 = _registerPasswordInputField.text;
            string password2 = _registerPassword2InputField.text;

            // Checks empty fields and password requirements
            if (_registerUsernameInputField.text == string.Empty || _registerPasswordInputField.text == string.Empty || _registerPassword2InputField.text == string.Empty)
            {
                ShowMessage(ERROR_EMPTY_FIELD, Color.red);
                _registerUsernameInputFieldError.gameObject.SetActive(true);
                return;
            }

            if (password1 != password2)
            {
                ShowMessage(ERROR_PASSWORD_MISMATCH, Color.red);
                _registerPassword2InputFieldError.gameObject.SetActive(true);
                return;
            }

            if (password1.Length < _passwordMinLength)
            {
                ShowMessage(ERROR_PASSWORD_TOO_SHORT, Color.red);
                _registerPasswordInputFieldError.gameObject.SetActive(true);
                return;
            }

            if (username.Length < _userNameMinLength)
            {
                ShowMessage(ERROR_USERNAME_TOO_SHORT, Color.red);
                _registerUsernameInputFieldError.gameObject.SetActive(true);
                return;
            }

            if (!_registerAgeVerificationToggle.isOn)
            {
                ShowMessage(ERROR_AGE_CONSENT_NOT_GRANTED, Color.red);
                _registerAgeVerificationToggleError.gameObject.SetActive(true);
                return;
            }

            if (!_privacyPolicyAuthToggle.isOn)
            {
                ShowMessage(ERROR_PRIVACY_CONCENT_NOT_GRANTED, Color.red);
                _privacyPolicyToggleError.gameObject.SetActive(true);
                return;
            }

            if (!_informationPolicyAuthToggle.isOn)
            {
                ShowMessage(ERROR_INFORMATION_CONCENT_NOT_GRANTED, Color.red);
                _informationPolicyToggleError.gameObject.SetActive(true);
                return;
            }


            string body = @$"{{""username"":""{_registerUsernameInputField.text}"",""password"":""{_registerPasswordInputField.text}"",
                ""Player"":{{""name"":""{username}"",""backpackCapacity"":{255},""uniqueIdentifier"":""{username}"",
                    ""above13"":{_registerAgeVerificationToggle.isOn.ToString().ToLower()},""parentalAuth"":{_registerParentalAuthToggle.isOn.ToString().ToLower()}}}}}";
            StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "profile", body, null, request =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorString = string.Empty;

                    switch (request.responseCode)
                    {
                        default:
                            errorString = ERROR_DEFAULT;
                            break;
                        case 409:
                            errorString = ERROR409;
                            _registerUsernameInputFieldError.gameObject.SetActive(true);
                            break;
                        case 500:
                            errorString = ERROR500;
                            break;
                    }

                    ShowMessage(errorString + "\n" + request.error, Color.red);
                }
                else
                {
                    Debug.Log("Registering successful!");
                    string username = _registerUsernameInputField.text;
                    string password = _registerPasswordInputField.text;
                    _returnToSignInScreenButton.onClick.Invoke();
                    ShowMessage(REGISTERING_SUCCESS, Color.green);
                    JObject result = JObject.Parse(request.downloadHandler.text);
                    //Debug.Log(request.downloadHandler.text);
                    if (ServerManager.Instance.isLoggedIn) ServerManager.Instance.LogOut();
                    ServerLogIn(username, password);
                }

                _registerButton.interactable = true;
            }));
        }

        public void GuestLogin()
        {
            string body = "";

            StartCoroutine(WebRequests.Post(ServerManager.SERVERADDRESS + "profile/guest", body, null, request =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string errorString = string.Empty;

                    switch (request.responseCode)
                    {
                        default:
                            errorString = ERROR_DEFAULT;
                            break;
                        case 409:
                            errorString = ERROR409;
                            _registerUsernameInputFieldError.gameObject.SetActive(true);
                            break;
                        case 500:
                            errorString = ERROR500;
                            break;
                    }

                    ShowMessage(errorString + "\n" + request.error, Color.red);
                }
                else
                {
                    Debug.Log("Registering successful!");
                    JObject result = JObject.Parse(request.downloadHandler.text);;
                    if (ServerManager.Instance.isLoggedIn) ServerManager.Instance.LogOut();
                    ServerManager.Instance.SetProfileValues(result);
                    GameConfig.Get().GameVersionType = VersionType.Education;
                    PlayerPrefs.SetInt("AutomaticLogin", 1);
                    _returnToMainMenuButton.onClick.Invoke();
                }
            }));
        }

        private void ShowMessage(string message, Color textColor)
        {
            SignalBus.OnChangePopupInfoSignal(message);
        }

        private void ClearMessage()
        {
            _logInUsernameInputFieldError.gameObject.SetActive(false);
            _logInPasswordInputFieldError.gameObject.SetActive(false);
            _registerUsernameInputFieldError.gameObject.SetActive(false);
            _registerPasswordInputFieldError.gameObject.SetActive(false);
            _registerPassword2InputFieldError.gameObject.SetActive(false);
            _registerAgeVerificationToggleError.gameObject.SetActive(false);
        }

        public void CheckToggle()
        {
            if (_ageAuthToggleGroup.AnyTogglesOn())
            {
                _ageAuthButton.interactable = true;
                _registerAgeVerificationCheckToggle.isOn = true;

            }
            else
            {
                _ageAuthButton.interactable = false;
                _registerAgeVerificationCheckToggle.isOn = false;
            }
        }
        private void ReturnToLogIn()
        {
            _returnToLogIn.onClick?.Invoke();
        }
        private void SetVersionState(bool value)
        {
            if (value)
            {
                //PlayerPrefs.SetInt("AutomaticLogin", 1);
                _autoLoginToggle.SetState(value);
            }
            else
            {
                //PlayerPrefs.SetInt("AutomaticLogin", 0);
                _autoLoginToggle.SetState(value);
            }
        }
    }
}
