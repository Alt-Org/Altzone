using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.AvatarEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroPresetHandler : MonoBehaviour
{
    [SerializeField] private AvatarFaceLoader _faceloader;
    [SerializeField] private Image _backgroundStoneImage;
    [SerializeField] private TMP_Text _className;
    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameObject _selectedSprite;
    private CharacterClassType _classType = CharacterClassType.None;

    public CharacterClassType ClassType => _classType;

    public void SetPresetData(CharacterClassType classType, Action<CharacterClassType> callback)
    {
        _classType = classType;
        _backgroundStoneImage.sprite = AvatarReference.Instance.GetCharacterSprite(classType);
        _className.text = AvatarReference.Instance.GetName(classType);
        _className.color = AvatarReference.Instance.GetColour(classType);
        AvatarData data= new(AvatarReference.Instance.GetDefaultAvatar(classType));
        AvatarVisualData visualData= AvatarDesignLoader.Instance.CreateAvatarVisualData(data);
        _faceloader.UpdateVisuals(visualData);
        _toggle.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                _selectedSprite.SetActive(true);
                callback(classType);
            }
            else
            {
                _selectedSprite.SetActive(false);
            }
        });
    }

    public void Toggle(bool value)
    {
        _toggle.SetIsOnWithoutNotify(value);
        _selectedSprite.SetActive(value);
    }
}
