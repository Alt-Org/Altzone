using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view; //modelview script

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
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                _playerData = playerData;
                _view.OnCurrentCharacterIdChanged += HandleCurrentCharacterIdChanged;
                var currentCharacterId = (CharacterID)playerData.SelectedCharacterId;
                var characters = playerData.BattleCharacters.ToList();
                characters.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                // Set characters in the ModelView
                _view.SetCharacters(characters, currentCharacterId);
            });
        }
        private void HandleCurrentCharacterIdChanged(CharacterID newCharacterId)
        {
            if (_view.CurrentCharacterId != (CharacterID)_playerData.SelectedCharacterId)
            {
                _playerData.SelectedCharacterId = (int)_view.CurrentCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }
    }
}
