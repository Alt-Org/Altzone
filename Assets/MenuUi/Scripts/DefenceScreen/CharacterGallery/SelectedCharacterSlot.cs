using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharacterSlot : SlotBase
    {
        [SerializeField] private Image _selectedImage;

        private bool _isSelected = false;

        [HideInInspector] public int SlotIndex = 0;

        public delegate void SlotSelectedEventHandler(int slotIndex);
        public SlotSelectedEventHandler OnSlotSelected;

        public Action OnSelectedSlotDeselected;

        public void SelectSlot()
        {
            if (!_isSelected)
            {
                _selectedImage.enabled = true;
                _isSelected = true;
                OnSlotSelected?.Invoke(SlotIndex);

                GalleryCharacter galleryCharacter = GetComponentInChildren<GalleryCharacter>();
                if (galleryCharacter != null)
                {
                    galleryCharacter.ShowRemoveButton();
                }
            }
            else
            {
                DeSelectSlot();
                OnSelectedSlotDeselected?.Invoke();
            }
        }

        public void DeSelectSlot()
        {
            if (_isSelected)
            {
                _selectedImage.enabled = false;
                _isSelected = false;

                GalleryCharacter galleryCharacter = GetComponentInChildren<GalleryCharacter>();
                if (galleryCharacter != null)
                {
                    galleryCharacter.HideRemoveButton();
                }
            }
        }
    }
}
