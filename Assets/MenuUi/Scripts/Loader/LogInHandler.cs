using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Login;
using MenuUi.Scripts.Window;
using UnityEngine;

namespace MenuUi.Scripts.Loader
{
    public class LogInHandler : MonoBehaviour
    {
        [SerializeField]
        private SignInManager _signInManager;
        [SerializeField]
        private LogInPanelSuccessHandler _loginSuccess;
        [SerializeField]
        private WindowNavigation _privacyNavigation;
        [SerializeField]
        private WindowNavigation _mainMenuNavigation;
        [SerializeField]
        private WindowNavigation _introStoryNavigation;

        // Start is called before the first frame update
        void Start()
        {
            /*ServerManager.OnLogInFailed += OpenLogInScreen;
            ServerManager.OnLogInStatusChanged += MoveToMain;
            CheckPrivacy();*/
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            ServerManager.OnLogInFailed += OpenLogInScreen;
            ServerManager.OnLogInStatusChanged += MoveToMain;
            _loginSuccess.OnLogInPanelSuccess += CloseLogInScreen;
            CheckPrivacy();
        }

        private void OnDisable()
        {
            ServerManager.OnLogInFailed -= OpenLogInScreen;
            ServerManager.OnLogInStatusChanged -= MoveToMain;
            _loginSuccess.OnLogInPanelSuccess -= CloseLogInScreen;
        }

        private void CheckPrivacy()
        {
            if (PlayerPrefs.GetInt("PrivacyPolicy") == 0)
                _privacyNavigation.Navigate();
            else
            {
                AttemptLogIn();
            }
        }

        private void AttemptLogIn()
        {
            StartCoroutine(ServerManager.Instance.LogIn());
        }

        private void OpenLogInScreen()
        {
            _signInManager.gameObject.SetActive(true);
        }

        private void CloseLogInScreen()
        {
            _signInManager.gameObject.SetActive(false);
            AttemptLogIn();
        }

        private void MoveToMain(bool value)
        {
            if (value)
            {
                PlayerData playerData = null;
                Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => playerData = p);

                if (playerData.SelectedCharacterId == 0)
                    _introStoryNavigation.Navigate();
                else
                    _mainMenuNavigation.Navigate();
            }
        }
    }
}
