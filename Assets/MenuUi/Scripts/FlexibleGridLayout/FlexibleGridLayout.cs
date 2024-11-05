using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    //Made using this tutorial: https://www.youtube.com/watch?v=CGsEJToeXmA&ab_channel=GameDevGuide
    public enum FitType
    {
        Uniform, // Based on sqr root of the grid
        Width,
        Height,
        //FixedRows,
        //FixedColumns
    }
    public FitType fitType;

    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;

    //public bool fixX;
    //public bool fixY;

    private float _parentWidth;
    private float _parentHeight;


    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float sqrRoot = Mathf.Sqrt(transform.childCount); //Calculationg how many rows and colums grid have; based on childrens amount; "cube"like (4x4, or 3x3, ect.)
        rows = Mathf.CeilToInt(sqrRoot);
        columns = Mathf.CeilToInt(sqrRoot);

        if(fitType == FitType.Width)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);
        }
        if(fitType == FitType.Height)
        {
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);
        }

        _parentWidth = rectTransform.rect.width;
        _parentHeight = rectTransform.rect.height;

        float cellWidth = (_parentWidth / columns) - ((spacing.x/(float)columns)*2) - (padding.left/(float)columns) - (padding.right/(float)columns);
        float cellHeight = (_parentHeight / rows) - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

         
        int columnCount = 0;
        int rowCount = 0;
        for(int i = 0; i < rectChildren.Count; i++)
        {
            columnCount = i % columns;
            rowCount = i / columns;

            var item = rectChildren[i];

            float xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left; //"Offset" on x
            float yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top; //"Offset" on y

            SetChildAlongAxis(item, 0, xPos, cellSize.x); //Horizontal aligment
            SetChildAlongAxis(item, 1, yPos, cellSize.y); //Vertical aligment


        }
    }
    public override void CalculateLayoutInputVertical() => throw new System.NotImplementedException();
    public override void SetLayoutHorizontal() => throw new System.NotImplementedException();
    public override void SetLayoutVertical() => throw new System.NotImplementedException();
}
