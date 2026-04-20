using UnityEngine;
using UnityEngine.UI;

public class ShadowFollowAvatar : MonoBehaviour
{
    [SerializeField] private RectTransform _avatarRect;
    [SerializeField] private RectTransform _shadowRect;
    //[SerializeField] private float _offsetY = 10f;
    [SerializeField] private Image _baseImage;

    void OnEnable()
    {
        if (_avatarRect == null || _shadowRect == null || _baseImage == null) return;

        Vector3 size =_baseImage.sprite.rect.size;

        float spriteAspect = size.x/size.y;
        float avatarBoxAspect = _avatarRect.rect.width/_avatarRect.rect.height;

        if(spriteAspect > avatarBoxAspect)
        {
            float extraSpace = _avatarRect.rect.height*(1-(avatarBoxAspect/spriteAspect));
            float anchorPos = (_avatarRect.rect.height-extraSpace)*0.17f;
            float position = (_avatarRect.rect.height/2-extraSpace/2-anchorPos)*-1;
            _shadowRect.anchoredPosition = new Vector2(0, position);
            _shadowRect.sizeDelta = new(_avatarRect.rect.width/2, _avatarRect.rect.width / 2);
        }
        else
        {
            float anchorPos = _avatarRect.rect.height  * 0.17f;
            float position = (_avatarRect.rect.height / 2 - anchorPos )* -1;
            _shadowRect.anchoredPosition = new Vector2(0, position);
            float extraSpace = _avatarRect.rect.width * (1 - (spriteAspect/avatarBoxAspect));
            _shadowRect.sizeDelta = new((_avatarRect.rect.width-extraSpace) / 2, (_avatarRect.rect.width - extraSpace) / 2);
        }


        // Avatarin alareunan y-koordinaatti suoraan lokaalissa koordinaatistossa
        /*float avatarBottomY = _avatarRect.anchoredPosition.y
                            - (_avatarRect.rect.height * _avatarRect.pivot.y);

        _shadowRect.anchoredPosition = new Vector2(0, avatarBottomY + _offsetY);*/
    }
}
