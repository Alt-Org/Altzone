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
                var currentCharacterId = playerData.SelectedCharacterIds;
                var characters = playerData.CustomCharacters.ToList();
                characters.Sort((a, b) => a.Id.CompareTo(b.Id));
                // Set characters in the ModelView
                _view.SetCharacters(characters, currentCharacterId);
            });
        }
        private void HandleCurrentCharacterIdChanged(CharacterID newCharacterId)
        {
            if (newCharacterId != (CharacterID)_playerData.SelectedCharacterIds[0])
            {
                _playerData.SelectedCharacterIds[0] = (int)newCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }
    }
}
