using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class SliderController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _sliderText = null;
        [SerializeField] private Slider _mainSlider;
        [SerializeField] private Scrollbar _scrollbar = null;

        private float _maxSliderAmount = 100.0f;

        public float MaxSliderAmount { get => _maxSliderAmount; set => _maxSliderAmount = value; }

        private void Start()
        {
            // Add listener to the main slider's value changed event
            _mainSlider.onValueChanged.AddListener(OnMainSliderValueChanged);
            _scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        }

        public void SliderChange(float value)
        {
            if (_sliderText != null)
            {
                // Update the TextMeshProUGUI text to display the value of the main slider
                float localValue = value * _maxSliderAmount;
                _sliderText.text = localValue.ToString("0");

            }
        }

        public void SetSlider(float value)
        {
            if (_mainSlider != null)
            {
                // Update the TextMeshProUGUI text to display the value of the main slider
                //float localValue = value * maxSliderAmount;
                //_sliderText.text = value.ToString("0");
                _mainSlider.value = value;

            }
        }

        public void SetText(int value)
        {
            if (_sliderText != null)
            {
                // Update the TextMeshProUGUI text to display the value of the main slider
                //float localValue = value * maxSliderAmount;
                _sliderText.text = value.ToString("0");

            }
        }

        private void OnMainSliderValueChanged(float value)
        {
            // Update the values of the other sliders based on the value of the main slider
            SliderChange(value);
            _scrollbar.value = value;
        }
        private void OnScrollbarValueChanged(float value)
        {
            // Update the values of the other sliders based on the value of the main slider
            SetSlider(value);
        }
    }
}
