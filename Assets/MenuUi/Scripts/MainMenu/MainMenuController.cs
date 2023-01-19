using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            var playerDataCache = GameConfig.Get().PlayerSettings;
            _view.Reset();
            _view.PlayerName = playerDataCache.PlayerName;
            var clan = Storefront.Get().GetClanModel(playerDataCache.ClanId);
            if (clan != null)
            {
                _view.ClanName = clan.Name;
            }
        }
    }
}