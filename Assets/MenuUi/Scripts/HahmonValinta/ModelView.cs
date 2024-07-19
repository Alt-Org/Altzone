using System;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform VerticalContentPanel;
        [SerializeField] private Transform HorizontalContentPanel;

        [SerializeField] private GameObject _characterSlotprefab;
        [SerializeField] private GalleryCharacterReference _referenceSheet;

        [SerializeField] private bool _isReady;

        private List<Button> _buttons = new();
        public CharacterSlot[] _CurSelectedCharacterSlot { get; private set; }
        private List<CharacterSlot> _characterSlot = new();

        public delegate void CurrentCharacterIdChangedHandler(CharacterID newCharacterId);
        public event CurrentCharacterIdChangedHandler OnCurrentCharacterIdChanged;
        public bool IsReady => _isReady;

        private CharacterID _currentCharacterId;

        private void Awake()
        {
            _CurSelectedCharacterSlot = HorizontalContentPanel.GetComponentsInChildren<CharacterSlot>();
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
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
            }
            foreach (var characterSlot in _characterSlot)
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

                slot.GetComponent<CharacterSlot>().SetInfo(info.Image, info.Name, this);

                _characterSlot.Add(slot.GetComponent<CharacterSlot>());
                _buttons.Add(slot.transform.Find("Button").GetComponent<Button>());
            }

            for (var i = 0; i < _buttons.Count && i < _characterSlot.Count; ++i)
            {
                var button = _buttons[i];
                var characterSlot = _characterSlot[i];

                if (i < characters.Count)
                {
                    var character = characters[i];
                    button.gameObject.SetActive(true);
                    button.interactable = true;

                    characterSlot.gameObject.SetActive(true);


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


