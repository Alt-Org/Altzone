using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class ValueSliderUpHandler : MonoBehaviour, IPointerUpHandler
    {
        public UnityEvent upEvent;

        public void OnPointerUp(PointerEventData eventData)
        {
            upEvent.Invoke();
        }
    }

    public class ValueSlider : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Slider _slider;
        [SerializeField] private UnityEvent _sliderUpEvent;
        [SerializeField] private TMP_InputField.SubmitEvent _inputFieldSubmitEvent;

        private void Start()
        {
            _inputField.onEndEdit.AddListener(delegate { UpdateSliderValue(); });
            _inputField.onSubmit = _inputFieldSubmitEvent;
            _slider.onValueChanged.AddListener(delegate { UpdateInputFieldValue(); });
            _slider.gameObject.AddComponent<ValueSliderUpHandler>().upEvent = _sliderUpEvent;
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
