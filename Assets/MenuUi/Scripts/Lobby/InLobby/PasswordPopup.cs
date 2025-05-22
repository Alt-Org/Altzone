using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles opening the password popup and asking for a password and closing it.
    /// </summary>
    public class PasswordPopup : MonoBehaviour
    {
        [SerializeField] TMP_InputField _passwordInput;
        [SerializeField] Button _confirmButton;

        /// <summary>
        /// Open password popup and ask for password. Return the password player typed as a string parameter to the callback.
        /// </summary>
        /// <param name="callback">Callback function which takes string as a parameter.</param>
        public IEnumerator AskForPassword(System.Action<string> callback)
        {
            gameObject.SetActive(true);

            bool? confirmPressed = null;

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() => confirmPressed = true);

            yield return new WaitUntil(() => confirmPressed.HasValue);

            callback(_passwordInput.text);
        }

        /// <summary>
        /// Closes the password popup.
        /// </summary>
        public void ClosePopup()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
            _passwordInput.text = string.Empty;
        }
    }
}

