using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CarbonButtonHandler : MonoBehaviour
{
    private Button _button;

    public delegate void ToggleCarbonPopup();
    public static event ToggleCarbonPopup OnToggleCarbonPopup;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => OnToggleCarbonPopup?.Invoke());
    }
}
