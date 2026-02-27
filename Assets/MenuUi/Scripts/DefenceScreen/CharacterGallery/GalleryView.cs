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
    /// <summary>
    /// Controls the defence gallery's character gallery view by initializing CharacterInventorySlots to a grid.
    /// </summary>
    public class GalleryView : MonoBehaviour
    {
        enum FilterType
        {
            All = 0,
            Unlocked = 1,
            Locked = 2,
            Test = 3,
            Desensitizer = 100,
            Trickster = 200,
            Obedient = 300,
            Projector = 400,
            Retroflector = 500,
            Confluent = 600,
            Intellectualizer = 700,
        }

        [SerializeField] private Transform _unlockedCharacterGridContent;
        [SerializeField] private Transform _lockedCharacterGridContent;
        [SerializeField] private Button _filterButton;
        [SerializeField] private TMP_Text _filterText;
        [SerializeField] private BaseScrollRect _scrollRect;

        [SerializeField] private GameObject _characterSlotPrefab;

        [SerializeField] private ClassReference _classReference;

        private FilterType _currentFilter = FilterType.All;
        private List<int> filterEnumValues;

        // List of character slots in character grid
        private readonly List<CharacterSlot> _characterSlots = new();
        public List<CharacterSlot> CharacterSlots => _characterSlots;

        public delegate void GalleryCharactersSetHandler(CustomCharacter[] _selectedCharacters);
        public GalleryCharactersSetHandler OnGalleryCharactersSet;

        public Action OnFilterChanged;


        private void Awake()
        {
            // Initializing the list of enum values for filtering
            filterEnumValues = new((int[])Enum.GetValues(typeof(FilterType)));

            if (_lockedCharacterGridContent == null)
            {
                filterEnumValues.Remove((int)FilterType.Locked);
                filterEnumValues.Remove((int)FilterType.Unlocked);
            }

            // Adding listener to filter button and setting the initial filter text
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
            // Remove all character slots
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                Destroy(characterSlot.Character.gameObject);
                Destroy(characterSlot.gameObject);
            }
            _characterSlots.Clear();
        }


        /// <summary>
        /// Set filter button visibility.
        /// </summary>
        /// <param name="show">If the filter button should be shown or not.</param>
        public void ShowFilterButton(bool show)
        {
            _filterButton.gameObject.SetActive(show);
        }


        /// <summary>
        /// Place the characters to character gallery.
        /// </summary>
        /// <param name="customCharacters">List of player's custom (owned) characters.</param>
        /// <param name="selectedCharacterIds">Array of selected character ids.</param>
        public void SetCharacters(List<CustomCharacter> customCharacters, CustomCharacterListObject[] selectedCharacterIds)
        {
            Reset();

            CustomCharacter[] selectedCharacters = new CustomCharacter[3];

            // Placing unlocked characters
            foreach (CustomCharacter character in customCharacters)
            {
                var charSlot = InstantiateCharacterSlot(character.Id, false, _unlockedCharacterGridContent);

                // Going through selectedCharacterIds to check if the CustomCharacter is one of the selected characters
                for (int i = 0; i < selectedCharacterIds.Length; i++)
                {
                    if (character.Id == selectedCharacterIds[i].CharacterID)
                    {
                        // Adding the CustomCharacter to the array of selected characters
                        selectedCharacters[i] = character;
                        break;
                    }
                }
            }

            // Placing locked characters
            if (_lockedCharacterGridContent != null)
            {
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

                    InstantiateCharacterSlot(baseCharacter.Id, true, _lockedCharacterGridContent);
                }
            }

            // Invoking the event with selectedCharacters CustomCharacter array
            OnGalleryCharactersSet?.Invoke(selectedCharacters);
        }


        private CharacterSlot InstantiateCharacterSlot(CharacterID charID, bool isLocked, Transform parent)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(((int)charID).ToString());
            if (info == null) return null;

            GameObject slot = Instantiate(_characterSlotPrefab, parent);

            CharacterClassType classType = CustomCharacter.GetClass(charID);
            Sprite classNameIcon = _classReference.GetNameIcon(classType);
            Color bgColor = _classReference.GetColor(classType);
            Color bgAltColor = _classReference.GetAlternativeColor(classType);
            Sprite classIcon = _classReference.GetCornerIcon(classType);


            CharacterSlot charSlot = slot.GetComponent<CharacterSlot>();
            charSlot.SetInfo(info.GalleryImage, bgColor, bgAltColor, info.Name, classNameIcon, classIcon, charID);

            _characterSlots.Add(charSlot);

            if (isLocked)
            {
                charSlot.Character.SetLockedVisuals();
                charSlot.IsLocked = true;
            }
            else
            {
                charSlot.Character.SetDefaultVisuals();
            }

            return charSlot;
        }

        private void RotateFilters()
        {
            for (int i = 0; i < filterEnumValues.Count; i++)
            {
                if ((int)_currentFilter == filterEnumValues[i])
                {
                    if (i + 1 < filterEnumValues.Count)
                    {
                        SetFilter((FilterType)filterEnumValues[i + 1]);
                    }
                    else
                    {
                        SetFilter((FilterType)filterEnumValues[0]);
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

                case FilterType.Test:
                    foreach (CharacterSlot characterSlot in _characterSlots)
                    {
                        characterSlot.gameObject.SetActive(CustomCharacter.IsTestCharacter(characterSlot.Id));
                    }
                    break;

                case FilterType.Desensitizer: // Only showing desensitizers
                    FilterForClassID(CharacterClassType.Desensitizer);
                    break;

                case FilterType.Trickster: // Only showing tricksters
                    FilterForClassID(CharacterClassType.Trickster);
                    break;

                case FilterType.Obedient: // Only showing obedients
                    FilterForClassID(CharacterClassType.Obedient);
                    break;

                case FilterType.Projector: // Only showing projectors
                    FilterForClassID(CharacterClassType.Projector);
                    break;

                case FilterType.Retroflector: // Only showing retroflectors
                    FilterForClassID(CharacterClassType.Retroflector);
                    break;

                case FilterType.Confluent: // Only showing confluents
                    FilterForClassID(CharacterClassType.Confluent);
                    break;

                case FilterType.Intellectualizer: // Only showing intellectualizers
                    FilterForClassID(CharacterClassType.Intellectualizer);
                    break;
            }

            SetFilterText(filter);
            _currentFilter = filter;
            OnFilterChanged?.Invoke();
        }


        private void FilterForClassID(CharacterClassType classType)
        {
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                if (CustomCharacter.GetClass(characterSlot.Character.Id) == classType)
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

                case FilterType.Test:
                    _filterText.text = "Testihahmot";
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
