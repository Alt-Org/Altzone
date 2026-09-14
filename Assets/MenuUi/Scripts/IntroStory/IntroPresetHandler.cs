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

    public void SetPresetData(CharacterClassType classType, Action<bool> callback)
    {
        _backgroundStoneImage.sprite = ClassReference.Instance.GetCharacter(classType);
        _className.text = ClassReference.Instance.GetName(classType);
        AvatarData data= new(AvatarReference.Instance.GetDefaultAvatar(classType));
        AvatarVisualData visualData= AvatarDesignLoader.Instance.CreateAvatarVisualData(data);
        _faceloader.UpdateVisuals(visualData);
        _toggle.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                _selectedSprite.SetActive(true);
                callback(true);
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
