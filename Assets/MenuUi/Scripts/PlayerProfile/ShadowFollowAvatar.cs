using UnityEngine;

public class ShadowFollowAvatar : MonoBehaviour
{
    [SerializeField] private RectTransform _avatarRect;
    [SerializeField] private RectTransform _shadowRect;
    [SerializeField] private float _offsetY = 10f;

    void LateUpdate()
    {
        if (_avatarRect == null || _shadowRect == null) return;

        // Avatarin alareunan y-koordinaatti suoraan lokaalissa koordinaatistossa
        float avatarBottomY = _avatarRect.anchoredPosition.y
                            - (_avatarRect.rect.height * _avatarRect.pivot.y);

        _shadowRect.anchoredPosition = new Vector2(0, avatarBottomY + _offsetY);
    }
}
