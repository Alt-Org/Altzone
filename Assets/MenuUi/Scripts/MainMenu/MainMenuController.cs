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
        }

        private void Start()
        {
            var windowManager = WindowManager.Get();
            SetMainMenuLayoutDimensions(GameObject.FindGameObjectsWithTag("MainMenuWindow"), GameObject.FindGameObjectWithTag("ScrollRectCanvas").GetComponent<RectTransform>());
        }

        private void SetMainMenuLayoutDimensions(GameObject[] layoutElementsGameObjects, RectTransform scrollRectCanvas)
        {
            LayoutElement[] layoutElements = new LayoutElement[layoutElementsGameObjects.Length];

            for (int i = 0; i < layoutElementsGameObjects.Length; i++)
                layoutElements[i] = layoutElementsGameObjects[i].GetComponent<LayoutElement>();

            float width = scrollRectCanvas.sizeDelta.x;
            float height = scrollRectCanvas.sizeDelta.y;

            foreach (LayoutElement element in layoutElements)
            {
                element.preferredWidth = width;
                element.preferredHeight = height;
            }
        }
    }
}
