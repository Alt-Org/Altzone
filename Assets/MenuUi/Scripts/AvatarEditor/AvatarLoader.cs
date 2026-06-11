using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;
    [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
    [SerializeField] private bool _useOwnAvatarVisuals = true;
    [SerializeField] private AvatarPartsReference _avatarPartsReference;

    private void OnEnable()
    {
        if (_useOwnAvatarVisuals)
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
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Hair, _avatarVisuals.Hair, _avatarVisuals.HairColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, _avatarVisuals.Eyes, _avatarVisuals.EyesColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Nose, _avatarVisuals.Nose, _avatarVisuals.NoseColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, _avatarVisuals.Mouth, _avatarVisuals.MouthColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, _avatarVisuals.Clothes, _avatarVisuals.ClothesColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Hands, _avatarVisuals.Hands, _avatarVisuals.HandsColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Feet, _avatarVisuals.Feet, _avatarVisuals.FeetColor));
    }

    public void UpdateVisuals(AvatarVisualData data)
    {
        _characterHandle.SetSkinColor(data.SkinColor);
        _characterHandle.SetClassColor(data.ClassColor);
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Hair, data.Hair, data.HairColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Eyes, data.Eyes, data.EyesColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Nose, data.Nose, data.NoseColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Mouth, data.Mouth, data.MouthColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Clothes, data.Clothes, data.ClothesColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Hands, data.Hands, data.HandsColor));
        StartCoroutine(_characterHandle.SetMainCharacterImage(AvatarPiece.Feet, data.Feet, data.FeetColor));
    }
}

