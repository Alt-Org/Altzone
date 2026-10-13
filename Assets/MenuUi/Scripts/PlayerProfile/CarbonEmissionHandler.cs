using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarbonEmissionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _closeButton;

    private void Start()
    {
        ClosePopup();
        CarbonButtonHandler.OnToggleCarbonPopup += OpenPopup;
        _closeButton.onClick.AddListener(ClosePopup);
    }

    private void OnDisable()
    {
        ClosePopup();
    }

    private void OnDestroy()
    {
        CarbonButtonHandler.OnToggleCarbonPopup -= OpenPopup;
    }

    private void OpenPopup()
    {
        _panel.SetActive(true);
    }

    private void ClosePopup()
    {
        _panel.SetActive(false);
    }
}
