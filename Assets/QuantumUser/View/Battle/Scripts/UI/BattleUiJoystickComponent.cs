using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui joystick drag input functionality.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiJoystickComponent : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _handleRectTransform;

        public delegate void JoystickInputHandler(Vector2 input);
        public event JoystickInputHandler OnJoystickInput;

        public void OnPointerDown(PointerEventData eventData)
        {
            _joystickRadius = Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height) * 0.5f - JoystickRadiusOffset;
            HandleDrag(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            HandleDrag(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _handleRectTransform.localPosition = Vector3.zero;
            OnJoystickInput(Vector2.zero);
        }

        private const int JoystickRadiusOffset = 10;
        private float _joystickRadius;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void HandleDrag(Vector2 dragPos)
        {
            Vector2 joystickPos = _rectTransform.position;
            Vector2 dragVector = Vector2.ClampMagnitude(dragPos - joystickPos, _joystickRadius);

            _handleRectTransform.position = joystickPos + dragVector;
            OnJoystickInput(dragVector / _joystickRadius);
        }
    }
}
