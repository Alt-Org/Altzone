using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class ValueSliderUpHandler : MonoBehaviour, IPointerUpHandler
    {
        public delegate void ValueSliderUp();
        public event ValueSliderUp OnValueSliderUp;

        public void OnPointerUp(PointerEventData eventData)
        {
            OnValueSliderUp?.Invoke();
        }
    }

    public class ValueSlider : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_InputField.SubmitEvent _inputFieldSubmitEvent;
        [SerializeField] private InvFront _invFront;

        private void Start()
        {
            _inputField.onEndEdit.AddListener(delegate { UpdateSliderValue(); });
            _inputField.onSubmit = _inputFieldSubmitEvent;
            _slider.onValueChanged.AddListener(delegate { UpdateInputFieldValue(); });
            _slider.gameObject.AddComponent<ValueSliderUpHandler>().OnValueSliderUp += _invFront.UpdateInventory;
        }

        private void UpdateSliderValue()
        {
            _slider.value = float.Parse(_inputField.text);
        }

        private void UpdateInputFieldValue()
        {
            _inputField.text = _slider.value.ToString();
        }

        public void SetSliderMaxValue(float value)
        {
            _slider.maxValue = value;
        }

        public float GetSliderValue()
        {
            return _slider.value;
        }
    }
}
