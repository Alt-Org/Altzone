using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine.UI;
using MenuUi.Scripts.Signals;
using TMPro;
using System;

namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
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
        [SerializeField] private Toggle _editModeToggle;
        [SerializeField] private Button _filterButton;
        [SerializeField] private TMP_Text _filterText;
        [SerializeField] private BaseScrollRect _scrollRect;

        [SerializeField] private GameObject _characterSlotPrefab;

        [SerializeField] private ClassColorReference _classColorReference;

        private bool _isReady;
        private FilterType _currentFilter = FilterType.All;

        // Array of character slots in selected grid
        [SerializeField] private SelectedCharacterSlot[] _selectedCharacterSlots;
        // List of character slots in character grid
        private List<CharacterSlot> _characterSlots = new();

        public delegate void TopSlotCharacterSetHandler(CharacterID characterId, int slotIdx);
        public event TopSlotCharacterSetHandler OnTopSlotCharacterSet;

        private SwipeUI _swipe;

        public bool IsReady
        {
            get
            {
                return _isReady;
            }
        }


        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>(true);
            _swipe.OnCurrentPageChanged += ChangeEditToggleStatusToFalse;

            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                _selectedCharacterSlots[i].OnCharacterSelected += HandleCharacterSelected;
                _selectedCharacterSlots[i].SlotIndex = i;
            }

            SignalBus.OnDefenceGalleryEditModeRequested += ChangeEditToggleStatusToTrue;
            _filterButton.onClick.AddListener(RotateFilters);
            SetFilterText(_currentFilter);
        }


        private void OnDisable()
        {
            ChangeEditToggleStatusToFalse();
        }


        private void OnDestroy()
        {
            foreach (SelectedCharacterSlot slot in _selectedCharacterSlots)
            {
                slot.OnCharacterSelected -= HandleCharacterSelected;
            }

            foreach (CharacterSlot slot in _characterSlots)
            {
                slot.OnCharacterSelected -= HandleCharacterSelected;
            }

            _swipe.OnCurrentPageChanged -= ChangeEditToggleStatusToFalse;
            SignalBus.OnDefenceGalleryEditModeRequested -= ChangeEditToggleStatusToTrue;
        }


        public void Reset()
        {
            _isReady = false;

            // Remove selected characters
            foreach (SelectedCharacterSlot slot in _selectedCharacterSlots)
            {
                GalleryCharacter topSlotCharacter = slot.transform.GetComponentInChildren<GalleryCharacter>();
                if (topSlotCharacter != null)
                {
                    Destroy(topSlotCharacter.gameObject);
                }
            }

            // Remove all character slots
            foreach (CharacterSlot characterSlot in _characterSlots)
            {
                characterSlot.OnCharacterSelected -= HandleCharacterSelected;
                Destroy(characterSlot.gameObject);
            }
            _characterSlots.Clear();
            _isReady = true;
        }


        /// <summary>
        /// Set edit toggle status to true.
        /// </summary>
        public void ChangeEditToggleStatusToTrue()
        {
            _editModeToggle.isOn = true;
        }


        /// <summary>
        /// Set edit toggle status to false.
        /// </summary>
        public void ChangeEditToggleStatusToFalse()
        {
            _editModeToggle.isOn = false;
        }


        /// <summary>
        /// Toggle edit mode based on the value of edit mode toggle.
        /// </summary>
        public void ToggleEditMode()
        {
            SetCharacterSlotsSelectable(_editModeToggle.isOn);
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

                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    if (charSlot == null) break;

                    if (character.Id == (CharacterID)selectedCharacterIds[i]) // Check if character is selected
                    {
                        charSlot.Character.transform.SetParent(_selectedCharacterSlots[i].transform, false);
                        charSlot.Character.SetSelectedVisuals();
                    }
                }
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

            // ensures character slots are selectable if edit toggle is on, it can happen if adding unowned character from the + button while edit mode is on
            if (_editModeToggle.isOn)
            {
                SetCharacterSlotsSelectable(true);
            }
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
            charSlot.OnCharacterSelected += HandleCharacterSelected;

            if (isLocked)
            {
                charSlot.Character.SetLockedVisuals();
                charSlot.IsLocked = true;
            }

            return charSlot;
        }


        private void SetCharacterSlotsSelectable(bool selectable)
        {
            foreach (CharacterSlot charSlot in _characterSlots)
            {
                charSlot.SetSelectable(selectable);
            }

            foreach (SelectedCharacterSlot selectedSlot in _selectedCharacterSlots)
            {
                selectedSlot.SetSelectable(selectable);
            }
        }


        private void HandleCharacterSelected(SlotBase pressedSlot)
        {
            GalleryCharacter galleryCharacter = pressedSlot.GetComponentInChildren<GalleryCharacter>();

            SelectedCharacterSlot selectedCharacterSlot = pressedSlot as SelectedCharacterSlot;
            if (selectedCharacterSlot != null && galleryCharacter != null)
            {
                galleryCharacter.ReturnToOriginalSlot();
                SetTopSlotCharacter(CharacterID.None, selectedCharacterSlot.SlotIndex);
            }
            else if (galleryCharacter != null && !pressedSlot.IsLocked) // can only place owned characters to top slots
            {
                PlaceCharacterToTopSlot(galleryCharacter);
            }
        }


        private void PlaceCharacterToTopSlot(GalleryCharacter galleryCharacter)
        {
            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                if (_selectedCharacterSlots[i].GetComponentInChildren<GalleryCharacter>() == null)
                {
                    galleryCharacter.transform.SetParent(_selectedCharacterSlots[i].transform, false);
                    galleryCharacter.SetSelectedVisuals();
                    SetTopSlotCharacter(galleryCharacter.Id, i);
                    return;
                }
            }
        }


        private void SetTopSlotCharacter(CharacterID id, int selectedSlotIdx)
        {
            OnTopSlotCharacterSet?.Invoke(id, selectedSlotIdx);
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
