using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        // content panels for the character slots
        [SerializeField] private Transform VerticalContentPanel;
        [SerializeField] private Transform HorizontalContentPanel;

        [SerializeField] private GameObject _characterSlotprefab;
        [SerializeField] private GalleryCharacterReference _referenceSheet;

        [SerializeField] private bool _isReady;

        // character buttons
        private Button[] _buttons;

        // Array of character slots in horizontalpanel
        public CharacterSlot[] _CurSelectedCharacterSlot { get; private set; }
        // array of character slots in verticalpanel
        private CharacterSlot[] CharacterSlot;

        public delegate void CurrentCharacterIdChangedHandler(CharacterID newCharacterId);
        // Event triggered when current character ID changes
        public event CurrentCharacterIdChangedHandler OnCurrentCharacterIdChanged;
        public bool IsReady => _isReady;

        private CharacterID _currentCharacterId;

        private void Awake()
        {
            _buttons = VerticalContentPanel.GetComponentsInChildren<Button>();
            _CurSelectedCharacterSlot = HorizontalContentPanel.GetComponentsInChildren<CharacterSlot>();
            CharacterSlot = VerticalContentPanel.GetComponentsInChildren<CharacterSlot>();

            LoadAndCachePrefabs();
        }

        private void LoadAndCachePrefabs()
        {
            var gameConfig = GameConfig.Get();
            var playerPrefabs = gameConfig.PlayerPrefabs;
            var prefabs = playerPrefabs._playerPrefabs;
            for (var prefabIndex = 0; prefabIndex < prefabs.Length; ++prefabIndex)
            {
                var playerPrefab = GameConfig.Get().PlayerPrefabs.GetPlayerPrefab(prefabIndex);
                Debug.Log($"prefabIndex {prefabIndex} playerPrefab {playerPrefab.name}");
            }
            _isReady = true;
        }

        public CharacterID CurrentCharacterId
        {
            get => _currentCharacterId;
            private set
            {
                if (_currentCharacterId != value)
                {
                    _currentCharacterId = value;
                    OnCurrentCharacterIdChanged?.Invoke(_currentCharacterId);
                }
            }
        }

        public void Reset()
        {
            // Deactivate all buttons
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
            }
            // Deactivate all character slots
            foreach (var characterSlot in CharacterSlot)
            {
                characterSlot.gameObject.SetActive(false);
            }
        }

        public void SetCharacters(List<BattleCharacter> characters, CharacterID currentCharacterId)
        {
            CurrentCharacterId = currentCharacterId;
            Transform content = transform.Find("Content");
            foreach (var character in characters)
            {
                GameObject slot = Instantiate(_characterSlotprefab, content);

                slot.transform.Find("Button").GetComponent<DraggableCharacter>().Id = character.CustomCharacterId;

                CharacterInfo info = _referenceSheet.GetCharacterPrefabInfo((int)character.CustomCharacterId);


            }

            for (var i = 0; i < _buttons.Length && i < CharacterSlot.Length; ++i)
            {
                var button = _buttons[i];
                var characterSlot = CharacterSlot[i];

                if (i < characters.Count)
                {
                    var character = characters[i];
                    button.gameObject.SetActive(true);
                    button.interactable = true;
                    button.SetCaption(character.Name); // Set button caption to character name

                    characterSlot.gameObject.SetActive(true);

                    // Check if the character is currently selected
                    if (currentCharacterId == character.CustomCharacterId)
                    {
                        // Set the character in the first slot of the horizontal character slot
                        if (_CurSelectedCharacterSlot.Length > 0)
                        {
                            button.transform.SetParent(_CurSelectedCharacterSlot[0].transform, false);
                        }
                    }

                    // Subscribe to the event of parent change for the button
                    var parentChangeMonitor = button.GetComponent<DraggableCharacter>();
                    parentChangeMonitor.OnParentChanged += newParent =>
                    {
                        // Check if the character is in the first slot of the horizontal character slot
                        if (newParent == _CurSelectedCharacterSlot[0].transform)
                        {
                            // Set the id
                            CurrentCharacterId = character.CustomCharacterId;
                        }
                    };
                }
                else
                {
                    // Hide unused buttons and slots
                    button.gameObject.SetActive(false);
                    characterSlot.gameObject.SetActive(false);
                }
            }
        }
    }
}

