using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutModeSwitcher : MonoBehaviour
{
    [Header("Button roots")]
    [SerializeField] private GameObject popupButtonsRoot;  
    [SerializeField] private GameObject inlineButtonsRoot;  

    [Header("Mode selector buttons")]
    [SerializeField] private Button popupModeButton;  
    [SerializeField] private Button inlineModeButton; 

    private void Awake()
    {
        if (popupModeButton != null)
            popupModeButton.onClick.AddListener(ShowPopupMode);

        if (inlineModeButton != null)
            inlineModeButton.onClick.AddListener(ShowInlineMode);

        ShowPopupMode();
    }

    private void ShowPopupMode()
    {
        if (popupButtonsRoot != null)
            popupButtonsRoot.SetActive(true);

        if (inlineButtonsRoot != null)
            inlineButtonsRoot.SetActive(false);
    }

    private void ShowInlineMode()
    {
        if (popupButtonsRoot != null)
            popupButtonsRoot.SetActive(false);

        if (inlineButtonsRoot != null)
            inlineButtonsRoot.SetActive(true);
    }
}
