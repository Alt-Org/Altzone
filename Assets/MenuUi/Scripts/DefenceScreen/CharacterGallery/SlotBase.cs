using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// TODO: write doc comment
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
    }
}
