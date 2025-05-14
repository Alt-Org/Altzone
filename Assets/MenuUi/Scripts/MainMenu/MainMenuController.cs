using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Audio;
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
            _swipe = GetComponentInParent<SwipeUI>();
            StartCoroutine(CheckWindowSize());
            AudioManager.Instance?.PlayMusic(MusicSection.MainMenu);
            LobbyManager.Instance.Activate();
        }

        private void Start()
        {
            var windowManager = WindowManager.Get();
            _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("MainMenuWindow");
            _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
            SetMainMenuLayoutDimensions();
            SetAudioVolumeLevels();
            AudioManager.Instance?.PlayMusic(MusicSection.MainMenu);
        }

        /// <summary>
        /// Sets the audio levels of main menu audio sources according to the values in PlayerPrefs
        /// </summary>
        public void SetAudioVolumeLevels()
        {
            audioSources = FindObjectsOfType<SetVolume>(true);

            //carrier.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1);
            //carrier.menuVolume = PlayerPrefs.GetFloat("MenuVolume", 1);
            //carrier.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            //carrier.soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1);

            foreach (SetVolume audioSource in audioSources)
            {
                audioSource.VolumeSet();
            }
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
