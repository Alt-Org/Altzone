using System.Collections.Generic;
using Altzone.Scripts;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine.UI;
using System.Linq;
namespace MenuUi.Scripts.CharacterGallery
{
    public class ModelView : MonoBehaviour
    {
        [SerializeField] private Transform _characterGridContent;
        [SerializeField] private Toggle _editModeToggle;

        [SerializeField] private GameObject _characterSlotPrefab;

        [SerializeField] private ClassColorReference _classColorReference;

        private bool _isReady;

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


        public void ChangeEditToggleStatusToFalse()
        {
            _editModeToggle.isOn = false;
        }


        public void ToggleEditMode()
        {
            SetCharacterSlotsSelectable(_editModeToggle.isOn);
        }


        public void SetCharacters(List<CustomCharacter> characters, int[] currentCharacterIds)
        {
            DataStore store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach (BaseCharacter character in allItems)
            {
                PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(((int)character.Id).ToString());
                if (info == null) continue;

                GameObject slot = Instantiate(_characterSlotPrefab, _characterGridContent);

                CharacterClassID classID = CustomCharacter.GetClassID(character.Id);
                Color bgColor = _classColorReference.GetColor(classID);
                Color bgAltColor = _classColorReference.GetAlternativeColor(classID);

                CharacterSlot charSlot = slot.GetComponent<CharacterSlot>();
                charSlot.SetInfo(info.GalleryImage, bgColor, bgAltColor, info.Name, character.Id);

                _characterSlots.Add(charSlot);
                charSlot.OnCharacterSelected += HandleCharacterSelected;

                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    if (character.Id == (CharacterID)currentCharacterIds[i])
                    {
                        charSlot.Character.transform.SetParent(_selectedCharacterSlots[i].transform, false);
                        charSlot.Character.SetSelectedVisuals();
                    }
                }
            }
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
            else if (galleryCharacter != null)
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
    }
}
