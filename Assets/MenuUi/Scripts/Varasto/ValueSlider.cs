using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class ValueSlider : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Slider _slider;

        public void Start()
        {
            _inputField.onEndEdit.AddListener(delegate { UpdateSliderValue(); });
            _slider.onValueChanged.AddListener(delegate { UpdateInputFieldValue(); });
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
