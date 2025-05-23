using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Altzone.Scripts;
using Altzone.Scripts.ReferenceSheets;

using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;

namespace MenuUi.Scripts.Loader
{
    public static partial class SignalBus
    {
        public delegate void VideoEndHandler();
        public static event VideoEndHandler OnVideoEnd;
        public static void OnVideoEndSignal()
        {
            OnVideoEnd?.Invoke();
        }
    }

    /// <summary>
    /// Loader to check that game can be started with all required services running.<br />
    /// Waits until game and services has been loaded and then opens the main window.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after services has been loaded";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _mainWindow;
        [SerializeField] private WindowDef _privacyPolicyWindow;
        [SerializeField] private WindowNavigation _introVideo;
        [SerializeField] private WindowDef _introSceneWindow;
        [SerializeField] private int _introScene;

        private bool _videoPlaying = false;
        private bool _videoEnded = false;

        private void Start()
        {
            Debug.Log("start");
            //_introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>().loopPointReached += CheckOver;
            SignalBus.OnVideoEnd += LoadHandler;
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            SignalBus.OnVideoEnd -= LoadHandler;
        }

        private void Update()
        {
            if (!_videoPlaying && !_videoEnded)
            {
                if (PlayerPrefs.GetInt("skipIntroVideo", 0) == 0)
                {
                    //_introVideo.transform.Find("Video Player").GetComponent<VideoPlayer>().loopPointReached += CheckOver;
                    if (Application.platform is RuntimePlatform.WebGLPlayer)
                        LoadHandler();
                    else
                    {
                        _videoPlaying = true;
                        StartCoroutine(_introVideo.Navigate());
                    }
                }
                else
                {
                    LoadHandler();
                }
            }
        }

        private void LoadHandler()
        {
            _videoEnded = true;
            _videoPlaying = false;
            StartCoroutine(InitializeDataStore());
            //OpenLogIn();
        }

        public IEnumerator InitializeDataStore()
        {
            yield return new WaitUntil(() => Storefront.Get() != null);
            Debug.Log("Datastore initialized");
            yield return new WaitUntil(() => Storefront.Get().SetFurniture(StorageFurnitureReference.Instance.GetAllGameFurniture()));
            OpenLogIn();
        }

        public void OpenLogIn()
        {
            StartCoroutine(GetComponent<WindowNavigation>().Navigate());
            /*var windowManager = WindowManager.Get();
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
            else if (PlayerPrefs.GetInt("hasSelectedCharacter") == 0 || _playerData == null || _playerData.SelectedCharacterIds[0] < 0)
            windowManager.ShowWindow(_introSceneWindow);
            else
            windowManager.ShowWindow(_mainWindow);
            Debug.Log("exit");*/
        }

    }
}
