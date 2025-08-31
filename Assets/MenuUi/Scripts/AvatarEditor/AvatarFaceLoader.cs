using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
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
            // Päivitä väri
            _characterHandle.SetHeadColor(_avatarVisuals.Color);

            // Tarkista sprites-property
            if (_avatarVisuals.Sprites == null || _avatarVisuals.Sprites.Count == 0)
                return;

            _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, _avatarVisuals.Hair);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, _avatarVisuals.Eyes);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, _avatarVisuals.Nose);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, _avatarVisuals.Mouth);
        }

        public void UpdateVisuals(AvatarVisualData data)
        {
            // Käytä julkista Color-propertya
            _characterHandle.SetHeadColor(data.Color);

            _characterHandle.SetMainCharacterImage(FeatureSlot.Hair, data.Hair);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Eyes, data.Eyes);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Nose, data.Nose);
            _characterHandle.SetMainCharacterImage(FeatureSlot.Mouth, data.Mouth);
        }
    }
}

