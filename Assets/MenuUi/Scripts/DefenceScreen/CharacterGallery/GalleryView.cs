using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.CharacterGallery
{
    public class GalleryView : MonoBehaviour
    {
        enum FilterType
        {
            All = 0,
            Unlocked = 1,
            Locked = 2,
            Desensitizer = 100,
            Trickster = 200,
            Obedient = 300,
            Projector = 400,
            Retroflector = 500,
            Confluent = 600,
            Intellectualizer = 700,
        }

        [SerializeField] private Transform _characterGridContent;
        [SerializeField] private Button _filterButton;
        [SerializeField] private TMP_Text _filterText;
        [SerializeField] private BaseScrollRect _scrollRect;

        [SerializeField] private GameObject _characterSlotPrefab;

        [SerializeField] private ClassColorReference _classColorReference;

        private bool _isReady;
        private FilterType _currentFilter = FilterType.All;
        
        // List of character slots in character grid
        private List<CharacterSlot> _characterSlots = new();
        public List<CharacterSlot> CharacterSlots => _characterSlots;

        public bool IsReady => _isReady;

        public delegate void GalleryCharactersSetHandler(int[] _selectedCharacterIds);
        public GalleryCharactersSetHandler OnGalleryCharactersSet;


        private void Awake()
        {
            _filterButton.onClick.AddListener(RotateFilters);
            SetFilterText(_currentFilter);
        }

        private void OnEnable()
        {
            _scrollRect.VerticalNormalizedPosition = 1; // setting scroll to the top
        }

        private void OnDestroy()
        {
            _filterButton.onClick.RemoveAllListeners();
        }

        public void Reset()
        {
            _isReady = false;

            // Remove all character slots
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                Destroy(characterSlot.Character);
                Destroy(characterSlot.gameObject);
            }
            _characterSlots.Clear();
            _isReady = true;
        }

        /// <summary>
        /// Place the characters to character gallery.
        /// </summary>
        /// <param name="customCharacters">List of player's custom (owned) characters.</param>
        /// <param name="selectedCharacterIds">Array of selected character ids which will be placed to the top slot.</param>
        public void SetCharacters(List<CustomCharacter> customCharacters, int[] selectedCharacterIds)
        {
            // Placing unlocked characters
            foreach (CustomCharacter character in customCharacters)
            {
                var charSlot = InstantiateCharacterSlot(character.Id, false);
            }

            // Placing locked characters
            DataStore store = Storefront.Get();
            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (BaseCharacter baseCharacter in allItems)
            {
                // Checking if player has already unlocked the character and if so, skipping the character
                bool characterUnlocked = false;
                foreach (CharacterSlot slot in _characterSlots)
                {
                    if (slot.Character.Id == baseCharacter.Id)
                    {
                        characterUnlocked = true;
                        break;
                    }
                }
                if (characterUnlocked) continue;

                InstantiateCharacterSlot(baseCharacter.Id, true);
            }

            OnGalleryCharactersSet?.Invoke(selectedCharacterIds);
        }


        private CharacterSlot InstantiateCharacterSlot(CharacterID charID, bool isLocked)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(((int)charID).ToString());
            if (info == null) return null;

            GameObject slot = Instantiate(_characterSlotPrefab, _characterGridContent);

            CharacterClassID classID = CustomCharacter.GetClassID(charID);
            Color bgColor = _classColorReference.GetColor(classID);
            Color bgAltColor = _classColorReference.GetAlternativeColor(classID);

            CharacterSlot charSlot = slot.GetComponent<CharacterSlot>();
            charSlot.SetInfo(info.GalleryImage, bgColor, bgAltColor, info.Name, charID);

            _characterSlots.Add(charSlot);

            if (isLocked)
            {
                charSlot.Character.SetLockedVisuals();
                charSlot.IsLocked = true;
            }

            return charSlot;
        }
       
        private void RotateFilters()
        {
            int[] enumValues = (int[])Enum.GetValues(typeof(FilterType));

            for (int i = 0; i < enumValues.Length; i++)
            {
                if ((int)_currentFilter == enumValues[i])
                {
                    if (i + 1 < enumValues.Length)
                    {
                        SetFilter((FilterType)enumValues[i + 1]);
                    }
                    else
                    {
                        SetFilter((FilterType)enumValues[0]);
                    }
                    break;
                }
            }

            _scrollRect.VerticalNormalizedPosition = 1; // setting scroll to the top so that it's not possibly scrolled too far
        }


        private void SetFilter(FilterType filter)
        {
            switch (filter)
            {
                case FilterType.All: // Showing all characters
                    foreach (CharacterSlot characterSlot in _characterSlots)
                    {
                        if (!characterSlot.gameObject.activeSelf) characterSlot.gameObject.SetActive(true);
                    }
                    break;

                case FilterType.Unlocked: // Only showing unlocked characters
                    foreach (CharacterSlot characterSlot in _characterSlots)
                    {
                        characterSlot.gameObject.SetActive(!characterSlot.IsLocked);
                    }
                    break;

                case FilterType.Locked: // Only showing locked characters
                    foreach (CharacterSlot characterSlot in _characterSlots)
                    {
                        characterSlot.gameObject.SetActive(characterSlot.IsLocked);
                    }
                    break;

                case FilterType.Desensitizer: // Only showing desensitizers
                    FilterForClassID(CharacterClassID.Desensitizer);
                    break;

                case FilterType.Trickster: // Only showing tricksters
                    FilterForClassID(CharacterClassID.Trickster);
                    break;

                case FilterType.Obedient: // Only showing obedients
                    FilterForClassID(CharacterClassID.Obedient);
                    break;

                case FilterType.Projector: // Only showing projectors
                    FilterForClassID(CharacterClassID.Projector);
                    break;

                case FilterType.Retroflector: // Only showing retroflectors
                    FilterForClassID(CharacterClassID.Retroflector);
                    break;

                case FilterType.Confluent: // Only showing confluents
                    FilterForClassID(CharacterClassID.Confluent);
                    break;

                case FilterType.Intellectualizer: // Only showing intellectualizers
                    FilterForClassID(CharacterClassID.Intellectualizer);
                    break;
            }

            SetFilterText(filter);
            _currentFilter = filter;
        }


        private void FilterForClassID(CharacterClassID classID)
        {
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                if (CustomCharacter.GetClassID(characterSlot.Character.Id) == classID)
                {
                    if (!characterSlot.gameObject.activeSelf) characterSlot.gameObject.SetActive(true);
                }
                else
                {
                    if (characterSlot.gameObject.activeSelf) characterSlot.gameObject.SetActive(false);
                }
            }
        }


        private void SetFilterText(FilterType filter)
        {
            switch (filter)
            {
                case FilterType.All:
                    _filterText.text = "Kaikki";
                    break;
                case FilterType.Unlocked:
                    _filterText.text = "Tietoiset";
                    break;
                case FilterType.Locked:
                    _filterText.text = "Tiedostamattomat";
                    break;
                case FilterType.Desensitizer:
                    _filterText.text = "Tunnottomat";
                    break;
                case FilterType.Trickster:
                    _filterText.text = "Hämääjät";
                    break;
                case FilterType.Obedient:
                    _filterText.text = "Tottelijat";
                    break;
                case FilterType.Projector:
                    _filterText.text = "Peilaajat";
                    break;
                case FilterType.Retroflector:
                    _filterText.text = "Torjujat";
                    break;
                case FilterType.Confluent:
                    _filterText.text = "Sulautujat";
                    break;
                case FilterType.Intellectualizer:
                    _filterText.text = "Älyllistäjät";
                    break;
            }
        }

        
    }
}
