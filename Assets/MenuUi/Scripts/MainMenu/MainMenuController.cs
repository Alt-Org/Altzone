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
        [Tooltip("Interval to check window size")]
        public float _interval = 2f;

        private SwipeUI _swipe;

        private GameObject[] _layoutElementsGameObjects;
        private RectTransform _scrollRectCanvas;

        private int lastWidth;
        private int lastHeight;

        private SetVolume[] audioSources;
        private SettingsCarrier carrier = SettingsCarrier.Instance;

        [Tooltip("The buttons that should be disabled when a task is active on TurboEducation")]
        [SerializeField]
        private Button[] _buttonsToDisableOnTurboEducationTask;

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
            OverlayPanelCheck.Instance?.ToggleOverlay(true);

            try
            {
                if (jukeboxMainMenu)
                {
                    if (JukeboxManager.Instance != null && string.IsNullOrEmpty(JukeboxManager.Instance.TryPlayTrack()))
                        AudioManager.Instance?.PlayMusic(AudioCategoryType.MainMenu);
                }
                else
                    AudioManager.Instance?.PlayMusic(AudioCategoryType.MainMenu);
            }
            catch (Exception e) { Debug.LogException(e); }

            if(!LobbyManager.IsActive) LobbyManager.Instance.Activate();
            if (LobbyManager.Instance.RunnerActive) LobbyManager.CloseRunner();

            StartCoroutine(EnableChooseTask());

            UpdateTurboEdButtonsState();
        }

        private void Start()
        {

            ChooseTask.OnChooseTaskShown += DisableTurboEdButtons;
            DailyTaskProgressManager.OnTaskDone += UpdateTurboEdButtonsState;

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
            ChooseTask.OnChooseTaskShown -= DisableTurboEdButtons;
            DailyTaskProgressManager.OnTaskDone -= UpdateTurboEdButtonsState;

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
        private IEnumerator CheckWindowSize()
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

        /// <summary>
        /// This is to enable/disable specified buttons on TurboEducation when a task is active
        /// </summary>
        private void UpdateTurboEdButtonsState()
        {
            if (GameConfig.Get().GameVersionType == VersionType.TurboEducation)
            {
                SetTurboEdButtonsState(!DailyTaskProgressManager.Instance.HasOnGoingTask());
            }
        }

        public void SetTurboEdButtonsState(bool active)
        {
            foreach (Button button in _buttonsToDisableOnTurboEducationTask)
            {
                button.interactable = active;
            }
        }

        private void DisableTurboEdButtons()
        {
            SetTurboEdButtonsState(false);
        }
    }
}
