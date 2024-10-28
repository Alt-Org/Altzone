using UnityEngine;

[ExecuteInEditMode]
public class ChildAnchorSetter : MonoBehaviour
{
    private void OnEnable()
    {
        Vector2 defaultSize = GetComponent<RectTransform>().rect.size;
        if (defaultSize.x == 0 || defaultSize.y == 0) return;

        foreach (Transform child in transform)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            // Get current position and size
            Vector2 position = rectTransform.localPosition;
            Vector2 size = rectTransform.rect.size;

            // Set anchors
            Vector2 anchorOffset = (position + defaultSize / 2) / defaultSize;
            Vector2 halfAnchoredSize = size / defaultSize / 2;
            rectTransform.anchorMin = anchorOffset - halfAnchoredSize;
            rectTransform.anchorMax = anchorOffset + halfAnchoredSize;

            // Reset position and size
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }
    }
}
