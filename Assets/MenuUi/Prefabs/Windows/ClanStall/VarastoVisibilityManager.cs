using System.Collections.Generic;
using UnityEngine;

public class VarastoVisibilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject kojuContentWindow;
    [SerializeField] private List<GameObject> uiElementsToHide;

    private bool kojuWindowStatus = false;

    // When notified, toggle UI elements off
    private void OnEnable()
    {
        KojuContentNotifier.OnActiveStateChanged += HandleKojuActiveChanged;
        if (kojuContentWindow != null)
        {
            kojuWindowStatus = kojuContentWindow.activeSelf;
            ToggleUIElements(!kojuWindowStatus);
        }
    }

    // When notified, toggle UI elements on
    private void OnDisable()
    {
        KojuContentNotifier.OnActiveStateChanged -= HandleKojuActiveChanged;
        ToggleUIElements(true);
    }

    private void HandleKojuActiveChanged(bool isActive)
    {
        if (isActive != kojuWindowStatus)
        {
            ToggleUIElements(!isActive);
            kojuWindowStatus = isActive;
        }
    }

    private void ToggleUIElements(bool show)
    {
        foreach (var uiElement in uiElementsToHide)
        {
            if (uiElement != null)
                uiElement.SetActive(show);
        }
    }
}
