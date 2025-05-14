using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Base class for defence gallery character slots. Has a serializefield for the slot's button and an event which gets invoked on button press.
    /// </summary>
    public class SlotBase : MonoBehaviour
    {
        [SerializeField] protected Button _slotButton;

        public delegate void SlotPressedHandler(SlotBase slot);
        public SlotPressedHandler OnSlotPressed;


        private void Awake()
        {
            if (_slotButton == null) return;
            _slotButton.onClick.AddListener(SlotButtonPressed);
        }


        private void OnDestroy()
        {
            if (_slotButton == null) return;
            _slotButton.onClick.RemoveAllListeners();
        }

        public void SetEditable(bool editable)
        {
            if (_slotButton == null) return;
            _slotButton.gameObject.SetActive(editable);
        }

        public void SlotButtonPressed()
        {
            OnSlotPressed?.Invoke(this);
        }

        private void OnTransformChildrenChanged()
        {
            if (_slotButton != null) _slotButton.transform.SetAsLastSibling();
        }
    }
}
