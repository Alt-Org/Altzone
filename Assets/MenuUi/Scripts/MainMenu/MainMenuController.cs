using System;
using System.Collections;
using Altzone.Scripts.Audio;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        public float _interval = 2f;

        private SwipeUI _swipe;

        private GameObject[] _layoutElementsGameObjects;
        private RectTransform _scrollRectCanvas;

        private int lastWidth;
        private int lastHeight;

        private SetVolume[] audioSources;
        private SettingsCarrier carrier = SettingsCarrier.Instance;

        [Tooltip("The button that allows the player queue for a match")]
        [SerializeField] Button _playButton;

        private void Awake()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }

        private void OnEnable()
        {
            bool jukeboxMainMenu = carrier.CanPlayJukeboxInArea(SettingsCarrier.JukeboxPlayArea.MainMenu);

            _swipe = GetComponentInParent<SwipeUI>();
            StartCoroutine(CheckWindowSize());

            OverlayPanelCheck.Instance?.gameObject.SetActive(true);
            AudioManager.Instance?.SetCurrentAreaCategoryName("MainMenu");

            try
            {
                if (jukeboxMainMenu)
                {
                    if (JukeboxManager.Instance != null && string.IsNullOrEmpty(JukeboxManager.Instance.TryPlayTrack()))
                        AudioManager.Instance?.PlayMusic("MainMenu");
                }
                else
                    AudioManager.Instance?.PlayMusic("MainMenu");
            }
            catch (Exception e) { Debug.LogException(e); }

            if(!LobbyManager.IsActive) LobbyManager.Instance.Activate();
            if (LobbyManager.Instance.RunnerActive) LobbyManager.CloseRunner();

            StartCoroutine(EnableChooseTask());

            UpdatePlayButton();
        }

        private void Start()
        {

            ChooseTask.OnChooseTaskShown += DisablePlayButton;
            DailyTaskProgressManager.OnTaskDone += UpdatePlayButton;

            var windowManager = WindowManager.Get();
            if (_swipe)
            {
                _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("MainMenuWindow");
                _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
                SetMainMenuLayoutDimensions();
            }
            AudioManager.Instance.UpdateMaxVolume();
        }

        private void OnDestroy()
        {
            ChooseTask.OnChooseTaskShown -= DisablePlayButton;
            DailyTaskProgressManager.OnTaskDone -= UpdatePlayButton;

        }
        /// <summary>
        /// Sets the correct windows size to swipeable main menu windows.
        /// </summary>
        /// <remarks>
        /// Each individual window should be exactly the size of the device screen.
        /// Since the main menu windows are inside of a scroll view component we cannot just use anchors
        /// to fit the windows to screen.
        /// </remarks>
        private void SetMainMenuLayoutDimensions()
        {
            Debug.Log("Setting dimensions");

            LayoutElement[] layoutElements = new LayoutElement[_layoutElementsGameObjects.Length];

            for (int i = 0; i < _layoutElementsGameObjects.Length; i++)
                layoutElements[i] = _layoutElementsGameObjects[i].GetComponent<LayoutElement>();

            float width = _scrollRectCanvas.sizeDelta.x;
            float height = _scrollRectCanvas.sizeDelta.y;

            foreach (LayoutElement element in layoutElements)
            {
                element.preferredWidth = width;
                element.preferredHeight = height;
            }

            _swipe.UpdateSwipeAreaValues();
        }


        /// <summary>
        /// Checks periodically if device screen dimensions have changed and sets the windows sizes accordingly.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This might only be necessary on PC and Unity Editor
        /// </remarks>
        private IEnumerator CheckWindowSize() //Tällä saa ikkunan koon.
        {
            while (_swipe)
            {
                if (lastWidth != Screen.width || lastHeight != Screen.height)
                {
                    SetMainMenuLayoutDimensions();
                    _swipe.UpdateSwipe();
                    lastWidth = Screen.width;
                    lastHeight = Screen.height;
                }

                yield return new WaitForSeconds(_interval);
            }
        }


        private IEnumerator EnableChooseTask()
        {
            yield return new WaitUntil(() => GameObject.FindObjectOfType<ChooseTask>() != null);

            // Initialize ChooseTask.cs
            GameObject.FindObjectOfType<ChooseTask>().InitializeChooseTask();
        }

        private void UpdatePlayButton()
        {
            if (GameConfig.Get().GameVersionType == VersionType.TurboEducation)
            {
                SetPlayButtonState(!DailyTaskProgressManager.Instance.HasOnGoingTask());
            }
        }

        public void SetPlayButtonState(bool active)
        {
            _playButton.interactable = active;
        }

        private void DisablePlayButton()
        {
            SetPlayButtonState(false);
        }
    }
}
