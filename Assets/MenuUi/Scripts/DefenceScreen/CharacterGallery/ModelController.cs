using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

//TODO: muokkaa HandleCurrentCharacterIdChanged metodia ottamaan parametrinä sisään sen paikan id johon
// hahmo juuri laitettiin ja sitten sen perusteella tarkistaa ja tallentaa tieto.
// Myöskin pitäisi olla mahdollista poistaa valittu hahmo listasta
// niin kauan kunhan ainakin yksi hahmo on vielä listassa.

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
        private void HandleCurrentCharacterIdChanged(CharacterID newCharacterId, int slot)
        {
            if (slot < 0 || slot >= 3) return;
            if (newCharacterId != (CharacterID)_playerData.SelectedCharacterIds[slot])
            {
                _playerData.SelectedCharacterIds[slot] = (int)newCharacterId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }
    }
}
