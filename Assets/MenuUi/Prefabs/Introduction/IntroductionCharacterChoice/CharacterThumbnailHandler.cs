using System;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CharacterThumbnailHandler : MonoBehaviour
{
    [SerializeField] private Image _backgroundSprite;
    [SerializeField] private Image _characterSprite;
    [SerializeField] private Image _nameSprite;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private AvatarLoader _avatarLoader;
    [SerializeField] private Button _selectButton;

    public void SetData(CharacterClassType classType, Action<CharacterClassType> callback)
    {
        AvatarReference avatarreference = AvatarReference.Instance;

        _backgroundSprite.color = avatarreference.GetColour(classType); //change card color
        _characterSprite.sprite = avatarreference.GetCharacterSprite(classType); //set character sprite

        AvatarData data = new(avatarreference.GetDefaultAvatar(classType));
        AvatarVisualData visualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(data);
        _avatarLoader.UpdateVisuals(visualData); //set avatar design

        _nameText.text = avatarreference.GetName(classType); //set name sprite
        _nameText.color = avatarreference.GetColour(classType);

        _selectButton.onClick.AddListener(() => { callback?.Invoke(classType); });
    }
}
