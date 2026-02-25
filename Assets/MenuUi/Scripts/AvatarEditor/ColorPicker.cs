using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MenuUi.Scripts.AvatarEditor;
using Assets.Altzone.Scripts.Model.Poco.Player;
using TMPro;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private Button _colorApplyButton;
    [SerializeField] private Button _colorPickerButton;
    [SerializeField] private ScrollBarFeatureLoader _featureLoader;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private AvatarEditorController _controller;
    [SerializeField] private Button _clickAreaButton;
    [SerializeField] private Button _popupConfirmButton;
    [SerializeField] private Button _popupCancelButton;
    [SerializeField] private GameObject _colorPickerPopup;
    [SerializeField] private Image _previewColor;
    [SerializeField] private Image _colorPickerColorButtonColorImage;
    [SerializeField] private ColorGetter _colorCircle;

    private Color _color = Color.white;
    private Color _previousColor = Color.white;

    private void Start()
    {
        _colorApplyButton.onClick.AddListener(() => AddListener(_color));
        //_clickArea.SetActive(false);
        _popupConfirmButton.onClick.AddListener(() => ClosePopup());
        _popupCancelButton.onClick.AddListener(() => CancelChanges());
        _clickAreaButton.onClick.AddListener(() => ClosePopup());
        _colorPickerButton.onClick.AddListener(() => OpenPopup());
    }

    private void OnEnable()
    {
        _colorCircle.OnColorChanged += HandleColorChanged;
    }

    private void OnDisable()
    {
        _colorCircle.OnColorChanged -= HandleColorChanged;
    }

    private void HandleColorChanged(Color color)
    {
        UpdateColor(color);
    }

    public void SetActive(bool setActive)
    {
        _colorApplyButton.gameObject.SetActive(setActive);
        _colorPickerButton.gameObject.SetActive(setActive);
    }

    private void ClosePopup()
    {
        _clickAreaButton.gameObject.SetActive(false);
        _colorPickerPopup.SetActive(false);
    }

    private void OpenPopup()
    {
        _clickAreaButton.gameObject.SetActive(true);
        _colorPickerPopup.SetActive(true);
        _previousColor = _color;
    }

    private void CancelChanges()
    {
        ClosePopup();
        UpdateColor(_previousColor);
    }

    private void UpdateColor(Color color)
    {
        _color = color;

        _previewColor.color = _color;
        _colorPickerColorButtonColorImage.color = _color;
    }

    private void AddListener(Color color)
    {
        AvatarPiece? slot = _featureLoader.CurrentCategory;

        if (slot == null)
        {
            return;
        }

        AvatarPiece actualSlot = (AvatarPiece)slot;

        _characterHandle.SetPartColor(actualSlot, color);

        _controller.PlayerAvatar.SetPartColor(actualSlot, ColorUtility.ToHtmlStringRGBA(color));
    }
}
