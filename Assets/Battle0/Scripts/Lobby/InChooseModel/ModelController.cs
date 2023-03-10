using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle0.Scripts.Lobby.InChooseModel
{
    /// <summary>
    /// UI controller for <c>CharacterModel</c> view.
    /// </summary>
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view;

        private PlayerData _playerData;

        private IEnumerator Start()
        {
            Debug.Log("Start");
            yield return new WaitUntil(() => _view.IsReady);
            _view.Reset();
            _view.Title = $"Choose your character\r\nfor {Application.productName} {PhotonLobby.GameVersion}";
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                _playerData = playerData;
                _view.PlayerName = playerData.Name;
                _view.ContinueButtonOnClick = ContinueButtonOnClick;
                var currentCharacterId = playerData.CurrentCustomCharacterId;
                Storefront.Get().GetAllBattleCharacters(characters =>
                {
                    characters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    _view.SetCharacters(characters, currentCharacterId);
                });
            });
        }

        private void ContinueButtonOnClick()
        {
            Debug.Log("click");
            if (_view.CurrentCharacterId != _playerData.CurrentCustomCharacterId)
            {
                _playerData.CurrentCustomCharacterId = _view.CurrentCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }
    }
}