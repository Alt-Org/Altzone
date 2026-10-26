using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class SoulHomeHoverButtonHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform _confirmHoverButton; // Placed above the selected furniture.
        [SerializeField] private RectTransform _topButtonsGroup; // Moved instead of the confirm button, so the buttons laid out next to it move with it.
        [SerializeField] private RectTransform _cancelHoverButton; // Placed left of the selected furniture.
        [SerializeField] private float _hoverButtonMargin = 10f;
        [SerializeField] private float _maxDistanceFromCorner = 150f; // Max distance of the confirm and cancel buttons from the box's top left corner, so the line between them doesn't stretch too much.
        [SerializeField] private RectTransform _hoverButtonLine;
        [SerializeField] private Vector2 _hoverButtonLineStart = new(0.07f, 0f);
        [SerializeField] private Vector2 _hoverButtonLineEnd = new(1f, 0.94f);
        public RectTransform _screen; // Given by MainScreenController

        public void SetButtonPositions(Camera _camera, BoxCollider2D box, Transform furniture)
        {
            Vector2 half = box.size * 0.5f;
            Vector2 corner = EdgeScreenPoint(new(-half.x, half.y));
            PlaceHoverButton(_confirmHoverButton, ClampToCorner(EdgeScreenPoint(new(0f, half.y))), Vector2.up, _topButtonsGroup);
            PlaceHoverButton(_cancelHoverButton, ClampToCorner(EdgeScreenPoint(new(-half.x, 0f))), Vector2.left);
            PlaceHoverButtonLine();

            Vector2 EdgeScreenPoint(Vector2 edge) => ViewportToScreenPoint(_camera.WorldToViewportPoint(furniture.TransformPoint(box.offset + edge)));
            Vector2 ClampToCorner(Vector2 point) => corner + Vector2.ClampMagnitude(point - corner, _maxDistanceFromCorner);
        }

        // The tower camera renders onto the Screen, so a viewport point maps to the same relative spot on its rect.
        private Vector2 ViewportToScreenPoint(Vector3 viewportPoint)
        {
            Rect rect = _screen.rect;
            return new(rect.xMin + rect.width * viewportPoint.x, rect.yMin + rect.height * viewportPoint.y);
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

        private void PlaceHoverButton(RectTransform button, Vector2 localPosition, Vector2 direction, RectTransform group = null)
        {
            if (button == null) return;

            // Move the button out by half of its size plus the margin, so its edge sits next to the furniture's edge.
            Vector3 halfSize = _screen.InverseTransformVector(button.TransformVector(button.rect.size * 0.5f));
            float offset = Mathf.Abs(direction.x * halfSize.x) + Mathf.Abs(direction.y * halfSize.y) + _hoverButtonMargin;
            localPosition += direction * offset;

            Vector3 position = _screen.TransformPoint(localPosition);
            if (group == null)
            {
                button.position = position;
                return;
            }

            // Shift the group by how far the button is from its spot, so the button lands there no matter where the
            // layout puts it inside the group.
            LayoutRebuilder.ForceRebuildLayoutImmediate(group);
            group.position += position - button.position;
        }

        private static Vector3 GetEdgeWorldPoint(RectTransform rectTransform, Vector2 direction)
        {
            Rect rect = rectTransform.rect;
            return rectTransform.TransformPoint(rect.center + Vector2.Scale(rect.size * 0.5f, direction));
        }
    }
}
