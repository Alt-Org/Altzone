using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUi.Scripts.Login
{

    public class LogInPanelSuccessHandler : MonoBehaviour
    {
        [SerializeField]
        private WindowNavigation _navigation;
        [SerializeField]
        private WindowNavigation _introStoryNavigation;

        public delegate void LogInPanelSuccess(bool useSetToken);
        public event LogInPanelSuccess OnLogInPanelSuccess;

        public delegate void LogInPanelReturn(bool useSetToken);
        public event LogInPanelReturn OnLogInPanelReturn;

        public void LogInSuccess()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                OnLogInPanelSuccess?.Invoke(false);
            }
            else if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                ServerManager.OnClanFetchFinished += ReturnToMain;
                StartCoroutine(ServerManager.Instance.LogIn());
            }
        }

        public void ReturnToMain()
        {
            ServerManager.OnClanFetchFinished -= ReturnToMain;
            if ((ServerManager.Instance.Player.currentAvatarId == null) || ((CharacterID)ServerManager.Instance.Player.currentAvatarId) == CharacterID.None || !Enum.IsDefined(typeof(CharacterID), ServerManager.Instance.Player.currentAvatarId))
                StartCoroutine(_introStoryNavigation.Navigate());
            else
                StartCoroutine(_navigation.Navigate());
        }

        public void LogInReturn()
        {
            OnLogInPanelReturn?.Invoke(true);
        }
    }
}
