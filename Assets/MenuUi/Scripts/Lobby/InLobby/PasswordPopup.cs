using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InLobby
{
    public class PasswordPopup : MonoBehaviour
    {
        [SerializeField] TMP_InputField _passwordInput;
        [SerializeField] Button _confirmButton;

        public IEnumerator AskForPassword(System.Action<string> callback)
        {
            gameObject.SetActive(true);

            bool? confirmPressed = null;

            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() => confirmPressed = true);

            yield return new WaitUntil(() => confirmPressed.HasValue);

            callback(_passwordInput.text);
        }

        public void ClosePopup()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
            _passwordInput.text = string.Empty;
        }
    }
}

