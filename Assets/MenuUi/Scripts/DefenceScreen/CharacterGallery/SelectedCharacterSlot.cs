using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharacterSlot : SlotBase
    {
        [SerializeField] private Image _selectedImage;

        [HideInInspector] public bool IsSelected = false;
        [HideInInspector] public int SlotIndex = 0;

        public delegate void SlotSelectedEventHandler(int slotIndex);
        public SlotSelectedEventHandler OnSlotSelected;

        public Action OnSelectedSlotDeselected;

        public void SelectSlot()
        {
            if (!IsSelected)
            {
                _selectedImage.enabled = true;
                IsSelected = true;
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
            if (IsSelected)
            {
                _selectedImage.enabled = false;
                IsSelected = false;

                GalleryCharacter galleryCharacter = GetComponentInChildren<GalleryCharacter>();
                if (galleryCharacter != null)
                {
                    galleryCharacter.HideRemoveButton();
                }
            }
        }
    }
}
