using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Audio;
using MenuUi.Scripts.SwipeNavigation;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

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

            AudioManager.Instance?.SetCurrentAreaCategoryName("MainMenu");

            if (jukeboxMainMenu)
            {
                if (JukeboxManager.Instance != null && string.IsNullOrEmpty(JukeboxManager.Instance.TryPlayTrack()))
                    AudioManager.Instance?.PlayMusic("MainMenu");
            }
            else
                AudioManager.Instance?.PlayMusic("MainMenu");

            if(!LobbyManager.IsActive) LobbyManager.Instance.Activate();
            if (LobbyManager.Instance.RunnerActive) LobbyManager.CloseRunner();
        }

        private void Start()
        {
            var windowManager = WindowManager.Get();
            _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("MainMenuWindow");
            _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
            SetMainMenuLayoutDimensions();
            AudioManager.Instance.UpdateMaxVolume();
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
            while (true)
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
    }
}
