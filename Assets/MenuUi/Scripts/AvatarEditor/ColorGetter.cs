using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorGetter : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Image _colorCircle;
    [SerializeField] private RectTransform _pointer;

    public System.Action<Color> OnColorChanged;
    private Texture2D _colorCircleTex;

    private void Awake()
    {
        _colorCircleTex = _colorCircle.sprite.texture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PickColor(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        PickColor(eventData);
    }

    private void PickColor(PointerEventData eventData)
    {
        RectTransform rectTransform = _colorCircle.rectTransform;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            return;
        }

        float radius = (rectTransform.rect.width * 0.5f) * 0.99f;

        if (localPoint.magnitude > radius)
        {
            localPoint = localPoint.normalized * radius;
        }

        _pointer.localPosition = localPoint;

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float x = (localPoint.x + width * 0.5f) / width;
        float y = (localPoint.y + height * 0.5f) / height;

        int texX = Mathf.Clamp(Mathf.RoundToInt(x * _colorCircleTex.width), 0, _colorCircleTex.width - 1);
        int texY = Mathf.Clamp(Mathf.RoundToInt(y * _colorCircleTex.height), 0, _colorCircleTex.height - 1);

        Color pickedColor = _colorCircleTex.GetPixel(texX, texY);

        OnColorChanged?.Invoke(pickedColor);
    }
}
