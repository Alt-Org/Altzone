using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private bool _useOwnAvatarVisuals = true;

    private void OnEnable()
    {
        if (_useOwnAvatarVisuals)
        {
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
       
        _characterHandle.SetHeadColor(_avatarVisuals.Color);

        if (_avatarVisuals.Sprites == null || _avatarVisuals.Sprites.Count == 0)
            return;

        _characterHandle.SetMainCharacterImage(AvatarPiece.Hair, _avatarVisuals.Hair);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, _avatarVisuals.Eyes);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Nose, _avatarVisuals.Nose);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, _avatarVisuals.Mouth);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, _avatarVisuals.Clothes);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hands, _avatarVisuals.Hands);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Feet, _avatarVisuals.Feet);
    }

    public void UpdateVisuals(AvatarVisualData data)
    {
       
        _characterHandle.SetHeadColor(data.Color);

        _characterHandle.SetMainCharacterImage(AvatarPiece.Hair, data.Hair);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, data.Eyes);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Nose, data.Nose);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, data.Mouth);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, data.Clothes);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Hands, data.Hands);
        _characterHandle.SetMainCharacterImage(AvatarPiece.Feet, data.Feet);
    }
}

