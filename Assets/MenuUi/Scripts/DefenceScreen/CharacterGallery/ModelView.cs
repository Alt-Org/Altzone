using System;
using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.ModelV2;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform VerticalContentPanel;
        [SerializeField] private Transform HorizontalContentPanel;

        [SerializeField] private GameObject _characterSlotprefab;
        [SerializeField] private GalleryCharacterReference _referenceSheet;

        [SerializeField] private GameObject _selectedCharacterSlotText1;
        [SerializeField] private GameObject _selectedCharacterSlotText2;
        [SerializeField] private GameObject _selectedCharacterSlotText3;

        [SerializeField] private bool _isReady;

        // character buttons
        private List<Button> _characterButtons = new();

        // Array of character slots in horizontalpanel
        public CharacterSlot[] _CurSelectedCharacterSlots { get; private set; }
        // array of character slots in verticalpanel
        private List<CharacterSlot> _characterSlots = new();

        public delegate void CurrentCharacterIdChangedHandler(CharacterID newCharacterId, int slot);
        public event CurrentCharacterIdChangedHandler OnCurrentCharacterIdChanged;
        public bool IsReady => _isReady;
        public int characterTextCounter;

        private CharacterID _currentCharacterId;
        private int _slotToSet = 0;

        public ColorBlock _colorBlock = new();

        public CharacterID CurrentCharacterId
        {
            get => _currentCharacterId;
            private set
            {
                _currentCharacterId = value;
                OnCurrentCharacterIdChanged?.Invoke(_currentCharacterId, _slotToSet);

            }
        }


        private void Awake()
        {
            _CurSelectedCharacterSlots = HorizontalContentPanel.GetComponentsInChildren<CharacterSlot>();
        }


        private void LoadAndCachePrefabs()
        {
            var gameConfig = GameConfig.Get();
            var playerPrefabs = gameConfig.PlayerPrefabs;
            var prefabs = playerPrefabs._playerPrefabs;
            for (var prefabIndex = 0; prefabIndex < prefabs.Length; ++prefabIndex)
            {
                var playerPrefab = GameConfig.Get().PlayerPrefabs.GetPlayerPrefab(prefabIndex);
                //Debug.Log($"prefabIndex {prefabIndex} playerPrefab {playerPrefab.name}");
            }
            _isReady = true;
        }


        public void Reset()
        {
            _isReady = false;

            foreach (var slot in _CurSelectedCharacterSlots)
            {
                var topSlotCharacter = slot.transform.GetComponentInChildren<DraggableCharacter>();
                if (topSlotCharacter != null)
                {
                    Destroy(topSlotCharacter.gameObject);
                }
            }
            foreach (var button in _characterButtons)
            {
                if (!button.transform.IsChildOf(HorizontalContentPanel))
                    Destroy(button.gameObject);
            }
            // remove all character slots
            foreach (var characterSlot in _characterSlots)
            {
                if (!characterSlot.transform.IsChildOf(HorizontalContentPanel))
                    Destroy(characterSlot.gameObject);
            }
            _characterButtons.Clear();
            _characterSlots.Clear();
            LoadAndCachePrefabs();
            CheckSelectedCharacterSlotTexts();
        }


        public Transform GetContent()
        {
            Transform content = (VerticalContentPanel == null) ? transform.Find("Content") :
                VerticalContentPanel.transform;

            return content;
        }


        public void CheckSelectedCharacterSlotTexts()
        {
            if (_CurSelectedCharacterSlots[2].transform.childCount > 0)
            {
                _selectedCharacterSlotText3.SetActive(false);
            }
            else
            {
                _selectedCharacterSlotText3.SetActive(true);
            }

            if (_CurSelectedCharacterSlots[1].transform.childCount > 0)
            {
                _selectedCharacterSlotText2.SetActive(false);
            }
            else
            {
                _selectedCharacterSlotText2.SetActive(true);
            }

            if (_CurSelectedCharacterSlots[0].transform.childCount > 0)
            {
                _selectedCharacterSlotText1.SetActive(false);
            }
            else
            {
                _selectedCharacterSlotText1.SetActive(true);
            }
        }


        public void SetCharacters(List<CustomCharacter> characters, int[] currentCharacterIds)
        {
            var store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (var character in allItems)
            {
                //GalleryCharacterInfo info = _referenceSheet.GetCharacterPrefabInfoFast((int)character.Id);
                var info2 = PlayerCharacterPrototypes.GetCharacter(((int)character.Id).ToString());
                if (info2 == null) continue;

                GameObject slot = Instantiate(_characterSlotprefab, GetContent());
                slot.GetComponent<CharacterSlot>().SetInfo(info2.GalleryImage, info2.Name, character.Id, this);

                Button button = slot.transform.Find("GalleryCharacter").GetComponent<Button>();

                Outline outline = button.gameObject.GetComponent<Outline>();

                outline.effectDistance = new Vector2(3, 3);
                outline.effectColor = CharacterClass.GetCharacterClassColor(character.ClassID);
                _colorBlock.normalColor = CharacterClass.GetCharacterClassColor(default);
                button.colors = _colorBlock;

                _characterButtons.Add(button);
                _characterSlots.Add(slot.GetComponent<CharacterSlot>());
            }

            for (int i = 0; i < _characterButtons.Count && i < _characterSlots.Count; i++)
            {
                Button button = _characterButtons[i];
                CharacterSlot characterSlot = _characterSlots[i];

                if (_CurSelectedCharacterSlots[0].Id == characterSlot.Id ||
                    _CurSelectedCharacterSlots[1].Id == characterSlot.Id ||
                    _CurSelectedCharacterSlots[2].Id == characterSlot.Id)
                {
                    continue;
                }

                foreach (CustomCharacter customCharacter in characters)
                {
                    if (characterSlot.Id != customCharacter.Id) continue;

                    else
                    {
                        _colorBlock.normalColor = CharacterClass.GetCharacterClassColor(customCharacter.CharacterClassID);
                        button.colors = _colorBlock;
                        button.GetComponent<DraggableCharacter>().enabled = true;
                    }
                    // Check if the character is currently selected
                    // Subscribe to the event of parent change for the button 
                    var parentChangeMonitor = button.GetComponent<DraggableCharacter>();
                    parentChangeMonitor.OnParentChanged += newParent =>
                    {
                        int i = 0;
                        // Go through each topslot
                        foreach (var curSlot in _CurSelectedCharacterSlots)
                        {
                            // Check if newParent is one of the topslots
                            if (newParent == curSlot.transform)
                            {
                                _slotToSet = i;
                                // Set characterID, because it has been moved to the topslot
                                CurrentCharacterId = customCharacter.Id;
                                break;
                            }
                            i++;
                        }

                        CheckSelectedCharacterSlotTexts();
                    };

                    // subscribing to removed from top slot event
                    button.GetComponent<DraggableCharacter>().OnRemovedFromTopSlot += ReorderSelectedCharacters;
                }
            }

            int idx = 0;
            foreach (CharacterID curCharacter in currentCharacterIds)
            {
                if (curCharacter == 0) continue;
                foreach (Button button in _characterButtons)
                {
                    CharacterID id = button.GetComponent<DraggableCharacter>().Id;
                    if (curCharacter == id && idx < _CurSelectedCharacterSlots.Length)
                    {
                        // Set the character in the horizontal character slot
                        if (_CurSelectedCharacterSlots.Length > 0)
                        {
                            button.transform.SetParent(_CurSelectedCharacterSlots[idx].transform, false);
                            idx++;
                            break;
                        }
                    }
                }
            }

            CheckSelectedCharacterSlotTexts();
        }


        /// <summary>
        /// Reorders selected characters to the left and saves it.
        /// </summary>
        public void ReorderSelectedCharacters()
        {
            List<DraggableCharacter> characters = new List<DraggableCharacter>();

            foreach (CharacterSlot slot in _CurSelectedCharacterSlots) // if slot has character add its character to list
            {
                if (slot.transform.childCount > 0)
                {
                    characters.Add(slot.transform.GetComponentInChildren<DraggableCharacter>());
                }
            }

            for (int i = 0; i < characters.Count; i++) // reparent the characters starting from the leftmost characterslot
            {
                characters[i].transform.SetParent(_CurSelectedCharacterSlots[i].transform);
            }

            for (int i = 0; i < _CurSelectedCharacterSlots.Length; i++) // save character ids to slots through assigning CurrentCharacterId
            {
                _slotToSet = i;
                if (i < characters.Count)
                {
                    CurrentCharacterId = characters[i].Id;
                }
                else
                {
                    CurrentCharacterId = CharacterID.None;
                }
            }

            CheckSelectedCharacterSlotTexts();
        }
    }
}
