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

    private void OnEnable()
    {
        if (_useOwnAvatarVisuals)
        {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {

        _characterHandle.SetSkinColor(_avatarVisuals.SkinColor);
        _characterHandle.SetClassColor(_avatarVisuals.ClassColor);

        if (_avatarVisuals.PartInfos == null || _avatarVisuals.PartInfos.Count == 0)
            return;
        // Color.red for testing update later
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hair, _avatarVisuals.Hair, Color.red);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, _avatarVisuals.Eyes, _avatarVisuals.EyesColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Nose, _avatarVisuals.Nose, _avatarVisuals.NoseColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, _avatarVisuals.Mouth, _avatarVisuals.MouthColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, _avatarVisuals.Clothes, _avatarVisuals.ClothesColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hands, _avatarVisuals.Hands, _avatarVisuals.HandsColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Feet, _avatarVisuals.Feet, _avatarVisuals.FeetColor);
    }

    public void UpdateVisuals(AvatarVisualData data)
    {
        _characterHandle.SetSkinColor(data.SkinColor);
        _characterHandle.SetClassColor(data.ClassColor);

        _characterHandle.SetMainCharacterImage(AvatarPiece.Hair, data.Hair, data.HairColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, data.Eyes, data.EyesColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Nose, data.Nose, data.NoseColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, data.Mouth, data.MouthColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, data.Clothes, data.ClothesColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hands, data.Hands, data.HandsColor);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Feet, data.Feet, data.FeetColor);
    }
}

