
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ModalWindowPanel : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Transform _headerArea;
    [SerializeField] private TextMeshProUGUI _titleField;

    [Header("Content")]
    [SerializeField] private Transform _contentArea;
    [SerializeField] private Transform _verticalLayoutArea;
    [SerializeField] private Image _heroImage;
    [SerializeField] private TextMeshProUGUI _heroText;

    [Space()]

    [SerializeField] private Transform _horizontalLayoutArea;
    [SerializeField] private Transform _iconContainer;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _iconText;

    [Header("Footer")]
    [SerializeField] private Transform _footerArea;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _declineButton;
    [SerializeField] private Button _alternateButton;

    private Action onConfirmAction;
    private Action onDeclineAction;
    private Action onAlternateAction;


    public void Confirm()
    {
        onConfirmAction?.Invoke();
        Close();
    }

    public void Decline()
    {
        onDeclineAction?.Invoke();
        Close();
    }


    public void Alternate()
    {
        onAlternateAction?.Invoke();
        Close();
    }

    public void Close()
    {
        // Code to close the modal window
        gameObject.SetActive(false);
    }

    public void ShowAsHero(string title, Sprite imageToShow, string message, Action confirmAction, Action declineAction, Action alternateAction = null)
    {

        _horizontalLayoutArea.gameObject.SetActive(false);

        // Hide the header if there's no title
        bool hasTitle = string.IsNullOrEmpty(title);
        _headerArea.gameObject.SetActive(hasTitle);
        _titleField.text = title;

        _heroImage.sprite = imageToShow;
        _heroText.text = message;

        onConfirmAction = confirmAction;
        onDeclineAction = declineAction;

        bool hasAlternate = (alternateAction != null);
        _alternateButton.gameObject.SetActive(hasAlternate);
        onAlternateAction = alternateAction;
    }
}
