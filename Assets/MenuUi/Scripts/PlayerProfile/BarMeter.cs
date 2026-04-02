using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarMeter : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _maxValue = 120f;
    [SerializeField] private TextMeshProUGUI _valueText;

    [SerializeField] private RectTransform _statueTransform;
    [SerializeField] private float _minY;
    [SerializeField] private float _maxY;

    private void Awake()
    {
        if (_slider == null)
            _slider = GetComponent<Slider>();

        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = _maxValue;
        }
    }

    private void Update()
    {
        UpdateValue();
    }

    public void UpdateValue()
    {
        if (_slider == null)
            return;

        float carbonValue = CarbonFootprint.CarbonCount;
        float clampedValue = Mathf.Clamp(carbonValue, 0f, _maxValue);

        _slider.value = clampedValue;

        if (_valueText != null)
        {
            if (carbonValue >= 1000f)
                _valueText.text = $"{carbonValue / 1000f:F1} kg";
            else
                _valueText.text = $"{carbonValue:F1} g";
        }

        if (_statueTransform != null)
        {
            float normalized = _slider.normalizedValue;
            Vector2 pos = _statueTransform.anchoredPosition;
            pos.y = Mathf.Lerp(_minY, _maxY, normalized);
            _statueTransform.anchoredPosition = pos;
        }
    }
}
