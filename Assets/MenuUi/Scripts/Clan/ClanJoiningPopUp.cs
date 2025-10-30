using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ClanJoiningPopUp : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _bodyText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private UnityAction _onConfirm;
    private UnityAction _onCancel;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(UnityAction onConfirm, UnityAction onCancel = null,
        string bodyText = "Olet liittymässä uuteen klaaniin. Jatketaanko?")
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        _bodyText.text = bodyText;

        SetButtons(true);
        HookButtons();

        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        //setbuttons(false);
        UnhookButtons();
        //_canvasgroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void HookButtons()
    {
        if (_confirmButton)
        {
            _confirmButton.onClick.AddListener(() =>
            {
                SetButtons(false);
                _onConfirm?.Invoke();
                Hide();
            });
        }

        if (_cancelButton)
        {
            _cancelButton.onClick.AddListener(() =>
            {
                SetButtons(false);
                _onCancel?.Invoke();
                Hide();
            });
        }
    }

    private void UnhookButtons()
    {
        if (_confirmButton)
        {
            _confirmButton.onClick.RemoveAllListeners();
        }
        if (_cancelButton)
        {
            _cancelButton.onClick.RemoveAllListeners();
        }
    }

    private void SetButtons(bool interactable)
    {
        if (_confirmButton)
        {
            _confirmButton.interactable = interactable;
        }
        if (_cancelButton)
        {
            _cancelButton.interactable = interactable;
        }
    }
}
