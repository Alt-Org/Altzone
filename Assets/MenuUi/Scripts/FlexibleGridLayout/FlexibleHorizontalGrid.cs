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
    public int columns;

    public Vector2 cellSize;
    public float cellAspectRatio; //times that width takes from the height (f.e. aspect ratio = 1.5 => Height = 100, Width = 100 * 1.5 = 150)
    public Vector2 spacing;

    //public float oneRowHeight;
    private int previousRowCount = -2;

    public override void CalculateLayoutInputHorizontal()
    {
        ///Plan: Calculate size of cells based on width only
        /// Than calculate height based on width and aspect ratio.
        /// Calculate positions and parent size
        
        base.CalculateLayoutInputHorizontal();

        ////Calculating current rows 
        //Updating rectTransform(parent) size

        //Debug.Log(rows);

        ////fitX = true;
        ////fitY = false;

        //float parentHeight = rectTransform.rect.height;
        //float availableHeight = parentHeight - padding.top - padding.bottom - (spacing.y * (rows - 1));  //((spacing.y / rows) * (rows - 1));
        //float cellHeight = availableHeight / rows;

        float parentWidth = rectTransform.rect.width;
        float availableWidth = parentWidth - padding.left - padding.right - (spacing.x * (columns - 1));  //((spacing.x / columns) * (columns - 1));
        float cellWidth = availableWidth / columns;
        float cellHeight = cellWidth / cellAspectRatio;
        //Debug.Log("cellHeight  -- " + cellHeight);

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

        rows = CalculateRowsAmount();

        if (rows != previousRowCount)
        {
            previousRowCount = rows;
            float availableHeight = (cellHeight * rows) + padding.top + padding.bottom + (spacing.y * (rows - 1));  //((spacing.y / rows) * (rows - 1));
            Debug.Log("cellHeight * rows  -- " + cellHeight * rows);
            Debug.Log("(cellHeight * rows) + padding.top + padding.bottom -- " + (cellHeight * rows) + padding.top + padding.bottom);
            Debug.Log("(cellHeight * rows) + padding.top + padding.bottom + (spacing.y * (rows - 1)) -- " + (cellHeight * rows) + padding.top + padding.bottom + (spacing.y * (rows - 1)));
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, availableHeight);
        }

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
        Debug.Log("CalculateRowsAmount  --" + Mathf.CeilToInt((float)rectChildren.Count / columns));
        return Mathf.CeilToInt((float)rectChildren.Count / columns);
    }

    //if (fitType == FitType.WIDTH || fitType == FitType.HEIGHT || fitType == FitType.UNIFORM)
    //{
    //    float squareRoot = Mathf.Sqrt(transform.childCount);
    //    rows = columns = Mathf.CeilToInt(squareRoot);
    //    switch (fitType)
    //    {
    //        case FitType.WIDTH:
    //            fitX = true;
    //            fitY = false;
    //            break;
    //        case FitType.HEIGHT:
    //            fitX = false;
    //            fitY = true;
    //            break;
    //        case FitType.UNIFORM:
    //            fitX = fitY = true;
    //            break;
    //    }
    //}

    //if (fitType == FitType.WIDTH || fitType == FitType.FIXEDCOLUMNS)
    //{
    //    rows = Mathf.CeilToInt(transform.childCount / (float)columns);
    //    if (fitType == FitType.FIXEDCOLUMNS)
    //    {
    //        fitX = true;
    //    }
    //}
    //if (fitType == FitType.HEIGHT || fitType == FitType.FIXEDROWS)
    //{
    //    columns = Mathf.CeilToInt(transform.childCount / (float)rows);
    //}
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
