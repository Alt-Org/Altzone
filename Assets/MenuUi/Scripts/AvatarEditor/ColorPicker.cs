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

    private void Start()
    {
        _colorApplyButton.onClick.AddListener(() => AddListener(Color.red));
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
