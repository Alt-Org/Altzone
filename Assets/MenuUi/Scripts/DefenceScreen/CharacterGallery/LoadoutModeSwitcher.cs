using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadoutModeSwitcher : MonoBehaviour
{
    [Header("Button roots")]
    [SerializeField] private GameObject popupButtonsRoot;  
    [SerializeField] private GameObject inlineButtonsRoot;  

    [Header("Mode selector buttons")]
    [SerializeField] private Button popupModeButton;  
    [SerializeField] private Button inlineModeButton;

    public static event System.Action OnInlineModeShown;

    //true = popup mode active
    //false = inline mode active

    private bool _isPopupMode = true;

    private void Awake()
    {
        if (popupModeButton != null)
            popupModeButton.onClick.AddListener(ShowPopupMode);

        if (inlineModeButton != null)
            inlineModeButton.onClick.AddListener(ShowInlineMode);

        //default mode
        ShowPopupMode();
    }

    /// <summary>
    /// Activates popup loadout UI and updates mode button visuals
    /// </summary>
    private void ShowPopupMode()
    {
        _isPopupMode = true;

        if (popupButtonsRoot != null)
            popupButtonsRoot.SetActive(true);

        if (inlineButtonsRoot != null)
            inlineButtonsRoot.SetActive(false);

        RefreshModeButtons();

      
    }

    /// <summary>
    /// Activates inline loadout UI and updates mode button visuals
    /// </summary>
    private void ShowInlineMode()
    {
        _isPopupMode = false;
        if (popupButtonsRoot != null)
            popupButtonsRoot.SetActive(false);

        if (inlineButtonsRoot != null)
            inlineButtonsRoot.SetActive(true);

        RefreshModeButtons();

        OnInlineModeShown?.Invoke();
    }

    /// <summary>
    /// Updates mode selector button states so that the active mode button looks "selected"
    /// </summary>
    private void RefreshModeButtons()
    {
        if (popupModeButton != null)
            popupModeButton.interactable = !_isPopupMode;

        if (inlineModeButton != null)
            inlineModeButton.interactable = _isPopupMode;
    }
}
