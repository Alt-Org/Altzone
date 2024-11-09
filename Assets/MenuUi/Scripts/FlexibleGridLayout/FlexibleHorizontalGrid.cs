using UnityEngine;
using UnityEngine.UI;

public class FlexibleHorizontalGrid : LayoutGroup
{
    [Header("Flexible Horizontal Grid")]

    private int rows;
    private int columns = 4;
    public Vector2 cellSize;
    public Vector2 spacing;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

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

        rows = CalculateRowsAmount();
        Debug.Log(rows);
        //fitX = true;
        //fitY = false;

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float availableWidth = parentWidth - padding.left - padding.right - (spacing.x * (columns - 1));  //((spacing.x / columns) * (columns - 1));
        float availableHeight = parentHeight - padding.top - padding.bottom - (spacing.y * (rows - 1));  //((spacing.y / rows) * (rows - 1));

        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        //cellSize.x = fitX ? cellWidth : cellSize.x;
        //cellSize.y = fitY ? cellHeight : cellSize.y;
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
        if (rectChildren.Count <= 6)
            return 1;

        else if (rectChildren.Count > 6 && rectChildren.Count <= 12)
            return 2;

        else
        {
            Debug.LogWarning("Number of children cannot be greater than 12");
            return Mathf.CeilToInt(rectChildren.Count / columns);
        }
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
