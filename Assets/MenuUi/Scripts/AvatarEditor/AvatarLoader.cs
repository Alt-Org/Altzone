using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private bool _useOwnAvatarVisuals = true;
    [SerializeField] private AvatarPartsReference _avatarPartsReference;
    private ServerAvatar _serverAvatar;

    private AvatarPartInfo GetPartInfo(AvatarPiece piece)
    {
        return piece switch
        {
            AvatarPiece.Hair => _avatarPartsReference.GetAvatarPartById(_serverAvatar.hair.ToString()),
            AvatarPiece.Eyes => _avatarPartsReference.GetAvatarPartById(_serverAvatar.eyes.ToString()),
            AvatarPiece.Nose => _avatarPartsReference.GetAvatarPartById(_serverAvatar.nose.ToString()),
            AvatarPiece.Mouth => _avatarPartsReference.GetAvatarPartById(_serverAvatar.mouth.ToString()),
            AvatarPiece.Clothes => _avatarPartsReference.GetAvatarPartById(_serverAvatar.clothes.ToString()),
            AvatarPiece.Feet => _avatarPartsReference.GetAvatarPartById(_serverAvatar.feet.ToString()),
            AvatarPiece.Hands => _avatarPartsReference.GetAvatarPartById(_serverAvatar.hands.ToString()),
            _ => null
        };
    }

    private void Start()
    {
        _serverAvatar = ServerManager.Instance.Player.avatar;
    }

    private void OnEnable()
    {
        if (_useOwnAvatarVisuals)
        {
            UpdateVisuals();
        }
    }

    private void GetAvatarPartInfoFromServerAvatar(ServerAvatar avatar)
    {

    }

    private void UpdateVisuals()
    {

        _characterHandle.SetHeadColor(_avatarVisuals.Color);

        if (_avatarVisuals.Sprites == null || _avatarVisuals.Sprites.Count == 0)
            return;

        _characterHandle.SetMainCharacterImage(AvatarPiece.Hair, GetPartInfo(AvatarPiece.Hair));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, GetPartInfo(AvatarPiece.Eyes));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Nose, GetPartInfo(AvatarPiece.Nose));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, GetPartInfo(AvatarPiece.Mouth));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, GetPartInfo(AvatarPiece.Clothes));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hands, GetPartInfo(AvatarPiece.Hands));
        _characterHandle.SetMainCharacterImage(AvatarPiece.Feet, GetPartInfo(AvatarPiece.Feet));
    }

    // FIX LATER
    public void UpdateVisuals(AvatarVisualData data)
    {
        //_characterHandle.SetHeadColor(data.Color);

        //_characterHandle.SetMainCharacterImage(AvatarPiece.Hair, data.Hair);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, data.Eyes);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Nose, data.Nose);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, data.Mouth);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, data.Clothes);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Hands, data.Hands);
        //_characterHandle.SetMainCharacterImage(AvatarPiece.Feet, data.Feet);
    }
}

