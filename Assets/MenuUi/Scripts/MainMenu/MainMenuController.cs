using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private async void OnEnable()
        {
            var playerDataCache = GameConfig.Get().PlayerSettings;
            _view.Reset();
            _view.PlayerName = playerDataCache.PlayerName;
            var clanId = playerDataCache.ClanId;
            _view.ClanName = string.Empty;
            var clan = await Storefront.Get().GetClanModel(clanId);
            _view.ClanName = clan?.Name ?? $"«Clan {clanId} not found»";
        }
    }
}