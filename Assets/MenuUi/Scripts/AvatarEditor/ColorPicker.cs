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
    [SerializeField] private GameObject _bottomMenu;

    private Color _color = Color.red;
    private Color _previousColor = Color.red;

    private void Start()
    {
        _colorApplyButton.onClick.AddListener(() => SaveColor());
        //_clickArea.SetActive(false);
        _popupConfirmButton.onClick.AddListener(() => ConfirmChanges());
        _popupCancelButton.onClick.AddListener(() => CancelChanges());
        _clickAreaButton.onClick.AddListener(() => CancelChanges());
        _colorPickerButton.onClick.AddListener(() => OpenPopup());
        _previewColor.color = Color.red;
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
        _bottomMenu.SetActive(true);
    }

    private void OpenPopup()
    {
        _clickAreaButton.gameObject.SetActive(true);
        _colorPickerPopup.SetActive(true);
        _bottomMenu.SetActive(false);

        AvatarPiece? activeSlot = _featureLoader.CurrentCategory;

        if (activeSlot.HasValue)
        {
            ColorUtility.TryParseHtmlString(_controller.PlayerAvatar.GetPartColor(activeSlot.Value), out _previousColor);
        }
    }

    private void CancelChanges()
    {
        ClosePopup();
        UpdateColor(_previousColor);
    }

    private void ConfirmChanges()
    {
        SaveColor();
        ClosePopup();
    }

    private void SaveColor()
    {
        AvatarPiece? activeSlot = _featureLoader.CurrentCategory;

        if (activeSlot.HasValue)
        {
            _characterHandle.SetPartColor(activeSlot.Value, _color);
            _controller.PlayerAvatar.SetPartColor(activeSlot.Value, ColorUtility.ToHtmlStringRGBA(_color));
            _colorPickerColorButtonColorImage.color = _color;
        }
    }

    private void UpdateColor(Color color)
    {
        AvatarPiece? activeSlot = _featureLoader.CurrentCategory;
        _color = color;

        _previewColor.color = _color;

        if (activeSlot.HasValue)
        {
            _characterHandle.SetPartColor(activeSlot.Value, color);
        }
    }
}
