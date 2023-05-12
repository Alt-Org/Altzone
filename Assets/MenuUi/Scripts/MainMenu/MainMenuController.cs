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
    }
}