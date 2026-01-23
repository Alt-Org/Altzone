using Altzone.Scripts.AvatarPartsInfo;
using Assets.Altzone.Scripts.Model.Poco.Player;
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

            UpdateCharacterColor(_avatarVisuals.SkinColor, _avatarVisuals.ClassColor);
            UpdateFacialFeatures();
        }

        public void UpdateVisuals(AvatarVisualData data)
        {
            if (!ValidateComponents() || data == null)
            {
                Debug.LogWarning("Cannot update visuals: missing components or data is null.");
                return;
            }

            UpdateCharacterColor(data.SkinColor, data.ClassColor);
            UpdateFacialFeatures(data);
        }

        private void UpdateCharacterColor(Color skinColor, Color classColor)
        {
            _characterHandle.SetClassColor(/*classColor*/Color.blue);
            _characterHandle.SetSkinColor(skinColor);
        }

        private void UpdateFacialFeatures()
        {
            if (!HasValidSprites())
                return;

            var featureUpdates = new[]
            {
                // Color.red for testing update later
                (AvatarPiece.Hair, _avatarVisuals.Hair, Color.red),
                (AvatarPiece.Eyes, _avatarVisuals.Eyes, _avatarVisuals.EyesColor),
                (AvatarPiece.Nose, _avatarVisuals.Nose, _avatarVisuals.NoseColor),
                (AvatarPiece.Mouth, _avatarVisuals.Mouth, _avatarVisuals.MouthColor)
            };

            ApplyFeatureUpdates(featureUpdates);
        }

        private void UpdateFacialFeatures(AvatarVisualData data)
        {
            var featureUpdates = new[]
            {
                (AvatarPiece.Hair, data.Hair, data.HairColor),
                (AvatarPiece.Eyes, data.Eyes, data.EyesColor),
                (AvatarPiece.Nose, data.Nose, data.NoseColor),
                (AvatarPiece.Mouth, data.Mouth, data.MouthColor)
            };

            ApplyFeatureUpdates(featureUpdates);
        }

        private void ApplyFeatureUpdates((AvatarPiece slot, AvatarPartInfo partInfo, Color color)[] featureUpdates)
        {
            foreach (var (slot, partInfo, color) in featureUpdates)
            {
                try
                {
                    _characterHandle.SetMainCharacterImage(slot, partInfo, color);
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
            if (_avatarVisuals.PartInfos == null || _avatarVisuals.PartInfos.Count == 0)
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

