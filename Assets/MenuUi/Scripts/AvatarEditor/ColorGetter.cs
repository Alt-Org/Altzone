using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorGetter : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform _colorCircle;
    [SerializeField] private RectTransform _colorCircleHandle;
    [SerializeField] private RectTransform _gradientSquare;
    [SerializeField] private RectTransform _gradientSquareHandle;
    [SerializeField] private Image _gradientSquareImage;

    public System.Action<Color> OnColorChanged;

    private float _currentHue = 0;
    private float _currentSat = 1;
    private float _currentVal = 1;

    // Modifies the position of the circle pointer, bigger = closer to outwards edge, smaller = closer to inwards edge
    private const float CirclePointerPosition = 0.43f;

    // Enum is for pointers to move when expected and not at the same time
    private enum SelectionMode { None, Square, Circle }
    private SelectionMode _currentMode = SelectionMode.None;

    private void Start()
    {
        float squareWidth = _gradientSquare.rect.width;
        float squareHeight = _gradientSquare.rect.height;
        // Set the gradient pointer to top left (=white)
        _gradientSquareHandle.localPosition = new Vector2(-squareWidth / 2f, squareHeight / 2f);

        float startAngle = 90f;
        float rads = startAngle * Mathf.Deg2Rad;
        float circleRadius = _colorCircle.rect.width * CirclePointerPosition;

        // Set the start position of the circle pointer, doesn't matter where it is since gradient is set to white
        _colorCircleHandle.localPosition = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * circleRadius;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_colorCircle, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        // distance from click position to the center of the circle
        float distance = localPoint.magnitude;

        // the radius of the "hole" inside the color circle, 0.75 means the hole radius is 75% of the circle radius
        float innerRadius = (_colorCircle.rect.width * 0.5f) * 0.75f;

        // if click landed outside the "hole" (where the square is)
        if (distance > innerRadius)
        {
            _currentMode = SelectionMode.Circle;
            UpdateHue(eventData);
        }
        // click on gradient square
        else
        {
            _currentMode = SelectionMode.Square;
            UpdateSV(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _currentMode = SelectionMode.None;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_currentMode == SelectionMode.Circle)
        {
            UpdateHue(eventData);
        }
        else if (_currentMode == SelectionMode.Square)
        {
            UpdateSV(eventData);
        }
    }

    private void ApplyHue(Vector2 localPoint)
    {
        // Calculate angle between center of the circle and where you clicked for some reason 0 degrees is to the right.
        float visualAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

        // make angle positive, for example -90 degrees becomes 270 degrees
        if (visualAngle < 0)
        {
            visualAngle += 360f;
        }

        // Subtract 90 degrees from the visual angle, otherwise the picked color is the color from 90 degrees left of the pointer position
        float colorAngle = (visualAngle - 90f) % 360f;
        if (colorAngle < 0)
        {
            colorAngle += 360f;
        }

        // normalize degrees to 0-1
        _currentHue = colorAngle / 360f;

        // keep pointer on the ring when dragging outside of it
        float radius = _colorCircle.rect.width * CirclePointerPosition;
        float rads = visualAngle * Mathf.Deg2Rad;
        _colorCircleHandle.localPosition = new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * radius;

        // update the center square color based on the selected color on the wheel
        _gradientSquareImage.color = Color.HSVToRGB(_currentHue, 1, 1);

        NotifyColorChange();
    }

    private void UpdateHue(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_colorCircle, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        ApplyHue(localPoint);
    }

    private void UpdateSV(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_gradientSquare, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        float width = _gradientSquare.rect.width;
        float height = _gradientSquare.rect.height;

        // prevent localPoint from going outside the square
        localPoint.x = Mathf.Clamp(localPoint.x, -width / 2, width / 2);
        localPoint.y = Mathf.Clamp(localPoint.y, -height / 2, height / 2);

        _gradientSquareHandle.localPosition = localPoint;

        // normalize the saturation and black value to 0-1
        _currentSat = (localPoint.x + width / 2) / width;
        _currentVal = (localPoint.y + height / 2) / height;

        NotifyColorChange();
    }

    private void NotifyColorChange()
    {
        Color finalColor = Color.HSVToRGB(_currentHue, _currentSat, _currentVal);
        OnColorChanged?.Invoke(finalColor);
    }
}

