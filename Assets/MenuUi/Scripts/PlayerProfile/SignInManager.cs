using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        [Header("Buttons")]
        [SerializeField] private Button logInButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button backButton;


        [Header("Navigation Buttons")]
        [SerializeField] private Button returnToMainMenuButton;
        [SerializeField] private Button returnToLogInScreenButton;

        private const string REGISTERING_SUCCESS = "Rekisteröinti onnistui!";
        private const string ERROR_DEFAULT = "Jotain meni pieleen!";
        private const string ERROR_EMPTY_FIELD = "Kentät eivät voi olla tyhjiä!";
        private const string ERROR_PASSWORD_MISMATCH = "Salasananat eivät täsmää!";
        private const string ERROR_USERNAME_TOO_SHORT = "Käyttäjänimen täytyy olla vähintään 3 merkkiä pitkä!";
        private const string ERROR_PASSWORD_TOO_SHORT = "Salasanan täytyy olla vähintään 5 merkkiä pitkä!";
        private const string ERROR400 = "Validointivirhe!";
        private const string ERROR401 = "Virheellinen käyttäjänimi tai salasana!";
        private const string ERROR409 = "Käyttäjätili on jo olemassa!";
        private const string ERROR500 = "Serverivirhe!";

        private void OnEnable()
        {
            Reset();
            signInWindow.SetActive(true);
            registerWindow.SetActive(false);
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                backButton.gameObject.SetActive(false);
            }
            else
            {
                backButton.gameObject.SetActive(true);
            }
        }

        public void Reset()
        {
            Debug.Log("resetting");

            ShowMessage("", Color.white);

            logInUsernameInputField.text = "";
            logInPasswordInputField.text = "";
            registerUsernameInputField.text = "";
            registerPasswordInputField.text = "";
            registerPassword2InputField.text = "";
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        public void LogIn()
        {
            ClearMessage();

            if (logInUsernameInputField.text == string.Empty || logInPasswordInputField.text == string.Empty)
            {
                ShowMessage(ERROR_EMPTY_FIELD, Color.red);
                return;
            }

            string body = "{\"username\":\"" + logInUsernameInputField.text + "\",\"password\":\"" + logInPasswordInputField.text + "\"}";

            StartCoroutine(WebRequests.Post(ServerManager.ADDRESS + "auth/signIn", body, null, request =>
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
                    ServerManager.Instance.SetProfileValues(result);
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
                return;
            }

            if (password1 != password2)
            {
                ShowMessage(ERROR_PASSWORD_MISMATCH, Color.red);
                return;
            }

            if (password1.Length < _passwordMinLength)
            {
                ShowMessage(ERROR_PASSWORD_TOO_SHORT, Color.red);
                return;
            }

            if (username.Length < _userNameMinLength)
            {
                ShowMessage(ERROR_USERNAME_TOO_SHORT, Color.red);
                return;
            }


            string body = @$"{{""username"":""{registerUsernameInputField.text}"",""password"":""{registerPasswordInputField.text}"",""Player"":{{""name"":""{username}"",""backpackCapacity"":{backpackCapacity},""uniqueIdentifier"":""{username}""}}}}";
            StartCoroutine(WebRequests.Post(ServerManager.ADDRESS + "profile", body, null, request =>
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
                    returnToLogInScreenButton.onClick.Invoke();
                    ShowMessage(REGISTERING_SUCCESS, Color.green);
                }

                registerButton.interactable = true;
            }));
        }

        private void ShowMessage(string message, Color textColor)
        {
            TextMeshProUGUI text = GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>();
            text.text = message;
            text.color = textColor;
        }

        private void ClearMessage()
        {
            GameObject.Find("ErrorText").GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
