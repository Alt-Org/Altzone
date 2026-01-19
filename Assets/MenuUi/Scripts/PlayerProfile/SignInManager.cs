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
        [SerializeField] private int backpackCapacity;

        [Header("Windows")]
        [SerializeField] private GameObject signInWindow;
        [SerializeField] private GameObject registerWindow;


        [Header("Input Fields")]
        [SerializeField] private TMP_InputField logInUsernameInputField;
        [SerializeField] private TMP_InputField logInPasswordInputField;
        [SerializeField] private TMP_InputField registerUsernameInputField;
        [SerializeField] private TMP_InputField registerPasswordInputField;
        [SerializeField] private TMP_InputField registerPassword2InputField;
        [SerializeField] private Toggle _privacyPolicyAuthToggle;
        [SerializeField] private Toggle _registerAgeVerificationCheckToggle;
        [SerializeField] private Toggle _registerAgeVerificationToggle;
        [SerializeField] private Toggle _registerParentalAuthToggle;
        [SerializeField] private Toggle _informationPolicyAuthToggle;
        [SerializeField] private ToggleGroup _ageAuthToggleGroup;


        [Header("Input Fields Errors")]
        [SerializeField] private Image logInUsernameInputFieldError;
        [SerializeField] private Image logInPasswordInputFieldError;
        [SerializeField] private Image registerUsernameInputFieldError;
        [SerializeField] private Image registerPasswordInputFieldError;
        [SerializeField] private Image registerPassword2InputFieldError;
        [SerializeField] private Image _privacyPolicyToggleError;
        [SerializeField] private Image _registerAgeVerificationToggleError;
        [SerializeField] private Image _informationPolicyToggleError;

        [Header("Buttons")]
        [SerializeField] private Button logInButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button backButton2;
        [SerializeField] private Button ageAuthButton;

        [Header("Version Toggle")]
        [SerializeField] private ToggleSwitchHandler _autoLoginToggle;

        [Header("Navigation Buttons")]
        [SerializeField] private Button returnToLogIn;
        [SerializeField] private Button returnToMainMenuButton;
        [SerializeField] private Button returnToSignInScreenButton;

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
            signInWindow.SetActive(true);
            registerWindow.SetActive(false);
            if (ServerManager.Instance.Player == null)
            {
                backButton.gameObject.SetActive(false);
            }
            else
            {
                backButton.gameObject.SetActive(true);
            }
            backButton2.gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(ReturnToLogIn);
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

            logInUsernameInputField.text = "";
            logInPasswordInputField.text = "";
            registerUsernameInputField.text = "";
            registerPasswordInputField.text = "";
            registerPassword2InputField.text = "";
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

                if (logInUsernameInputField.text == string.Empty || logInPasswordInputField.text == string.Empty)
                {
                    ShowMessage(ERROR_EMPTY_FIELD, Color.red);
                    if(logInUsernameInputField.text == string.Empty) logInUsernameInputFieldError.gameObject.SetActive(true);
                    else logInPasswordInputFieldError.gameObject.SetActive(true);
                    return;
                }

                body = "{\"username\":\"" + logInUsernameInputField.text + "\",\"password\":\"" + logInPasswordInputField.text + "\"}";
            }
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
                            logInUsernameInputFieldError.gameObject.SetActive(true);
                            logInPasswordInputFieldError.gameObject.SetActive(true);
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
                    Debug.Log(request.downloadHandler.text);
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
                    returnToMainMenuButton.onClick.Invoke();
                }

                logInButton.interactable = true;
            }));
        }
        public void Register()
        {
            ClearMessage();

            string username = registerUsernameInputField.text;
            string password1 = registerPasswordInputField.text;
            string password2 = registerPassword2InputField.text;

            // Checks empty fields and password requirements
            if (registerUsernameInputField.text == string.Empty || registerPasswordInputField.text == string.Empty || registerPassword2InputField.text == string.Empty)
            {
                ShowMessage(ERROR_EMPTY_FIELD, Color.red);
                registerUsernameInputFieldError.gameObject.SetActive(true);
                return;
            }

            if (password1 != password2)
            {
                ShowMessage(ERROR_PASSWORD_MISMATCH, Color.red);
                registerPassword2InputFieldError.gameObject.SetActive(true);
                return;
            }

            if (password1.Length < _passwordMinLength)
            {
                ShowMessage(ERROR_PASSWORD_TOO_SHORT, Color.red);
                registerPasswordInputFieldError.gameObject.SetActive(true);
                return;
            }

            if (username.Length < _userNameMinLength)
            {
                ShowMessage(ERROR_USERNAME_TOO_SHORT, Color.red);
                registerUsernameInputFieldError.gameObject.SetActive(true);
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


            string body = @$"{{""username"":""{registerUsernameInputField.text}"",""password"":""{registerPasswordInputField.text}"",
                ""Player"":{{""name"":""{username}"",""backpackCapacity"":{backpackCapacity},""uniqueIdentifier"":""{username}"",
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
                            registerUsernameInputFieldError.gameObject.SetActive(true);
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
                    returnToSignInScreenButton.onClick.Invoke();
                    ShowMessage(REGISTERING_SUCCESS, Color.green);
                    JObject result = JObject.Parse(request.downloadHandler.text);
                    Debug.Log(request.downloadHandler.text);
                    if (ServerManager.Instance.isLoggedIn) ServerManager.Instance.LogOut();
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
                    returnToMainMenuButton.onClick.Invoke();

                }

                registerButton.interactable = true;
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
                            registerUsernameInputFieldError.gameObject.SetActive(true);
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
                    Debug.Log(request.downloadHandler.text);
                    if (ServerManager.Instance.isLoggedIn) ServerManager.Instance.LogOut();
                    ServerManager.Instance.SetProfileValues(result);
                    GameConfig.Get().GameVersionType = VersionType.Education;
                    PlayerPrefs.SetInt("AutomaticLogin", 1);
                    returnToMainMenuButton.onClick.Invoke();
                }
            }));
        }

        private void ShowMessage(string message, Color textColor)
        {
            //TextMeshProUGUI text = GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>();
            //text.text = message;
            //text.color = textColor;
            SignalBus.OnChangePopupInfoSignal(message);
        }

        private void ClearMessage()
        {
            //GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>().text = "";
            logInUsernameInputFieldError.gameObject.SetActive(false);
            logInPasswordInputFieldError.gameObject.SetActive(false);
            registerUsernameInputFieldError.gameObject.SetActive(false);
            registerPasswordInputFieldError.gameObject.SetActive(false);
            registerPassword2InputFieldError.gameObject.SetActive(false);
            _registerAgeVerificationToggleError.gameObject.SetActive(false);
        }

        public void CheckToggle()
        {
            if (_ageAuthToggleGroup.AnyTogglesOn())
            {
                ageAuthButton.interactable = true;
                _registerAgeVerificationCheckToggle.isOn = true;

            }
            else
            {
                ageAuthButton.interactable = false;
                _registerAgeVerificationCheckToggle.isOn = false;
            }
        }
        private void ReturnToLogIn()
        {
            returnToLogIn.onClick?.Invoke();
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
