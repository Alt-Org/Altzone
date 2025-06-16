using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui joystick drag input functionality.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BattleUiJoystickComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _handleRectTransform;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _joystickRadius = Mathf.Min(_rectTransform.rect.width, _rectTransform.rect.height) * 0.5f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 joystickPos = _rectTransform.position;
            Vector2 dragVector = Vector2.ClampMagnitude(eventData.position - joystickPos, _joystickRadius);

            _handleRectTransform.position = joystickPos + dragVector;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _handleRectTransform.localPosition = Vector3.zero;
        }

        private float _joystickRadius;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}
