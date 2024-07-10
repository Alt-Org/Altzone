using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Prg.Scripts.Common;

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
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            if ((PlayerPrefs.GetInt("skipIntroVideo", 0) == 0 && !_videoPlaying && !_videoEnded))
            {
                //_introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>().loopPointReached += CheckOver;
                if(Application.platform is RuntimePlatform.WebGLPlayer)
                    _videoEnded = true;
                else
                PlayIntroVideo();
            }

            if (_videoPlaying)
            {
                if (ClickStateHandler.GetClickState() is ClickState.End)
                    EndIntroVideo();
                
            }

            if(PlayerPrefs.GetInt("skipIntroVideo", 0) == 1 || _videoEnded)
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
                else if (PlayerPrefs.GetInt("hasSelectedCharacter") == 0 || _playerData == null || _playerData.SelectedCharacterId < 0)
                    windowManager.ShowWindow(_introSceneWindow);
                else
                    windowManager.ShowWindow(_mainWindow);
                Debug.Log("exit");
            }
        }

        public void PlayIntroVideo()
        {
            _introVideo.SetActive(true);
            Debug.Log($"Play video");
            VideoPlayer player = _introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>();
                player.Play();
            _videoPlaying = true;

        }

        public void EndIntroVideo()
        {
            if (_videoPlaying)
            {
                VideoPlayer player = _introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>();
                player.Stop();
                _videoEnded = true;
                _videoPlaying = false;
                Debug.Log($"Skip video");
                _introVideo.SetActive(false);
            }
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
