using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderPercentageText : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    // Update is called once per frame
    void Update()
    {
        float normalizedValue = Mathf.InverseLerp(
            slider.minValue,
            slider.maxValue,
            slider.value
        );

        valueText.text = Mathf.RoundToInt(normalizedValue * 100).ToString();
    }
}
