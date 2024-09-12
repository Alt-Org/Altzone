using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUi.Scripts.Login
{

    public class LogInPanelSuccessHandler : MonoBehaviour
    {
        [SerializeField]
        private WindowNavigation _navigation;

        public delegate void LogInPanelSuccess();
        public event LogInPanelSuccess OnLogInPanelSuccess;

        public void LogInSuccess()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                OnLogInPanelSuccess?.Invoke();
            }
            else if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                StartCoroutine(ServerManager.Instance.LogIn());
                StartCoroutine(_navigation.Navigate());
            }
        }
    }
}
