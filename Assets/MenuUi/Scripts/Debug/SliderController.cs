using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugUi.Scripts.BattleAnalyzer
{

    public class SliderController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sliderText = null;
        [SerializeField] private Slider mainSlider;
        [SerializeField] private Slider slider1 = null;
        [SerializeField] private Slider slider2 = null;
        [SerializeField] private Slider slider3 = null;
        [SerializeField] private Slider slider4 = null;

        private float _maxSliderAmount = 100.0f;

        public float MaxSliderAmount { get => _maxSliderAmount; set => _maxSliderAmount = value; }

        private void Start()
        {
            // Add listener to the main slider's value changed event
            mainSlider.onValueChanged.AddListener(OnMainSliderValueChanged);
        }

        public void SliderChange(float value)
        {
            if (sliderText != null)
            {
                // Update the TextMeshProUGUI text to display the value of the main slider
                float localValue = value * _maxSliderAmount;
                sliderText.text = localValue.ToString("0");

            }
        }

        public void SetSlider(int value)
        {
            if (sliderText != null)
            {
                // Update the TextMeshProUGUI text to display the value of the main slider
                //float localValue = value * maxSliderAmount;
                sliderText.text = value.ToString("0");

            }
        }

        private void OnMainSliderValueChanged(float value)
        {
            // Update the values of the other sliders based on the value of the main slider
            SliderChange(value);
        }
    }
}
