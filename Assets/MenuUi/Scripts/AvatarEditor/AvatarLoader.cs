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
        if (_avatarVisuals.color != null)
            _characterHandle.SetHeadColor(_avatarVisuals.color);

        if (_avatarVisuals.sprites == null || _avatarVisuals.sprites.Count == 0)
            return;

        _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, _avatarVisuals.Hair);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, _avatarVisuals.Eyes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, _avatarVisuals.Nose);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, _avatarVisuals.Mouth);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Body, _avatarVisuals.Clothes);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Hands, _avatarVisuals.Hands);
        _characterHandle.SetMainCharacterImage(FeatureSlot.Feet, _avatarVisuals.Feet);
    }
}
