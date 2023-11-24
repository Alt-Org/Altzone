using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;
        public float _interval = 2f;

        private GameObject[] _layoutElementsGameObjects;
        private RectTransform _scrollRectCanvas;

        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }

        private void OnEnable()
        {
            _view.ResetView();
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                if (playerData == null)
                {
                    _view.PlayerName = "Player?";
                    return;
                }
                _view.PlayerName = playerData.Name;
                if (!playerData.HasClanId)
                {
                    return;
                }
                store.GetClanData(playerData.ClanId, clanData =>
                {
                    _view.ClanName = clanData?.Name ?? "Clan?";
                });
            });

            StartCoroutine(CheckWindowSize());
        }

        private void Start()
        {
            var windowManager = WindowManager.Get();
            _layoutElementsGameObjects = GameObject.FindGameObjectsWithTag("MainMenuWindow");
            _scrollRectCanvas = GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>();
            SetMainMenuLayoutDimensions();
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
        }

        private IEnumerator CheckWindowSize()
        {
            while (true)
            {
                if (lastWidth != Screen.width || lastHeight != Screen.height)
                {
                    SetMainMenuLayoutDimensions();
                    GetComponentInParent<SwipeUI>().UpdateSwipe();
                    lastWidth = Screen.width;
                    lastHeight = Screen.height;
                }

                yield return new WaitForSeconds(_interval);
            }
        }
    }
}
