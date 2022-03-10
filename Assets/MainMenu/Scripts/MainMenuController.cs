using Altzone.Scripts.Config;
using UnityEngine;

namespace MainMenu.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void Awake()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerName = playerDataCache.PlayerName;
        }
    }
}