using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor{
    public class AvatarFaceLoader : MonoBehaviour
    {
        [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
        [SerializeField] private AvatarEditorCharacterHandle _characterHandle;

        private void OnEnable()
        {
            UpdateVisuals();
            AvatarDesignLoader.OnAvatarDesignUpdate += UpdateVisuals;
        }

        private void OnDisable()
        {
            AvatarDesignLoader.OnAvatarDesignUpdate -= UpdateVisuals;
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
        }
    }
}
