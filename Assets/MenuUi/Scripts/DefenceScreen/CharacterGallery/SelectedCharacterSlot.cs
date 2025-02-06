using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharacterSlot : MonoBehaviour
    {
        [SerializeField] private Image _selectedImage;

        [HideInInspector] public bool IsSelected = false;
        [HideInInspector] public int SlotIndex = 0;

        public delegate void SlotSelectedEventHandler(int slotIndex);
        public SlotSelectedEventHandler OnSlotSelected;

        public void SelectSlot()
        {
            if (!IsSelected)
            {
                _selectedImage.enabled = true;
                IsSelected = true;
                OnSlotSelected?.Invoke(SlotIndex);
            }
            else
            {
                DeSelectSlot();
            }
        }

        public void DeSelectSlot()
        {
            if (IsSelected)
            {
                _selectedImage.enabled = false;
                IsSelected = false;
            }
        }
    }
}
