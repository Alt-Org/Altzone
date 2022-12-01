using Altzone.Scripts.Config;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            _view.Reset();
            _view.PlayerName = playerDataCache.PlayerName;
            _view.ClanName = playerDataCache.Clan.Name;
        }
    }
}