using System.Collections.Generic;
using UnityEngine;

public class VarastoVisibilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject kojuContentWindow;
    [SerializeField] private List<GameObject> uiElementsToHide;

    [Header("Sorting UI Elements")]
    [SerializeField] private GameObject kojuSortingText;
    [SerializeField] private GameObject kojuSortButton;
    [SerializeField] private GameObject varastoSortingText;
    [SerializeField] private GameObject varastoSortButton;

    private bool kojuWindowStatus = false;

    // When KojuView is activated
    private void OnEnable()
    {
        KojuContentNotifier.OnActiveStateChanged += HandleKojuActiveChanged;
        if (kojuContentWindow != null)
        {
            kojuWindowStatus = kojuContentWindow.activeSelf;
            ToggleUIElements(!kojuWindowStatus);
            ToggleSortingUI(kojuWindowStatus);
        }
    }

    // When KojuView is deactivated
    private void OnDisable()
    {
        KojuContentNotifier.OnActiveStateChanged -= HandleKojuActiveChanged;
        ToggleUIElements(true);
        ToggleSortingUI(false);  
    }

    private void HandleKojuActiveChanged(bool isActive)
    {
        if (isActive != kojuWindowStatus)
        {
            ToggleUIElements(!isActive);
            ToggleSortingUI(isActive);
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

    private void ToggleSortingUI(bool isKojuActive)
    {
        if (kojuSortingText != null)
            kojuSortingText.SetActive(isKojuActive);
        if (kojuSortButton != null)
            kojuSortButton.SetActive(isKojuActive);

        if (varastoSortingText != null)
            varastoSortingText.SetActive(!isKojuActive);
        if (varastoSortButton != null)
            varastoSortButton.SetActive(!isKojuActive);
    }
}
