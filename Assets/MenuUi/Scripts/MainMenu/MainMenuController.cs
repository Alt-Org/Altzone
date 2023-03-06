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
            var playerData = GameConfig.Get().PlayerDataModel;
            _view.Reset();
            _view.PlayerName = playerData.Name;
            var clanId = playerData.ClanId;
            _view.ClanName = string.Empty;
            var clan = await Storefront.Get().GetClanModel(clanId);
            _view.ClanName = clan?.Name ?? $"«Clan {clanId} not found»";
        }
    }
}