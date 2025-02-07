using UnityEngine;
using UnityEngine.UI;

public class FlexibleHorizontalGrid : LayoutGroup
{

    [Header("Flexible Horizontal Grid")]
    public int columns = 4;
    public Vector2 spacing;
    public float cellAspectRatio = 1f; // Aspect ratio (width/height) for the cells

    private Vector2 cellSize;
    private int rows;
    private int _previousRowCount = -2;
    private bool _firstCalculation = true;

    private DrivenRectTransformTracker tracker; // Used for tracking RectTransform changes
    [SerializeField] private RectTransform _content;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float parentWidth = _content.rect.width;    
        //Debug.LogWarning("ContentRect"  + _content.rect.width);
        float availableWidth = parentWidth - padding.left - padding.right - (spacing.x * (columns - 1));
        float cellWidth = availableWidth / columns;
        float cellHeight = cellWidth / cellAspectRatio;

        rows = CalculateRowsAmount();
        if (rows != _previousRowCount)
        {
            _previousRowCount = rows;
            float availableHeight = (cellHeight * rows) + padding.top + padding.bottom + (spacing.y * (rows - 1));
            rectTransform.sizeDelta = new Vector2(parentWidth, availableHeight);
            //Debug.LogWarning($"New rectTransfrom of {name} is {new Vector2(parentWidth, availableHeight)} ");
        }

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;

            var item = rectChildren[i];
            float xPos = padding.left + (cellSize.x + spacing.x) * column;
            float yPos = padding.top + (cellSize.y + spacing.y) * row;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        // Not implemented as this layout is primarily horizontal-driven
    }

    public override void SetLayoutHorizontal()
    {
        // No additional logic required
    }

    public override void SetLayoutVertical()
    {
        // No additional logic required
    }

    private int CalculateRowsAmount()
    {
        return Mathf.CeilToInt((float)rectChildren.Count / columns);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tracker.Clear(); // Clear tracking when the component is disabled
    }
}

