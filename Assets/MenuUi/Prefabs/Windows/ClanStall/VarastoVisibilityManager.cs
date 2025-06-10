using System.Collections.Generic;
using UnityEngine;

public class VarastoVisibilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject kojuContentWindow;
    [SerializeField] private List<GameObject> uiElementsToHide;

    private bool kojuWindowStatus = false;

    private void OnEnable()
    {
        KojuContentNotifier.OnActiveStateChanged += HandleKojuActiveChanged;

        // Initialize UI state based on current active state of kojuContentWindow
        if (kojuContentWindow != null)
        {
            kojuWindowStatus = kojuContentWindow.activeSelf;
            ToggleUIElements(!kojuWindowStatus);
        }
    }

    private void OnDisable()
    {
        KojuContentNotifier.OnActiveStateChanged -= HandleKojuActiveChanged;

        // Optionally reset UI elements to visible when this manager is disabled
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
