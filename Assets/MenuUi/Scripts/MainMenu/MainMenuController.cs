using Altzone.Scripts;
using Altzone.Scripts.Config;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            _view.Reset();
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                _view.PlayerName = playerData?.Name ?? "Player?";
                _view.ClanName = "Clan?";
            });
        }
    }
}