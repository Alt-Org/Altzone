using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class SoulHomeHoverButtonHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform _confirmHoverButton; // Placed above the selected furniture.
        [SerializeField] private RectTransform _cancelHoverButton; // Placed left of the selected furniture.
        [SerializeField] private RectTransform _rotateHoverButton; // Placed below the selected furniture.
        [SerializeField] private float _hoverButtonMargin = 10f;
        [SerializeField] private RectTransform _hoverButtonLine;
        [SerializeField] private Vector2 _hoverButtonLineStart = new(0.07f, 0f);
        [SerializeField] private Vector2 _hoverButtonLineEnd = new(1f, 0.94f);
        public RectTransform _screen; // Given by MainScreenController

        public void SetButtonPositions(Camera _camera, BoxCollider2D box, Transform furniture)
        {
            Vector2 half = box.size * 0.5f;
            PlaceHoverButton(_confirmHoverButton, EdgeViewportPoint(new(0f, half.y)), Vector2.up);
            PlaceHoverButton(_cancelHoverButton, EdgeViewportPoint(new(-half.x, 0f)), Vector2.left);
            PlaceHoverButton(_rotateHoverButton, EdgeViewportPoint(new(0f, -half.y)), Vector2.down);
            PlaceHoverButtonLine();

            Vector3 EdgeViewportPoint(Vector2 edge) => _camera.WorldToViewportPoint(furniture.TransformPoint(box.offset + edge));
        }

        private void PlaceHoverButtonLine() // Line connecting the confirm and cancel buttons
        {
            Transform parent = _hoverButtonLine.parent;
            Vector2 start = parent.InverseTransformPoint(GetEdgeWorldPoint(_cancelHoverButton, Vector2.up));
            Vector2 end = parent.InverseTransformPoint(GetEdgeWorldPoint(_confirmHoverButton, Vector2.left));
            Vector2 delta = end - start;
            Vector2 imageSpan = _hoverButtonLineEnd - _hoverButtonLineStart;

            bool fits = delta.x > 0f && delta.y > 0f && imageSpan.x > 0f && imageSpan.y > 0f;
            _hoverButtonLine.gameObject.SetActive(fits);
            if (!fits) return;

            // With the pivot at the line's start, the pivot is placed on the start point and the size scales the
            // part of the image between the two ends to cover the distance between the buttons.
            _hoverButtonLine.anchorMin = _hoverButtonLine.anchorMax = new Vector2(0.5f, 0.5f);
            _hoverButtonLine.pivot = _hoverButtonLineStart;
            _hoverButtonLine.sizeDelta = new Vector2(delta.x / imageSpan.x, delta.y / imageSpan.y);
            _hoverButtonLine.position = parent.TransformPoint(start);
        }

        private void PlaceHoverButton(RectTransform button, Vector3 viewportPoint, Vector2 direction)
        {
             if (button == null) return;

             // The tower camera renders onto the Screen, so a viewport point maps to the same relative spot on its rect.
            Rect rect = _screen.rect;
            Vector2 localPosition = new(rect.xMin + rect.width * viewportPoint.x, rect.yMin + rect.height * viewportPoint.y);

            // Move the button out by half of its size plus the margin, so its edge sits next to the furniture's edge.
            Vector3 halfSize = _screen.InverseTransformVector(button.TransformVector(button.rect.size * 0.5f));
            float offset = Mathf.Abs(direction.x * halfSize.x) + Mathf.Abs(direction.y * halfSize.y) + _hoverButtonMargin;
            localPosition += direction * offset;

            button.position = _screen.TransformPoint(localPosition);
        }

        private static Vector3 GetEdgeWorldPoint(RectTransform rectTransform, Vector2 direction)
        {
            Rect rect = rectTransform.rect;
            return rectTransform.TransformPoint(rect.center + Vector2.Scale(rect.size * 0.5f, direction));
        }
    }
}
