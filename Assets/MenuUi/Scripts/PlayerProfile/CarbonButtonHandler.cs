using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CarbonButtonHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text _buttonText;
    private Button _button;

    public delegate void ToggleCarbonPopup();
    public static event ToggleCarbonPopup OnToggleCarbonPopup;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnToggleCarbonPopup?.Invoke());
    }

    private void Update()
    {
        float carbonValue = CarbonFootprint.CarbonCount;

        if (_buttonText != null)
        {
            if (carbonValue >= 1000f)
                _buttonText.text = $"{carbonValue / 1000f:F1}kg";
            else
                _buttonText.text = $"{carbonValue:F1}g";
        }
    }
}
