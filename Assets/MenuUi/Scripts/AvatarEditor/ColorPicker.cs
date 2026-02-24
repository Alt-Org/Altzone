using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MenuUi.Scripts.AvatarEditor;
using Assets.Altzone.Scripts.Model.Poco.Player;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private Button _colorApplyButton;
    [SerializeField] private Button _colorPickerButton;
    [SerializeField] private ScrollBarFeatureLoader _featureLoader;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private AvatarEditorController _controller;
    [SerializeField] private Button _clickAreaButton;
    [SerializeField] private Button _popupConfirmButton;
    [SerializeField] private GameObject _colorPickerPopup;
    [SerializeField] private Image _previewColor;
    [SerializeField] private Image _colorPickerColorButton;
    [SerializeField] private Image _sliderBackground;


    private void Start()
    {
        _colorApplyButton.onClick.AddListener(() => AddListener(Color.red));
        //_clickArea.SetActive(false);
        _popupConfirmButton.onClick.AddListener(() => ClosePopup());
        _clickAreaButton.onClick.AddListener(() => ClosePopup());
    }

    private void ClosePopup()
    {
        _clickAreaButton.gameObject.SetActive(false);
        _colorPickerPopup.SetActive(false);
    }

    private void UpdateColor(Color color)
    {
        _previewColor.color = color;
        _colorPickerColorButton.color = color;
        _sliderBackground.color = color;
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
