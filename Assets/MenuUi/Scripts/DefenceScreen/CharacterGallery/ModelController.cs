using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using SignalBus = MenuUi.Scripts.Lobby.SignalBus;


namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelController : MonoBehaviour
    {
        [SerializeField] private ModelView _view; //modelview script

        private PlayerData _playerData;


        private void Awake()
        {
            ServerManager.OnLogInStatusChanged += StartLoading;
            SignalBus.OnRandomSelectedCharactersRequested += SetRandomSelectedCharactersToEmptySlots;
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
            SignalBus.OnRandomSelectedCharactersRequested -= SetRandomSelectedCharactersToEmptySlots;
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
                Debug.LogWarning(_playerData.CustomCharacters.Count+" : "+ _playerData.CustomCharacters[0].ServerID);
                var currentCharacterId = playerData.SelectedCharacterIds;
                int[] characterId = new int[3];
                for (int i = 0; i < currentCharacterId.Length; i++)
                {
                    characterId[i] = _playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == currentCharacterId[i])== null ? 0 : (int)_playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == currentCharacterId[i]).Id;
                }
                var characters = playerData.CustomCharacters.ToList();
                characters.Sort((a, b) => a.Id.CompareTo(b.Id));
                // Set characters in the ModelView
                _view.SetCharacters(characters, characterId);
            });
        }


        private void HandleCurrentCharacterIdChanged(CharacterID newCharacterId, int slot)
        {
            if (slot < 0 || slot >= 3) return;

            string newServerId = _playerData.CustomCharacters.FirstOrDefault(x => x.Id == newCharacterId)?.ServerID;
            if(newServerId == null) return;

            if (newServerId != _playerData.SelectedCharacterIds[slot])
            {
                _playerData.SelectedCharacterIds[slot] = newServerId;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
        }


        /// <summary>
        /// Set random characters to selected character slots which are empty. Reloads character gallery afterwards.
        /// </summary>
        public void SetRandomSelectedCharactersToEmptySlots()
        {
            var characters = _playerData.CustomCharacters.ToList();
            characters.Sort((a, b) => a.Id.CompareTo(b.Id));

            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(_playerData.SelectedCharacterIds[i]) || _playerData.SelectedCharacterIds[i] == "0")
                {
                    bool suitableCharacterFound = false;
                    CustomCharacter character = null;
                    do
                    {
                        character = characters[UnityEngine.Random.Range(0, characters.Count)];
                        if (character.ServerID != _playerData.SelectedCharacterIds[0] && character.ServerID != _playerData.SelectedCharacterIds[1] && character.ServerID != _playerData.SelectedCharacterIds[2])
                        {
                            suitableCharacterFound = true;
                        }

                    } while (!suitableCharacterFound);

                    _playerData.SelectedCharacterIds[i] = character.ServerID;
                }
            }
            var store = Storefront.Get();
            store.SavePlayerData(_playerData, null);
            StartCoroutine(Load());
        }
    }
}
