using UnityEngine;
using UnityEngine.UI;

public class FlexibleHorizontalGrid : LayoutGroup
{
    /// Examples of grid:
    /// 
    ///  6 items            [] [] [] []
    ///                     [] []

    ///  12 items           [] [] [] []
    ///                     [] [] [] []
    ///                     [] [] [] [] 


    [Header("Flexible Horizontal Grid")]

    private int rows;
    public int columns = 4;

    public Vector2 cellSize;
    public float cellAspectRatio; //times that width takes from the height (f.e. aspect ratio = 1.5 => Height = 100, Width = 100 * 1.5 = 150)
    public Vector2 spacing;

    private int _previousRowCount = -2;
    private bool _firstCalculation = true; //For some reason the first calculation shows incorrect values for rectTransform.rect's. This bool intended to fix this problem

    public override void CalculateLayoutInputHorizontal()
    {
        /// Plan: Calculate size of cells based on width only
        /// If rows amount has been changed => chnage parent's size (rectTransform.sizeDelta). Then calculate the height based on the width and aspect ratio.
        /// Calculate Positions and 

        base.CalculateLayoutInputHorizontal();

        float parentWidth = rectTransform.rect.width;
        float availableWidth = rectTransform.rect.width - padding.left - padding.right - (spacing.x * (columns - 1));  //((spacing.x / columns) * (columns - 1));
        //Debug.Log("Available width: "+ availableWidth);
        float cellWidth = availableWidth / columns;
        float cellHeight = cellWidth / cellAspectRatio;
        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

        //Debug.Log(cellHeight + "--- Cell Height");
        //Debug.Log(cellSize.y + "--- cellSize y");

        if (!_firstCalculation)
        {
            rows = CalculateRowsAmount();
            if (rows != _previousRowCount)
            {
                _previousRowCount = rows;
                float availableHeight = (cellSize.y * (float)rows) + (float)padding.top + (float)padding.bottom + (spacing.y * ((float)rows - 1));  //((spacing.y / rows) * (rows - 1));
                //Debug.Log("Available Height: " + availableHeight);
                rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, availableHeight);
            }
        }
        _firstCalculation = false;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];

            var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }
    private int CalculateRowsAmount()
    {
        return Mathf.CeilToInt((float)rectChildren.Count / columns);
    }

    private int CalculateCellWidth()
    {
        return 0;
    }
    private int CalculateColumnsAmount()
    {
        return 0;
    }

    public override void CalculateLayoutInputVertical()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetLayoutHorizontal()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetLayoutVertical()
    {
        //throw new System.NotImplementedException();
    }
}
