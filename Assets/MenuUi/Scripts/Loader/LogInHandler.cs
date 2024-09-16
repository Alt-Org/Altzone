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
        private LoadingInfoController _loadInfoController;
        [SerializeField]
        private AgeVerificationHandler _ageVerificationHandler;
        [SerializeField]
        private WindowNavigation _privacyNavigation;
        [SerializeField]
        private WindowNavigation _mainMenuNavigation;
        [SerializeField]
        private WindowNavigation _introStoryNavigation;

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
                StartCoroutine(_privacyNavigation.Navigate());
            else
            {
                AttemptLogIn();
            }
        }

        private void AttemptLogIn()
        {
            _loadInfoController.SetInfoText(InfoType.LogIn);
            StartCoroutine(ServerManager.Instance.LogIn());
        }

        private void OpenLogInScreen()
        {
            _loadInfoController.DisableInfo();
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
                StartCoroutine(MoveToMain());
            }
        }

        private IEnumerator MoveToMain()
        {
            _loadInfoController.SetInfoText(InfoType.FetchPlayerData);
            PlayerData playerData = null;
            Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p => playerData = p);

            if (ServerManager.Instance.Player.above13 == null)
            {
                _ageVerificationHandler.gameObject.SetActive(true);
            }

            yield return new WaitUntil(() => _ageVerificationHandler.Finished);

            if ((playerData.SelectedCharacterId == 0) || (playerData.SelectedCharacterId == 1))
                StartCoroutine(_introStoryNavigation.Navigate());
            else
            {
                _loadInfoController.SetInfoText(InfoType.Finished);
                StartCoroutine(_mainMenuNavigation.Navigate());
            }
        }
    }
}
