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

        public bool LockYAxis = false;

        public delegate void JoystickInputHandler(Vector2 input);
        public event JoystickInputHandler OnJoystickInput;

        public delegate void JoystickXAxisInputHandler(float input);
        public event JoystickXAxisInputHandler OnJoystickXAxisInput;

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

            if (LockYAxis) OnJoystickXAxisInput(0);
            else OnJoystickInput(Vector2.zero);
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

            if (LockYAxis) dragVector.y = 0;

            _handleRectTransform.position = joystickPos + dragVector;

            Vector2 inputVector = dragVector / _joystickRadius;
            if (LockYAxis) OnJoystickXAxisInput(inputVector.x);
            else OnJoystickInput(inputVector);
        }
    }
}
