using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sliderText = null;
    [SerializeField] private Slider mainSlider;
    [SerializeField] private Slider slider1 = null;
    [SerializeField] private Slider slider2 = null;
    [SerializeField] private Slider slider3 = null;
    [SerializeField] private Slider slider4 = null;

    [SerializeField] private float maxSliderAmount = 100.0f;

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
        float localValue = value * maxSliderAmount;
        sliderText.text = localValue.ToString("0");
    
    }
}
    private void OnMainSliderValueChanged(float value)
    {
        // Update the values of the other sliders based on the value of the main slider
        SliderChange(value);
        slider1.value = value;
        slider2.value = value;
        slider3.value = value;
        slider4.value = value;
    }
}