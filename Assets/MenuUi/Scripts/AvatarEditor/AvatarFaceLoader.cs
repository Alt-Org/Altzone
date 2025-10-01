using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor{
    public class AvatarFaceLoader : MonoBehaviour
    {
        [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
        [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
        [SerializeField] private bool _useOwnAvatarVisuals = true;

        private void OnEnable()
        {
            if (_useOwnAvatarVisuals)
            {
                UpdateVisuals();
                AvatarDesignLoader.OnAvatarDesignUpdate += UpdateVisuals;
            }
        }

        private void OnDisable()
        {
            AvatarDesignLoader.OnAvatarDesignUpdate -= UpdateVisuals;
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
        }

        public void UpdateVisuals(AvatarVisualData data)
        {
            if (data.color != null)
                _characterHandle.SetHeadColor(data.color);

            /*if (_avatarVisuals.sprites == null || _avatarVisuals.sprites.Count == 0)
                return;*/

            _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, data.Hair);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, data.Eyes);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, data.Nose);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, data.Mouth);
        }
    }
}
