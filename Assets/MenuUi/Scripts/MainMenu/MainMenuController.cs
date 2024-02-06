using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
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
        }

        private void Start()
        {
            var windowManager = WindowManager.Get();
            _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("MainMenuWindow");
            _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
            SetMainMenuLayoutDimensions();
            SetAudioVolumeLevels();
        }

        public void SetAudioVolumeLevels()
        {
            audioSources = FindObjectsOfType<SetVolume>(true);

            carrier.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1);
            carrier.menuVolume = PlayerPrefs.GetFloat("MenuVolume", 1);
            carrier.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            carrier.soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1);

            foreach (SetVolume audioSource in audioSources)
            {
                audioSource.VolumeSet();
            }
        }
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

        private IEnumerator CheckWindowSize()
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
