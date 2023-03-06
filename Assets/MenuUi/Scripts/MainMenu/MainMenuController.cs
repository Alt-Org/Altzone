using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using UnityEngine;

namespace MenuUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            _view.Reset();
            StartCoroutine(WaitForServiceLoader());
        }

        private IEnumerator WaitForServiceLoader()
        {
            // Wait that service loader has loaded everything and player data is accessible.
            var serviceLoader = FindObjectOfType<ServiceLoader>();
            yield return new WaitUntil(() => serviceLoader.IsLoaded);
            LoadClanDataAsync();
        }

        private async void LoadClanDataAsync()
        {
            // Clan data needs to be loaded asynchronously.
            var playerDataModel = GameConfig.Get().PlayerDataModel;
            Debug.Log($"Show player {playerDataModel}");
            var clanId = playerDataModel.ClanId;
            var clan = await Storefront.Get().GetClanModel(clanId);
            ShowViewData(playerDataModel, clan);
        }

        private void ShowViewData(IPlayerDataModel playerDataModel, IClanModel clan)
        {
            _view.PlayerName = playerDataModel.Name;
            _view.ClanName = clan?.Name ?? $"«Clan {playerDataModel.ClanId} not found»";
        }
    }
}