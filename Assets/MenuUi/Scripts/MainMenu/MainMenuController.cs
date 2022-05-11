using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void Awake()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerName = playerDataCache.PlayerName;
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GeoLocation.HasData);
            Debug.Log($"GeoLocation {GeoLocation.Data}");
        }
    }
}