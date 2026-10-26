using System.Collections;
using System.Collections.Generic;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarbonEmissionHandler : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _closeButton;

    private bool _closing = false;

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

    private void LateUpdate()
    {
        if (ClickStateHandler.GetClickState() is ClickState.Start)
        {
            if (_panel.activeSelf)
            {
                if (!CheckIfPanel()) _closing = true;
            }
        }
        if (ClickStateHandler.GetClickState() is ClickState.End && _closing)
        {
            if (!CheckIfPanel()) ClosePopup();
            _closing = false;
        }
    }

    private void OpenPopup()
    {
        _panel.SetActive(true);
    }

    private void ClosePopup()
    {
        _panel.SetActive(false);
    }

    private bool CheckIfPanel()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData data = new(EventSystem.current);
        data.position = ClickStateHandler.GetClickPosition();
        if (data.position == Vector2.negativeInfinity) return false;
        var modules = RaycasterManager.GetRaycasters();
        foreach (var module in modules)
        {
            module.Raycast(data, results);
        }
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == _panel) return true;
        }
        return false;
    }

}
