using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;


namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view; //modelview script

        private PlayerData _playerData;


        private void Awake()
        {
            ServerManager.OnLogInStatusChanged += StartLoading;
        }


        // When starting from 01-Loader OnLogInStatusChanged doesn't get called, so checking here in start function if player is already logged in to start loading.
        // This is done in start because ModelView wasn't loaded yet during the Awake function. 
        private void Start() 
        {
            ServerManager manager = ServerManager.Instance;
            if (manager.isLoggedIn)
            {
                StartLoading(manager.isLoggedIn);
            }
        }


        private void OnDestroy()
        {
            ServerManager.OnLogInStatusChanged -= StartLoading;
        }


        private void StartLoading(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                StartCoroutine(Load());
            }
        }


        private IEnumerator Load()
        {
            Debug.Log("Start");
            _view.Reset();
            yield return new WaitUntil(() => _view.IsReady);
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                _playerData = playerData;
                _view.OnCurrentCharacterIdChanged -= HandleCurrentCharacterIdChanged;
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
