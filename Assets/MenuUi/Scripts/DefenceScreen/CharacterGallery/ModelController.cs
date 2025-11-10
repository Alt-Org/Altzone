using System;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using MenuUi.Scripts.Signals;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MenuUi.Scripts.Signals
{
    public static partial class SignalBus
    {
        public delegate void RandomSelectedCharactersRequested();
        public static event RandomSelectedCharactersRequested OnRandomSelectedCharactersRequested;
        public static void OnRandomSelectedCharactersRequestedSignal()
        {
            OnRandomSelectedCharactersRequested?.Invoke();
        }

        public delegate void DefenceGalleryEditPanelRequested();
        public static event DefenceGalleryEditPanelRequested OnDefenceGalleryEditPanelRequested;
        public static void OnDefenceGalleryEditPanelRequestedSignal()
        {
            OnDefenceGalleryEditPanelRequested?.Invoke();
        }

        public delegate void DefenceGalleryStatPopupRequested(CharacterID characterId);
        public static event DefenceGalleryStatPopupRequested OnDefenceGalleryStatPopupRequested;
        public static void OnDefenceGalleryStatPopupRequestedSignal(CharacterID characterID)
        {
            OnDefenceGalleryStatPopupRequested?.Invoke(characterID);
        }

        public delegate void ReloadCharacterGalleryRequested();
        public static event ReloadCharacterGalleryRequested OnReloadCharacterGalleryRequested;
        public static void OnReloadCharacterGalleryRequestedSignal()
        {
            OnReloadCharacterGalleryRequested?.Invoke();
        }

        public delegate void SelectedDefenceCharacterChanged(CharacterID characterID, int slot);
        public static event SelectedDefenceCharacterChanged OnSelectedDefenceCharacterChanged;
        public static void OnSelectedDefenceCharacterChangedSignal(CharacterID characterID, int slot)
        {
            OnSelectedDefenceCharacterChanged?.Invoke(characterID, slot);
        }
    }
}


namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Controls the character gallery "model". Has methods for loading gallery characters and controlling the selected characters such as initiating the saving or selecting random characters.
    /// </summary>
    public class ModelController : AltMonoBehaviour
    {
        [SerializeField] private GalleryView _view;
        [SerializeField] private GalleryView _editingPanelView;
        [SerializeField] private StatsWindowController _statsWindowController;

        private PlayerData _playerData;
        private bool _reloadRequested = false;

        public event Action<PlayerData> OnPlayerDataReady;

        public PlayerData Player => _playerData;

        private void Awake()
        {
            ServerManager.OnLogInStatusChanged += StartLoading;
            SettingsCarrier.OnLanguageChanged += LanguageChanged;
            SignalBus.OnRandomSelectedCharactersRequested += SetRandomSelectedCharactersToEmptySlots;
            SignalBus.OnReloadCharacterGalleryRequested += OnReloadRequested;
            SignalBus.OnSelectedDefenceCharacterChanged += HandleCharacterSelected;
            SignalBus.OnDefenceGalleryStatPopupRequested += _statsWindowController.OpenPopup;
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


        private void OnEnable()
        {
            if (_reloadRequested)
            {
                Load();
                _reloadRequested = false;
            }
        }


        private void OnDestroy()
        {
            ServerManager.OnLogInStatusChanged -= StartLoading;
            SettingsCarrier.OnLanguageChanged -= LanguageChanged;
            SignalBus.OnRandomSelectedCharactersRequested -= SetRandomSelectedCharactersToEmptySlots;
            SignalBus.OnReloadCharacterGalleryRequested -= OnReloadRequested;
            SignalBus.OnSelectedDefenceCharacterChanged -= HandleCharacterSelected;
            SignalBus.OnDefenceGalleryStatPopupRequested -= _statsWindowController.OpenPopup;
        }


        private void StartLoading(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                Load();
            }
        }

        private void LanguageChanged(SettingsCarrier.LanguageType language) => OnReloadRequested();

        private void OnReloadRequested()
        {
            if (gameObject.activeInHierarchy)
            {
                Load();
            }
            else
            {
                _reloadRequested = true;
            }
        }


        private void Load()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                _playerData = playerData;

                // Getting selected character ids as CharacterID array
                CustomCharacterListObject[] selectedCharacterIds = playerData.SelectedCharacterIds;

                // Getting custom characters, ensuring no duplicate characters if account is bugged
                var characters = playerData.CustomCharacters.GroupBy(x => x.Id).Select(x => x.First()).ToList();

                // Sorting custom characters
                characters.Sort((a, b) => a.Id.CompareTo(b.Id));

                // Set characters in the ModelView
                _view.SetCharacters(characters, selectedCharacterIds);
                _editingPanelView.SetCharacters(characters, selectedCharacterIds);

                OnPlayerDataReady?.Invoke(_playerData);
            }));
        }

        private CharacterID[] GetCharacterIDs(string[] serverIDs)
        {
            CharacterID[] characterIds = new CharacterID[serverIDs.Length];
            for (int i = 0; i < serverIDs.Length; i++)
            {
                CustomCharacter character = _playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == serverIDs[i]);
                if (serverIDs[i] == ((int)CharacterID.None).ToString() || character == null)
                {
                    characterIds[i] = CharacterID.None;
                }
                else
                {
                    characterIds[i] = character.Id;
                }
            }

            return characterIds;
        }

        private void HandleCharacterSelected(CharacterID newCharacterId, int slot)
        {
            if (slot < 0 || slot >= 3) return;

            string newServerId = _playerData.CustomCharacters.FirstOrDefault(x => x.Id == newCharacterId)?.ServerID;
            if (newServerId == null || newServerId == ((int)CharacterID.None).ToString())
            {
                if (_playerData.SelectedCharacterIds.Length > slot)
                if (CustomCharacter.IsTestCharacter(newCharacterId))
                {
                        _playerData.SelectedCharacterIds[slot].SetData(Id: newCharacterId);
                }
                else
                {
                    _playerData.SelectedCharacterIds[slot].SetData();
                }
            }
            else if (newServerId != _playerData.SelectedCharacterIds[slot].ServerID)
            {
                _playerData.SelectedCharacterIds[slot].SetData(newServerId, newCharacterId);
            }

            string[] serverList = new string[_playerData.SelectedCharacterIds.Length];

            for(int i = 0; i < _playerData.SelectedCharacterIds.Length; i++)
            {
                serverList[i] = _playerData.SelectedCharacterIds[i].ServerID;
            }

            string body = JObject.FromObject(
            new
            {
                _id = _playerData.Id,
                battleCharacter_ids = serverList

            }).ToString();

            StartCoroutine(ServerManager.Instance.UpdatePlayerToServer(body, callback =>
            {
                if (callback != null)
                {
                    Debug.Log("Profile info updated.");
                }
                else
                {
                    Debug.Log("Profile info update failed.");
                }

                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }));

            //_playerData.OnCurrentTeamChanged_AutoSave();
        }


        /// <summary>
        /// Set random characters to selected character slots which are empty. Reloads character gallery afterwards.
        /// </summary>
        public void SetRandomSelectedCharactersToEmptySlots()
        {
            var characters = _playerData.CustomCharacters.ToList();
            characters.Sort((a, b) => a.Id.CompareTo(b.Id));

            if (characters.Count < _playerData.SelectedCharacterIds.Length) return;

            for (int i = 0; i < _playerData.SelectedCharacterIds.Length; i++)
            {
                if (_playerData.SelectedCharacterIds[i].CharacterID == CharacterID.None)
                {
                    bool suitableCharacterFound = false;
                    CustomCharacter character = null;
                    do
                    {
                        character = characters[UnityEngine.Random.Range(0, characters.Count)];
                        if (_playerData.SelectedCharacterIds.FirstOrDefault(c=>c.ServerID == character.ServerID) == null)
                        {
                            suitableCharacterFound = true;
                        }

                    } while (!suitableCharacterFound);

                    _playerData.SelectedCharacterIds[i].SetData(character.ServerID, character.Id);
                }
            }
            var store = Storefront.Get();
            store.SavePlayerData(_playerData, null);
            Load();
        }
    }
}
