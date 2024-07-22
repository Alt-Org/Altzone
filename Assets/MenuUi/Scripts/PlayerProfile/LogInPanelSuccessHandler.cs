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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LogInSuccess()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                OnLogInPanelSuccess?.Invoke();
            }
            else if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                StartCoroutine(ServerManager.Instance.LogIn());
                _navigation.Navigate();
            }
        }
    }
}
