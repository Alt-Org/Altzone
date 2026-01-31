using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Login;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Loader
{
    public class LogInHandler : AltMonoBehaviour
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
        private ChangeAccountHandler _changeAccountHandler;

        [SerializeField]
        private WindowNavigation _privacyNavigation;
        [SerializeField]
        private WindowNavigation _mainMenuNavigation;
        [SerializeField]
        private WindowNavigation _introStoryNavigation;
        [SerializeField]
        private WindowNavigation _languageNavigation;

        private string _currentAccessToken = null;

        private void OnEnable()
        {
            ServerManager.OnLogInFailed += OpenLogInScreen;
            ServerManager.OnLogInStatusChanged += PlayerDataFetched;
            ServerManager.OnClanFetchFinished += LogInReady;
            _loginSuccess.OnLogInPanelSuccess += CloseLogInScreen;
            _loginSuccess.OnLogInPanelReturn += CloseLogInScreen;
            _changeAccountHandler.OnChangeAccountEvent += ChangeAccount;
            _loadInfoController.OnMoveToMain += MoveToMain;
            StartCoroutine(WaitVersionCheck());
        }

        private void OnDisable()
        {
            ServerManager.OnLogInFailed -= OpenLogInScreen;
            ServerManager.OnLogInStatusChanged -= PlayerDataFetched;
            ServerManager.OnClanFetchFinished -= LogInReady;
            _loginSuccess.OnLogInPanelSuccess -= CloseLogInScreen;
            _loginSuccess.OnLogInPanelReturn -= CloseLogInScreen;
            _changeAccountHandler.OnChangeAccountEvent -= ChangeAccount;
            _loadInfoController.OnMoveToMain -= MoveToMain;
        }

        private IEnumerator WaitVersionCheck()
        {
            _loadInfoController.Status = LogInStatus.VersionCheck;
            yield return new WaitUntil(() => GameLoader.Instance.VersionCheckPassed.HasValue);
            CheckLanguage();
        }

        private void CheckLanguage()
        {
            if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.None)
                StartCoroutine(_languageNavigation.Navigate());
            else
            {
                CheckPrivacy();
            }
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
            _loadInfoController.LogIn();
            StartCoroutine(ServerManager.Instance.LogIn());
        }

        private void OpenLogInScreen()
        {
            _loadInfoController.DisableInfo();
            _signInManager.gameObject.SetActive(true);
        }

        private void CloseLogInScreen(bool useSavedToken = false)
        {
            _signInManager.gameObject.SetActive(false);
            if (!ServerManager.Instance.isLoggedIn)
            {
                if (useSavedToken) ServerManager.Instance.AccessToken = _currentAccessToken;
                AttemptLogIn();
            }
            else
            {
                LogInReady();
            }
        }

        private void ChangeAccount()
        {
            _currentAccessToken = ServerManager.Instance.AccessToken;
            _changeAccountHandler.gameObject.SetActive(false);
            OpenLogInScreen();
        }

        private void PlayerDataFetched(bool value)
        {
            _loadInfoController.Status = LogInStatus.FetchClanData;
        }

        private void LogInReady()
        {
            if (AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
            {
                _loadInfoController.LoadReady();
                _changeAccountHandler.gameObject.SetActive(true);
            }
            else
            {
                MoveToMain();
            }
        }

        private void MoveToMain()
        {
            if (true)
            {
                StartCoroutine(MoveToMainCoroutine());
            }
        }

        private IEnumerator MoveToMainCoroutine()
        {
            //_loadInfoController.SetInfoText(LogInStatus.FetchPlayerData);
            PlayerData playerData = null;
            StartCoroutine(GetPlayerData(p => playerData = p));

            yield return new WaitUntil(() => playerData != null);

            if (ServerManager.Instance.Player.above13 == null)
            {
                _ageVerificationHandler.gameObject.SetActive(true);
            }
            else { _ageVerificationHandler.Finished = true; }

            yield return new WaitUntil(() => _ageVerificationHandler.Finished );

            if ((ServerManager.Instance.Player.currentAvatarId == null) || ((CharacterID)ServerManager.Instance.Player.currentAvatarId)== CharacterID.None || !Enum.IsDefined(typeof(CharacterID), ServerManager.Instance.Player.currentAvatarId))
                StartCoroutine(_introStoryNavigation.Navigate());
            else
            {
                _loadInfoController.Status = LogInStatus.MovingToMain;
                StartCoroutine(_mainMenuNavigation.Navigate());
            }
        }
    }
}
