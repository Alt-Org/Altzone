using System;
using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
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
            //Poistaa myös valitut hahmot!!!
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }
            // remove all character slots
            foreach (var characterSlot in _characterSlot)
            {
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
        public void CheckSelectedCharacterSlotText(bool isAdditive)
        {
            //TODO: tee näistä SerializeField
            //Ois myös kiva jos tämä ylipäätään toimis.
            var text1 = GameObject.FindGameObjectWithTag("TextSuoja1");
            var text2 = GameObject.FindGameObjectWithTag("TextSuoja2");
            var text3 = GameObject.FindGameObjectWithTag("TextSuoja3");

            if (isAdditive == true)
            {
                characterTextCounter++;
            }
            else
            {
                characterTextCounter--;
            }
            if (characterTextCounter < 0) characterTextCounter = 0;
            if (characterTextCounter > 3) characterTextCounter = 3;
            
            /*if (_CurSelectedCharacterSlot[2].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(false);
            }
            else if (_CurSelectedCharacterSlot[1].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(true);
            }     
            else if (_CurSelectedCharacterSlot[0].transform.childCount > 0)
            {
                text1.SetActive(false);
                text2.SetActive(true);
                text3.SetActive(true);
            }     
            else
            {
                text1.SetActive(true);
                text2.SetActive(true);
                text3.SetActive(true);
            }*/

            if (characterTextCounter > 2)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(false);
            }
            else if (characterTextCounter > 1)
            {
                text1.SetActive(false);
                text2.SetActive(false);
                text3.SetActive(true);
            }     
            else if (characterTextCounter > 0)
            {
                text1.SetActive(false);
                text2.SetActive(true);
                text3.SetActive(true);
            }     
            else
            {
                text1.SetActive(true);
                text2.SetActive(true);
                text3.SetActive(true);
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
            foreach (var id in currentCharacterId)
            {
                CurrentCharacterId = (CharacterID)id;
            }

            var store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (var character in allItems)
            {
                GameObject slot = Instantiate(_characterSlotprefab, GetContent());

                GalleryCharacterInfo info = _referenceSheet.GetCharacterPrefabInfoFast((int)character.Id);

                if (info == null) continue;

                slot.GetComponent<CharacterSlot>().SetInfo(info.Image, info.Name, character.Id, this);

                _characterSlot.Add(slot.GetComponent<CharacterSlot>());
                _buttons.Add(slot.transform.Find("Button").GetComponent<Button>());
            }

            /*foreach (var character in characters)
            {
            }*/

            for (int i = 0; i < _buttons.Count && i < _characterSlot.Count; ++i)
            {
                var button = _buttons[i];
                var characterSlot = _characterSlot[i];

                if (i < characters.Count)
                {
                    var character = characters[i];

                    button.gameObject.SetActive(true);
                    button.interactable = true;
                    _colorBlock.normalColor = GetCharacterClassColor(character.CharacterClassID);
                    button.colors = _colorBlock;
                    //button.SetCaption(character.Name); // Set button caption to character name

                    characterSlot.gameObject.SetActive(true);

                    //***************
                    // Check if the character is currently selected
                    if ((CharacterID)currentCharacterId[0] == character.Id)
                    {
                        // Set the character in the first slot of the horizontal character slot
                        if (_CurSelectedCharacterSlot.Length > 0)
                        {
                            button.transform.SetParent(_CurSelectedCharacterSlot[0].transform, false);
                        }
                    }

                    //Tekstin vaihto tähän /characterSlot
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
                                CurrentCharacterId = character.Id;
                            }
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


