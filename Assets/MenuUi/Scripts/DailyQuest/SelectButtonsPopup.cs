using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButtonsPopup : MonoBehaviour
{
    private Button[] _selectedButtons = new Button[3];

    [SerializeField] private GameObject _popupWindow;
    [SerializeField] private Button _expandButton;
    [SerializeField] private Image[] _buttonImages = new Image[3];
    [Space]
    [SerializeField] private GameObject _expandedPopupWindow;
    [SerializeField] private Button[] _popupButtons = new Button[3];
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _confirmButton;

    public static event Action OnConfirm;

    private void OnEnable()
    {
        DailyTaskSelectButtons.OnButtonSelected += HandlePopup;

        if (_expandButton != null)
        {
            _expandButton.onClick.AddListener(ExpandPopup);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(ClosePopup);
        }

        if (_confirmButton != null)
        {
            _confirmButton.onClick.AddListener(ConfirmSelection);
            UpdateConfirmButton();
        }
    }

    private void OnDisable()
    {
        DailyTaskSelectButtons.OnButtonSelected -= HandlePopup;
    }

    private void HandlePopup(DailyTaskSelectButtons.SelectButtonObject button)
    {
        int emptyIndex = -1;
        for (int i = 0; i < _selectedButtons.Length; i++)
        {
            if (_selectedButtons[i] == null)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex == -1) return;

        _selectedButtons[emptyIndex] = button.Button;
        Button popupButton = _popupButtons[emptyIndex];

        Image sourceImage = button.Image;
        Image expandedPopupImage = popupButton.GetComponent<Image>();
        Image popupImage = _buttonImages[emptyIndex];

        if (sourceImage != null && expandedPopupImage != null && popupImage != null)
        {
            expandedPopupImage.sprite = sourceImage.sprite;
            expandedPopupImage.preserveAspect = true;

            popupImage.sprite = sourceImage.sprite;
            popupImage.preserveAspect = true;
        }

        popupButton.gameObject.SetActive(true);
        popupImage.gameObject.SetActive(true);

        popupButton.onClick.RemoveAllListeners();
        popupButton.onClick.AddListener(() =>
        {
            RemoveSelectedButton(emptyIndex);
        });

        _popupWindow.SetActive(true);

        UpdateConfirmButton();
    }

    private void RemoveSelectedButton(int index)
    {
        _selectedButtons[index] = null;

        Button popupButton = _popupButtons[index];
        Image expandedPopupImage = popupButton.GetComponent<Image>();
        Image popupImage = _buttonImages[index].GetComponent<Image>();

        if (expandedPopupImage != null && popupImage != null)
        {
            expandedPopupImage.sprite = null;
            popupImage.sprite = null;
        }

        popupButton.gameObject.SetActive(false);
        popupButton.onClick.RemoveAllListeners();
        popupImage.gameObject.SetActive(false);

        UpdateConfirmButton();
    }

    private void ExpandPopup()
    {
        _expandedPopupWindow.SetActive(true);
    }

    private void ClosePopup()
    {
        _expandedPopupWindow.SetActive(false);
    }

    private void ConfirmSelection()
    {
        OnConfirm?.Invoke();

        _expandedPopupWindow.SetActive(false);
        _popupWindow.SetActive(false);

        for (int i = 0; i < _selectedButtons.Length; i++)
        {
            RemoveSelectedButton(i);
        }
    }

    private void UpdateConfirmButton()
    {
        if (_confirmButton != null)
        {
            bool slotsFull = true;
            for (int i = 0; i < _selectedButtons.Length; i++)
            {
                if (_selectedButtons[i] == null)
                {
                    slotsFull = false;
                    break;
                }
            }
            _confirmButton.interactable = slotsFull;
        }
    }
}
