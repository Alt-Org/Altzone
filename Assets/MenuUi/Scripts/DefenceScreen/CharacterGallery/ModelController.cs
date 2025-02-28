using System.Collections;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using MenuUi.Scripts.Signals;


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

        public delegate void DefenceGalleryEditModeRequested();
        public static event DefenceGalleryEditModeRequested OnDefenceGalleryEditModeRequested;
        public static void OnDefenceGalleryEditModeRequestedSignal()
        {
            OnDefenceGalleryEditModeRequested?.Invoke();
        }

        public delegate void ReloadCharacterGalleryRequested();
        public static event ReloadCharacterGalleryRequested OnReloadCharacterGalleryRequested;
        public static void OnReloadCharacterGalleryRequestedSignal()
        {
            OnReloadCharacterGalleryRequested?.Invoke();
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
        [SerializeField] private ModelView _view; //modelview script

        private PlayerData _playerData;
        private bool _reloadRequested = false;

        private void Awake()
        {
            ServerManager.OnLogInStatusChanged += StartLoading;
            SignalBus.OnRandomSelectedCharactersRequested += SetRandomSelectedCharactersToEmptySlots;
            _view.OnTopSlotCharacterSet += HandleCharacterSelected;
            SignalBus.OnReloadCharacterGalleryRequested += OnReloadRequested;
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
                StartCoroutine(Load());
                _reloadRequested = false;
            }
        }


        private void OnDestroy()
        {
            ServerManager.OnLogInStatusChanged -= StartLoading;
            SignalBus.OnRandomSelectedCharactersRequested -= SetRandomSelectedCharactersToEmptySlots;
            _view.OnTopSlotCharacterSet -= HandleCharacterSelected;
            SignalBus.OnReloadCharacterGalleryRequested -= OnReloadRequested;
        }


        private void StartLoading(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                StartCoroutine(Load());
            }
        }


        private void OnReloadRequested()
        {
            _reloadRequested = true;
        }


        private IEnumerator Load()
        {
            _view.Reset();
            yield return new WaitUntil(() => _view.IsReady);

            StartCoroutine(GetPlayerData(playerData =>
            {
                _playerData = playerData;
                var selectedCharacterIds = playerData.SelectedCharacterIds;
                int[] characterIds = new int[3];
                for (int i = 0; i < selectedCharacterIds.Length; i++)
                {
                    characterIds[i] = _playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == selectedCharacterIds[i]) == null ? 0 : (int)_playerData.CustomCharacters.FirstOrDefault(x => x.ServerID == selectedCharacterIds[i]).Id;
                }
                var characters = playerData.CustomCharacters.ToList();
                characters.Sort((a, b) => a.Id.CompareTo(b.Id));
                // Set characters in the ModelView
                _view.SetCharacters(characters, characterIds);
            }));
        }


        private void HandleCharacterSelected(CharacterID newCharacterId, int slot)
        {
            if (slot < 0 || slot >= 3) return;

            string newServerId = _playerData.CustomCharacters.FirstOrDefault(x => x.Id == newCharacterId)?.ServerID;
            if (newServerId == null)
            {
                _playerData.SelectedCharacterIds[slot] = "0";
            }

            if (newServerId != _playerData.SelectedCharacterIds[slot])
            {
                _playerData.SelectedCharacterIds[slot] = newServerId;
            }

            var store = Storefront.Get();
            store.SavePlayerData(_playerData, null);
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
