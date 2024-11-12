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
    public Vector2 spacing;

    public float oneRowHeight;
    private int previousRowCount = -2;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        //Calculating current rows 
        rows = CalculateRowsAmount();
        //Updating rectTransform(parent) size
        if (rows != previousRowCount)
        {
            previousRowCount = rows;
            float newHeight = oneRowHeight * rows;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
        }
        Debug.Log(rows);

        //fitX = true;
        //fitY = false;

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float availableWidth = parentWidth - padding.left - padding.right - (spacing.x * (columns - 1));  //((spacing.x / columns) * (columns - 1));
        float availableHeight = parentHeight - padding.top - padding.bottom - (spacing.y * (rows - 1));  //((spacing.y / rows) * (rows - 1));

        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

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
