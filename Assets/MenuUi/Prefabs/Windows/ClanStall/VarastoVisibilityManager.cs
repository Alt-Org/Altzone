using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VarastoVisibilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject kojuContentWindow;
    [SerializeField] private List<GameObject> uiElementsToHide;

    private bool kojuStatus = false;

    private void Update()
    {
        if (kojuContentWindow == null) return;

        bool isKojuActive = kojuContentWindow.activeSelf;

        if (isKojuActive != kojuStatus)
        {
            ToggleUIElements(!isKojuActive); // Hide if Koju is active, show if not
            kojuStatus = isKojuActive;
        }
    }

    // Hides or shows the UI elements
    private void ToggleUIElements(bool show)
    {
        foreach (var uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(show);
            }
        }
    }
}
