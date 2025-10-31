using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarFaceLoader : MonoBehaviour
    {
        [Header("Avatar Configuration")]
        [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
        [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
        [SerializeField] private bool _useOwnAvatarVisuals = true;

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (_useOwnAvatarVisuals && _avatarVisuals != null)
            {
                UpdateVisuals();
                SubscribeToEvents();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnValidate()
        {
            ValidateReferences();
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            AvatarDesignLoader.OnAvatarDesignUpdate += UpdateVisuals;
        }

        private void UnsubscribeFromEvents()
        {
            AvatarDesignLoader.OnAvatarDesignUpdate -= UpdateVisuals;
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            if (!ValidateComponents())
                return;

            UpdateCharacterColor(_avatarVisuals.Color);
            UpdateFacialFeatures();
        }

        public void UpdateVisuals(AvatarVisualData data)
        {
            if (!ValidateComponents() || data == null)
            {
                Debug.LogWarning("Cannot update visuals: missing components or data is null.");
                return;
            }

            UpdateCharacterColor(data.Color);
            UpdateFacialFeatures(data);
        }

        private void UpdateCharacterColor(Color color)
        {
            _characterHandle.SetHeadColor(color);
        }

        private void UpdateFacialFeatures()
        {
            if (!HasValidSprites())
                return;

            var featureUpdates = new[]
            {
                (FeatureSlot.Hair, _avatarVisuals.Hair),
                (FeatureSlot.Eyes, _avatarVisuals.Eyes),
                (FeatureSlot.Nose, _avatarVisuals.Nose),
                (FeatureSlot.Mouth, _avatarVisuals.Mouth)
            };

            ApplyFeatureUpdates(featureUpdates);
        }

        private void UpdateFacialFeatures(AvatarVisualData data)
        {
            var featureUpdates = new[]
            {
                (FeatureSlot.Hair, data.Hair),
                (FeatureSlot.Eyes, data.Eyes),
                (FeatureSlot.Nose, data.Nose),
                (FeatureSlot.Mouth, data.Mouth)
            };

            ApplyFeatureUpdates(featureUpdates);
        }

        private void ApplyFeatureUpdates((FeatureSlot slot, Sprite sprite)[] featureUpdates)
        {
            foreach (var (slot, sprite) in featureUpdates)
            {
                try
                {
                    _characterHandle.SetMainCharacterImage(slot, sprite);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to set {slot} feature: {ex.Message}");
                }
            }
        }

        #endregion

        #region Validation

        private bool ValidateComponents()
        {
            if (_characterHandle == null)
            {
                Debug.LogError($"Character handle is not assigned in {gameObject.name}");
                return false;
            }

            if (_useOwnAvatarVisuals && _avatarVisuals == null)
            {
                Debug.LogError($"Avatar visuals ScriptableObject is not assigned in {gameObject.name}");
                return false;
            }

            return true;
        }

        private bool HasValidSprites()
        {
            if (_avatarVisuals.Sprites == null || _avatarVisuals.Sprites.Count == 0)
            {
                Debug.LogWarning($"No sprites available in avatar visuals for {gameObject.name}");
                return false;
            }

            return true;
        }

        private void ValidateReferences()
        {
            if (_characterHandle == null)
            {
                Debug.LogWarning($"Character handle is not assigned in {gameObject.name}");
            }

            if (_useOwnAvatarVisuals && _avatarVisuals == null)
            {
                Debug.LogWarning($"Avatar visuals ScriptableObject is not assigned in {gameObject.name}");
            }
        }

        #endregion

        #region Public Methods

        public void ForceUpdateVisuals()
        {
            if (_avatarVisuals != null)
            {
                UpdateVisuals();
            }
        }

        public void SetUseOwnAvatarVisuals(bool useOwn)
        {
            bool wasUsing = _useOwnAvatarVisuals;
            _useOwnAvatarVisuals = useOwn;

            if (useOwn && !wasUsing && _avatarVisuals != null)
            {
                UpdateVisuals();
            }

            if (gameObject.activeInHierarchy)
            {
                UnsubscribeFromEvents();
                if (_useOwnAvatarVisuals)
                {
                    SubscribeToEvents();
                }
            }
        }

        #endregion
    }
}

