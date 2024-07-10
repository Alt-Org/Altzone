using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
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

        private void OnEnable()
        {
            StartCoroutine(Load());
        }

        private IEnumerator Load()
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
                var currentCharacterId = (CharacterID)playerData.SelectedCharacterId;
                var characters = playerData.BattleCharacters.ToList();
                characters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                _view.SetCharacters(characters, currentCharacterId);
            });
        }

        private void ContinueButtonOnClick()
        {
            Debug.Log("click");
            if ((int)_view.CurrentCharacterId != _playerData.SelectedCharacterId)
            {
                _playerData.SelectedCharacterId = (int)_view.CurrentCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }
    }
}
