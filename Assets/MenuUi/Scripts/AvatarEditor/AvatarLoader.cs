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

    private void UpdateVisuals()
    {
       
        _characterHandle.SetHeadColor(_avatarVisuals.Color);

        if (_avatarVisuals.Sprites == null || _avatarVisuals.Sprites.Count == 0)
            return;

        _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, _avatarVisuals.Hair);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, _avatarVisuals.Eyes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, _avatarVisuals.Nose);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, _avatarVisuals.Mouth);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Body, _avatarVisuals.Clothes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Hands, _avatarVisuals.Hands);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Feet, _avatarVisuals.Feet);
    }

    public void UpdateVisuals(AvatarVisualData data)
    {
       
        _characterHandle.SetHeadColor(data.Color);

        _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, data.Hair);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, data.Eyes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, data.Nose);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, data.Mouth);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Body, data.Clothes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Hands, data.Hands);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Feet, data.Feet);
    }
}

