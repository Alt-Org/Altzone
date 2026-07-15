using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private bool _useOwnAvatarVisuals = true;
    [SerializeField] private AvatarPartsReference _avatarPartsReference;

    (AvatarPiece slot, AvatarPartInfo partInfo, Color color)[] _pendingFeatureUpdates = null;

    private void OnEnable()
    {
        if (_useOwnAvatarVisuals && _avatarVisuals != null)
        {
            UpdateVisuals();
            SubscribeToEvents();
        }

        if (_pendingFeatureUpdates != null)
        {
            ApplyFeatureUpdates(_pendingFeatureUpdates);
            if (gameObject.activeInHierarchy) _pendingFeatureUpdates = null;
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

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        AvatarDesignLoader.OnAvatarDesignUpdate += UpdateVisuals;
    }

    private void UnsubscribeFromEvents()
    {
        AvatarDesignLoader.OnAvatarDesignUpdate -= UpdateVisuals;
    }

    private void UpdateVisuals()
    {

        _characterHandle.SetSkinColor(_avatarVisuals.SkinColor);
        _characterHandle.SetClassColor(_avatarVisuals.ClassColor);

        if (_avatarVisuals.PartInfos == null || _avatarVisuals.PartInfos.Count == 0)
            return;

        var featureUpdates = new[]
            {
                (AvatarPiece.Hair, _avatarVisuals.Hair, _avatarVisuals.HairColor),
                (AvatarPiece.Eyes, _avatarVisuals.Eyes, _avatarVisuals.EyesColor),
                (AvatarPiece.Nose, _avatarVisuals.Nose, _avatarVisuals.NoseColor),
                (AvatarPiece.Mouth, _avatarVisuals.Mouth, _avatarVisuals.MouthColor),
                (AvatarPiece.Clothes, _avatarVisuals.Clothes, _avatarVisuals.ClothesColor),
                (AvatarPiece.Hands, _avatarVisuals.Hands, _avatarVisuals.HandsColor),
                (AvatarPiece.Feet, _avatarVisuals.Feet, _avatarVisuals.FeetColor)
            };

        ApplyFeatureUpdates(featureUpdates);
    }

    public void UpdateVisuals(AvatarVisualData data)
    {
        _characterHandle.SetSkinColor(data.SkinColor);
        _characterHandle.SetClassColor(data.ClassColor);

        var featureUpdates = new[]
            {
                (AvatarPiece.Hair, data.Hair, data.HairColor),
                (AvatarPiece.Eyes, data.Eyes, data.EyesColor),
                (AvatarPiece.Nose, data.Nose, data.NoseColor),
                (AvatarPiece.Mouth, data.Mouth, data.MouthColor),
                (AvatarPiece.Clothes, data.Clothes, data.ClothesColor),
                (AvatarPiece.Hands, data.Hands, data.HandsColor),
                (AvatarPiece.Feet, data.Feet, data.FeetColor)
            };

        ApplyFeatureUpdates(featureUpdates);
    }

    private void ApplyFeatureUpdates((AvatarPiece slot, AvatarPartInfo partInfo, Color color)[] featureUpdates)
    {
        if (!gameObject.activeInHierarchy)
        {
            _pendingFeatureUpdates = featureUpdates;
            return;
        }
        foreach (var (slot, partInfo, color) in featureUpdates)
        {
            try
            {
                StartCoroutine(_characterHandle.SetMainCharacterImage(slot, partInfo, color));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to set {slot} feature: {ex.Message}");
            }
        }
    }

}

