using System;
using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform VerticalContentPanel;
        [SerializeField] private Transform HorizontalContentPanel;

        [SerializeField] private GameObject _characterSlotprefab;
        [SerializeField] private GalleryCharacterReference _referenceSheet;

        [SerializeField] private bool _isReady;

        // character buttons
        private List<Button> _buttons = new();

        // Array of character slots in horizontalpanel
        public CharacterSlot[] _CurSelectedCharacterSlot { get; private set; }
        // array of character slots in verticalpanel
        private List<CharacterSlot> _characterSlot = new();

        public delegate void CurrentCharacterIdChangedHandler(CharacterID newCharacterId);
        public event CurrentCharacterIdChangedHandler OnCurrentCharacterIdChanged;
        public bool IsReady => _isReady;
        public int characterTextCounter;

        private CharacterID _currentCharacterId;

        public ColorBlock _colorBlock = new();


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
                if (!button.transform.IsChildOf(HorizontalContentPanel))
                    Destroy(button.gameObject);
            }
            // remove all character slots
            foreach (var characterSlot in _characterSlot)
            {
                if (!characterSlot.transform.IsChildOf(HorizontalContentPanel))
                    Destroy(characterSlot.gameObject);
            }
            _buttons.Clear();
            _characterSlot.Clear();
        }
        public Color GetCharacterClassColor(CharacterClassID id)
        {
            switch (id)
            {
                case CharacterClassID.Desensitizer:
                    return new Color(0.68f, 0.84f, 0.9f, 1);
                case CharacterClassID.Trickster:
                    return Color.green;
                case CharacterClassID.Obedient:
                    return new Color(1f, 0.64f, 0, 1);
                case CharacterClassID.Projector:
                    return Color.yellow;
                case CharacterClassID.Retroflector:
                    return Color.red;
                case CharacterClassID.Confluent:
                    return new Color(0.5f, 0, 0.5f, 1);
                case CharacterClassID.Intellectualizer:
                    return Color.blue;
                default:
                    return Color.gray;
            }
        }

        public Transform GetContent()
        {
            Transform content = (VerticalContentPanel == null) ? transform.Find("Content") :
                VerticalContentPanel.transform;

            return content;
        }

        public void SetCharacters(List<CustomCharacter> characters, int[] currentCharacterId)
        {
            var store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (var character in allItems)
            {
                GalleryCharacterInfo info = _referenceSheet.GetCharacterPrefabInfoFast((int)character.Id);
                if (info == null) continue;

                GameObject slot = Instantiate(_characterSlotprefab, GetContent());
                slot.GetComponent<CharacterSlot>().SetInfo(info.Image, info.Name, character.Id, this);

                Button button = slot.transform.Find("GalleryCharacter").GetComponent<Button>();

                Outline outline = button.gameObject.GetComponent<Outline>();

                outline.effectDistance = new Vector2(3, 3);
                outline.effectColor = GetCharacterClassColor(character.ClassID);
                _colorBlock.normalColor = GetCharacterClassColor(default);
                button.colors = _colorBlock;

                _buttons.Add(button);
                _characterSlot.Add(slot.GetComponent<CharacterSlot>());
            }

            int idx = 0;
            for (int i = 0; i < _buttons.Count && i < _characterSlot.Count; i++)
            {
                Button button = _buttons[i];
                CharacterSlot characterSlot = _characterSlot[i];

                if (_CurSelectedCharacterSlot[0].Id == characterSlot.Id ||
                    _CurSelectedCharacterSlot[1].Id == characterSlot.Id ||
                    _CurSelectedCharacterSlot[2].Id == characterSlot.Id)
                {
                    continue;
                }

                foreach (CustomCharacter customCharacter in characters)
                {
                    if (characterSlot.Id != customCharacter.Id) continue;
                    
                    else
                    {
                        _colorBlock.normalColor = GetCharacterClassColor(customCharacter.CharacterClassID);
                        button.colors = _colorBlock;
                        button.GetComponent<DraggableCharacter>().enabled = true;
                    }

                    foreach (CharacterID curCharacter in currentCharacterId)
                    {
                        if (curCharacter == customCharacter.Id && idx < _CurSelectedCharacterSlot.Length)
                        {
                            // Set the character in the horizontal character slot
                            if (_CurSelectedCharacterSlot.Length > 0)
                            {
                                button.transform.SetParent(_CurSelectedCharacterSlot[idx].transform, false);
                                idx++;
                            }
                        }
                    }
                    // Check if the character is currently selected
                    // Subscribe to the event of parent change for the button 
                    var parentChangeMonitor = button.GetComponent<DraggableCharacter>();
                    parentChangeMonitor.OnParentChanged += newParent =>
                    {
                        // Go through each topslot
                        foreach (var curSlot in _CurSelectedCharacterSlot)
                        {
                            // Check if newParent is one of the topslots
                            if (newParent == curSlot.transform)
                            {
                                // Set characterID, because it has been moved to the topslot
                                CurrentCharacterId = customCharacter.Id;
                            }
                        }
                    };
                }
            }
        }
    }
}


