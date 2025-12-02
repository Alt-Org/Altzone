using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Resizes the loadout buttons under this Content object so that
/// a fixed number of buttons (e.g. 5) fit inside the visible area.
/// Uses HorizontalLayoutGroup padding and spacing for the calculation.
/// </summary>
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class InlineLoadoutButtonSizer : MonoBehaviour
{
    [SerializeField] private RectTransform _viewport;       
    [SerializeField] private int _visibleButtons = 5;       
    private HorizontalLayoutGroup _layoutGroup;

    private void Awake()
    {
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();

        
        if (_viewport == null)
        {
            _viewport = transform.parent as RectTransform;
        }

        UpdateButtonSizes();
    }

    
    private void OnRectTransformDimensionsChange()
    {
        UpdateButtonSizes();
    }

    /// <summary>
    /// Calculates the size for each button so that the target number
    /// of buttons fits into the viewport width, accounting for padding and spacing.
    /// </summary>
    private void UpdateButtonSizes()
    {
        if (_viewport == null || _visibleButtons <= 0) return;

        float viewportWidth = _viewport.rect.width;

        float paddingLeft = _layoutGroup.padding.left;
        float paddingRight = _layoutGroup.padding.right;
        float spacing = _layoutGroup.spacing;

        
        float availableWidth = viewportWidth
                               - paddingLeft
                               - paddingRight
                               - spacing * (_visibleButtons - 1);

        if (availableWidth <= 0f) return;

        
        float buttonSize = availableWidth / _visibleButtons;

       
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform childRect = child as RectTransform;
            if (childRect == null) continue;

            childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
            childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonSize);
        }
    }
}
