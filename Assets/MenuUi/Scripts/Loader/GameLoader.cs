using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace MenuUi.Scripts.Loader
{
    /// <summary>
    /// Loader to check that game can be started with all required services running.<br />
    /// Waits until game and services has been loaded and then opens the main window.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after services has been loaded";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _mainWindow;
        [SerializeField] private WindowDef _privacyPolicyWindow;
        [SerializeField] private GameObject _introVideo;
        [SerializeField] private WindowDef _introSceneWindow;
        [SerializeField] private int _introScene;

        private bool _videoPlaying = false;
        private bool _videoEnded = false;


        private void Start()
        {
            Debug.Log("start");
            _introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>().loopPointReached += CheckOver;
        }

        private void Update()
        {
            if (PlayerPrefs.GetInt("skipIntroVideo", 0) == 0 && !_videoPlaying && !_videoEnded)
            {
                //_introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>().loopPointReached += CheckOver;
                StartCoroutine(PlayIntroVideo());
            }
            else if(PlayerPrefs.GetInt("skipIntroVideo", 0) == 1 || _videoEnded)
            {
                var windowManager = WindowManager.Get();
                Debug.Log($"show {_mainWindow}");

                var gameConfig = GameConfig.Get();
                var playerSettings = gameConfig.PlayerSettings;
                var playerGuid = playerSettings.PlayerGuid;
                var store = Storefront.Get();
                PlayerData _playerData = null;
                store.GetPlayerData(playerGuid, playerData =>
                {
                    _playerData = playerData;
                });

                if (PlayerPrefs.GetInt("PrivacyPolicy") == 0)
                    windowManager.ShowWindow(_privacyPolicyWindow);
                else if (PlayerPrefs.GetInt("hasSelectedCharacter") == 0 || _playerData == null || int.Parse(_playerData.CurrentCustomCharacterId) < 0)
                    windowManager.ShowWindow(_introSceneWindow);
                else
                    windowManager.ShowWindow(_mainWindow);
                Debug.Log("exit");
            }
        }

        public IEnumerator PlayIntroVideo()
        {
            _introVideo.SetActive(true);
            Debug.Log($"Play video");
            VideoPlayer player = _introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>();
                player.Play();
            _videoPlaying = true;
            yield return true;

        }

        void CheckOver(VideoPlayer vp)
        {
            _videoEnded = true;
            _videoPlaying = false;
            Debug.Log($"End video");
            _introVideo.SetActive(false);
        }

    }
}
