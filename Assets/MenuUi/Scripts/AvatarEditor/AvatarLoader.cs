using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;

    private void OnEnable()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_avatarVisuals.colors != null && _avatarVisuals.colors.Count != 0)
            _characterHandle.SetHeadColor(_avatarVisuals.colors[0]);

        if (_avatarVisuals.sprites == null || _avatarVisuals.sprites.Count == 0)
            return;

        _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, _avatarVisuals.sprites[0]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, _avatarVisuals.sprites[1]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, _avatarVisuals.sprites[2]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, _avatarVisuals.sprites[3]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Body, _avatarVisuals.sprites[4]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Hands, _avatarVisuals.sprites[5]);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Feet, _avatarVisuals.sprites[6]);
    }
}
