using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        DynamicColumns,
        DynamicRows,
        FixedColumns,
        FixedRows,
    }

    public enum CellSizeType
    {
        Manual,
        AspectRatio,
        BasedOnPrefab
    }


    const float ShortestAspectRatio = 4.0f / 3.0f;
    const float TallestAspectRatio = 22.0f / 9.0f;


    [Header("Flexible Grid")]

    [SerializeField, Tooltip("How the grid will fit its children.\n\nDynamic: Determine column amount based on screen's aspect ratio.\nFixed Columns: Column amount stays the same.\nFixed Rows: Row amount stays the same.")]
    private FitType _gridFit = FitType.DynamicColumns;

    [SerializeField, Min(1)]
    private int _minDynamicColumns = 2;

    [SerializeField, Min(1)]
    private int _maxDynamicColumns = 4;

    [SerializeField, Min(1)]
    private int _minDynamicRows = 2;

    [SerializeField, Min(1)]
    private int _maxDynamicRows = 4;

    [SerializeField, Min(1)]
    private int _columnAmount = 1;

    [SerializeField, Min(1)]
    private int _rowAmount = 1;

    [SerializeField, Tooltip("How the grid's cell size is determined.\n\nManual: Give cell size values manually.\nAspect Ratio: Cell size is automatically calculated to fit this aspect ratio.\nBased On Prefab: Cell size is set automatically based on the prefab's RectTransform component.")]
    private CellSizeType _gridCellSize = CellSizeType.Manual;

    [SerializeField, Min(0)]
    private Vector2 _minCellSize;

    [SerializeField, Min(0)]
    private Vector2 _maxCellSize;

    [SerializeField, Min(0)]
    private Vector2 _preferredCellSize;

    [SerializeField, Min(0)]
    private float _cellAspectRatio = 1;

    [SerializeField]
    private GameObject _prefab;

    [SerializeField, Tooltip("The spacing between grid cells."), Min(0)]
    private Vector2 _cellSpacing;

    private int _rows;
    private int _columns;
    private Vector2 _cellSize;


    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        // Calculating column and row amount
        switch (_gridFit)
        {
            case FitType.DynamicColumns:
                if (_gridCellSize != CellSizeType.Manual) // if cell size is given manually columns and rows are calculated later based on cell size
                {
                    _columns = CalculateDynamicColumns(_minDynamicColumns, _maxDynamicColumns);
                    _rows = GetRowsBasedOnColumns(_columns);
                }
                break;

            case FitType.DynamicRows:
                if (_gridCellSize != CellSizeType.Manual)
                {
                    _rows = CalculateDynamicRows(_minDynamicRows, _maxDynamicRows);
                    _columns = GetColumnsBasedOnRows(_rows);
                }
                break;

            case FitType.FixedColumns:
                _columns = _columnAmount;
                _rows = GetRowsBasedOnColumns(_columns);
                break;

            case FitType.FixedRows:
                _rows = _rowAmount;
                _columns = GetColumnsBasedOnRows(_rows);
                break;
        }

        // Scaling grid cells
        float cellWidth = 0;
        float cellHeight = 0;

        switch (_gridCellSize)
        {
            case CellSizeType.Manual:

                if (_gridFit == FitType.DynamicColumns)
                {
                    // Calculating how many columns fit with preferred size and how many pixels are left over
                    int preferredSizeColumns = CalculateColumnFit(_preferredCellSize.x);
                    float leftoverPixels = rectTransform.rect.width - (_preferredCellSize.x * preferredSizeColumns);

                    // If there is space for more than half a cell we shrink cells and add one more column
                    bool shrinkCells = false;
                    if (_preferredCellSize.x / 2 < leftoverPixels - _cellSpacing.x)
                    {
                        float shrunkCellWidth = CalculateMaxCellWidth(preferredSizeColumns + 1);

                        if (shrunkCellWidth >= _minCellSize.x)
                        {
                            shrinkCells = true;
                            _columns = preferredSizeColumns + 1;
                            cellWidth = shrunkCellWidth;
                            cellHeight = cellWidth / GetCellAspectRatio(_preferredCellSize);
                        }
                    }

                    // If we don't shrink we grow cells if there are leftover pixels
                    if (!shrinkCells)
                    {
                        _columns = preferredSizeColumns;

                        if (leftoverPixels > 0)
                        {
                            float grownCellWidth = CalculateMaxCellWidth(_columns);
                            grownCellWidth = Mathf.Clamp(grownCellWidth, 0, _maxCellSize.x);

                            cellWidth = grownCellWidth;
                            cellHeight = cellWidth / GetCellAspectRatio(_preferredCellSize);
                        }
                        else
                        {
                            cellWidth = _preferredCellSize.x;
                            cellHeight = _preferredCellSize.y;
                        }
                    }
                    
                    _rows = GetRowsBasedOnColumns(_columns);
                }

                else if (_gridFit == FitType.DynamicRows)
                {
                    // Calculating how many rows fit with preferred size and how many pixels are left over
                    int preferredSizeRows = CalculateRowFit(_preferredCellSize.y);
                    float leftoverPixels = rectTransform.rect.height - (_preferredCellSize.y * preferredSizeRows);

                    // If there is space for more than half a cell we shrink cells and add one more row
                    bool shrinkCells = false;
                    if (_preferredCellSize.y / 2 < leftoverPixels - _cellSpacing.y)
                    {
                        float shrunkCellHeight = CalculateMaxCellHeight(_rows + 1);

                        if (shrunkCellHeight >= _minCellSize.y)
                        {
                            shrinkCells = true;
                            _rows = preferredSizeRows + 1;
                            cellHeight = shrunkCellHeight;
                            cellWidth = GetCellAspectRatio(_preferredCellSize) * cellHeight;
                        }
                    }

                    // If we don't shrink we grow cells if there are leftover pixels
                    if (!shrinkCells)
                    {
                        _rows = preferredSizeRows;

                        if (leftoverPixels > 0)
                        {
                            float grownCellHeight = CalculateMaxCellHeight(_rows);
                            grownCellHeight = Mathf.Clamp(grownCellHeight, 0, _maxCellSize.y);

                            cellHeight = grownCellHeight;
                            cellWidth = GetCellAspectRatio(_preferredCellSize) * cellHeight;
                        }
                        else
                        {
                            cellWidth = _preferredCellSize.x;
                            cellHeight = _preferredCellSize.y;
                        }
                    }

                    _columns = GetColumnsBasedOnRows(_rows);
                }

                else if (_gridFit == FitType.FixedColumns)
                {
                    // Get widest possible cell and clamp it to max cell width
                    cellWidth = CalculateMaxCellWidth(_columns);
                    cellWidth = Mathf.Clamp(cellWidth, 0, _maxCellSize.x);

                    // Calculate height for the cell
                    cellHeight = cellWidth / GetCellAspectRatio(_maxCellSize);
                }

                else if (_gridFit == FitType.FixedRows)
                {
                    // Get tallest possible cell and clamp it to max cell height
                    cellHeight = CalculateMaxCellHeight(_rows);
                    cellHeight = Mathf.Clamp(cellHeight, 0, _maxCellSize.y);

                    // Calculate width for the cell
                    cellWidth = GetCellAspectRatio(_maxCellSize) * cellHeight;
                }
                break;

            case CellSizeType.AspectRatio: // Calculating cell size from aspect ratio value

                if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns)
                {
                    cellWidth = CalculateMaxCellWidth(_columns);
                    cellHeight = cellWidth / _cellAspectRatio;
                }
                else if (_gridFit == FitType.DynamicRows || _gridFit == FitType.FixedRows)
                {
                    cellHeight = CalculateMaxCellHeight(_rows);
                    cellWidth = _cellAspectRatio * cellHeight;
                }
                break;

            case CellSizeType.BasedOnPrefab: // Getting cell width and height from prefab
                if (_prefab != null)
                {
                    RectTransform prefabRect = _prefab.GetComponent<RectTransform>();
                    if (prefabRect != null)
                    {
                        cellWidth = prefabRect.rect.width;
                        cellHeight = prefabRect.rect.height;
                    }
                }
                break;
        }

        _cellSize.x = cellWidth;
        _cellSize.y = cellHeight;

        // Placing children
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int rowCount;
            int columnCount;
            if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns) // Vertical
            {
                rowCount = i / _columns;
                columnCount = i % _columns;
            }
            else // Horizontal
            {
                rowCount = i % _rows;
                columnCount = i / _rows;
            }
            

            var item = rectChildren[i];

            float xPos = (_cellSize.x * columnCount) + (_cellSpacing.x * columnCount) + padding.left;
            float yPos = (_cellSize.y * rowCount) + (_cellSpacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, _cellSize.x);
            SetChildAlongAxis(item, 1, yPos, _cellSize.y);
        }

        // Scaling the rectTransform
        if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns) // Vertical
        {
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = new Vector2(0, 1);

            rectTransform.sizeDelta = new Vector2(0, (_rows * _cellSize.y) + ((_rows - 1) * _cellSpacing.y));
        }
        else if (_gridFit == FitType.DynamicRows || _gridFit == FitType.FixedRows) // Horizontal
        {
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchorMin = Vector2.zero;

            rectTransform.sizeDelta = new Vector2((_columns * _cellSize.x) + ((_columns - 1) * _cellSpacing.x), 0);
        }
    }


    private int CalculateDynamicColumns(int minColumns, int maxColumns)
    {
        // Get the percentage at which point the current aspect ratio sits between shortest and tallest aspect ratios shortest = 1 tallest = 0
        float aspectRatioPercentage = (GetScreenAspectRatio() - ShortestAspectRatio) / (TallestAspectRatio - ShortestAspectRatio);

        // Getting column amount between min and max amount of columns based on the aspect ratio percentage
        return Mathf.RoundToInt(maxColumns + (minColumns - maxColumns) * aspectRatioPercentage);
    }


    private int CalculateDynamicRows(int minRows, int maxRows)
    {
        // Get the percentage at which point the current aspect ratio sits between tallest and shortest aspect ratios shortest = 0 tallest = 1
        float aspectRatioPercentage = (GetScreenAspectRatio() - TallestAspectRatio) / (ShortestAspectRatio - TallestAspectRatio);

        // Getting row amount between min and max amount of rows based on the aspect ratio percentage
        return Mathf.RoundToInt(maxRows + (minRows - maxRows) * aspectRatioPercentage);
    }


    private int CalculateColumnFit(float cellWidth) // calculating how many columns can fit based on cell width
    {
        return Mathf.FloorToInt((rectTransform.rect.width - padding.left - padding.right - _cellSpacing.x) / (cellWidth + _cellSpacing.x));
    }


    private int CalculateRowFit(float cellHeight) // calculating how many rows can fit based on cell height
    {
        return Mathf.FloorToInt((rectTransform.rect.height - padding.top - padding.bottom - _cellSpacing.y) / (cellHeight + _cellSpacing.y));
    }


    private float CalculateMaxCellWidth(int columns) // calculating maximum width for a cell based on column amount
    {
        return rectTransform.rect.width / (float)columns - ((_cellSpacing.x / (float)columns) * (columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
    }


    private float CalculateMaxCellHeight(int rows) // calculating maximum height for a cell based on row amount
    {
        return rectTransform.rect.height / (float)rows - ((_cellSpacing.y / (float)rows) * (rows - 1)) - (padding.top / (float)rows) - (padding.bottom / (float)rows);
    }


    private int GetColumnsBasedOnRows(int rows)
    {
        return Mathf.CeilToInt(transform.childCount / (float)rows);
    }


    private int GetRowsBasedOnColumns(int columns)
    {
        return Mathf.CeilToInt(transform.childCount / (float)columns);
    }


    private float GetCellAspectRatio(Vector2 cell)
    {
        return cell.x / cell.y;
    }


    private float GetScreenAspectRatio() // Get screen aspect ratio clamped between Shortest and Tallest aspect ratios
    {
        float screenAspectRatio = (float)Screen.currentResolution.height / Screen.currentResolution.width;
        return Mathf.Clamp(screenAspectRatio, ShortestAspectRatio, TallestAspectRatio);
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(FlexibleGridLayout))]
    public class FlexibleGridLayoutEditor : Editor
    {
        // LayoutGroup properties
        SerializedProperty _padding;
        SerializedProperty _childAlignment;

        // Grid fit properties
        SerializedProperty _gridFitSelection;
        SerializedProperty _gridFitMinColumns;
        SerializedProperty _gridFitMaxColumns;
        SerializedProperty _gridFitMinRows;
        SerializedProperty _gridFitMaxRows;
        SerializedProperty _gridFitColumns;
        SerializedProperty _gridFitRows;

        // Cell size properties
        SerializedProperty _cellSizeSelection;
        SerializedProperty _minCellSize;
        SerializedProperty _maxCellSize;
        SerializedProperty _preferredCellSize;
        SerializedProperty _cellAspectRatio;
        SerializedProperty _cellPrefab;

        // Cell spacing property
        SerializedProperty _cellSpacing;


        void OnEnable()
        {
            // Getting LayoutGroup properties
            _padding = serializedObject.FindProperty(nameof(m_Padding));
            _childAlignment = serializedObject.FindProperty(nameof(m_ChildAlignment));

            // Getting grid fit properties
            _gridFitSelection = serializedObject.FindProperty(nameof(_gridFit));
            _gridFitMinColumns = serializedObject.FindProperty(nameof(_minDynamicColumns));
            _gridFitMaxColumns = serializedObject.FindProperty(nameof(_maxDynamicColumns));
            _gridFitMinRows = serializedObject.FindProperty(nameof(_minDynamicRows));
            _gridFitMaxRows = serializedObject.FindProperty(nameof(_maxDynamicRows));
            _gridFitColumns = serializedObject.FindProperty(nameof(_columnAmount));
            _gridFitRows = serializedObject.FindProperty(nameof(_rowAmount));

            // Getting cell size properties
            _cellSizeSelection = serializedObject.FindProperty(nameof(_gridCellSize));
            _minCellSize = serializedObject.FindProperty(nameof(_minCellSize));
            _maxCellSize = serializedObject.FindProperty(nameof(_maxCellSize));
            _preferredCellSize = serializedObject.FindProperty(nameof(_preferredCellSize));
            _cellAspectRatio = serializedObject.FindProperty(nameof(_cellAspectRatio));
            _cellPrefab = serializedObject.FindProperty(nameof(_prefab));

            // Getting cell spacing property
            _cellSpacing = serializedObject.FindProperty(nameof(_cellSpacing));
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Place LayoutGroup property fields
            EditorGUILayout.PropertyField(_padding);
            EditorGUILayout.PropertyField(_childAlignment);

            // Place grid fit property fields
            EditorGUILayout.PropertyField(_gridFitSelection);

            switch ((FitType)_gridFitSelection.enumValueIndex)
            {
                case FitType.DynamicColumns:
                    // Manual cell size changes how dynamic columns and rows are calculated so we hide these options
                    if ((CellSizeType)_cellSizeSelection.enumValueIndex != CellSizeType.Manual)
                    {
                        EditorGUILayout.PropertyField(_gridFitMinColumns);
                        EditorGUILayout.PropertyField(_gridFitMaxColumns);
                        EditorGUILayout.Space();
                    }
                    break;
                case FitType.DynamicRows:
                    if ((CellSizeType)_cellSizeSelection.enumValueIndex != CellSizeType.Manual)
                    {
                        EditorGUILayout.PropertyField(_gridFitMinRows);
                        EditorGUILayout.PropertyField(_gridFitMaxRows);
                        EditorGUILayout.Space();
                    }
                    break;
                case FitType.FixedColumns:
                    EditorGUILayout.PropertyField(_gridFitColumns);
                    EditorGUILayout.Space();
                    break;
                case FitType.FixedRows:
                    EditorGUILayout.PropertyField(_gridFitRows);
                    EditorGUILayout.Space();
                    break;
            }

            // Place cell size property fields
            EditorGUILayout.PropertyField(_cellSizeSelection);

            switch ((CellSizeType)_cellSizeSelection.enumValueIndex)
            {
                case CellSizeType.Manual:
                    // In dynamic fit show all options, in fixed show only max size
                    if ((FitType)_gridFitSelection.enumValueIndex == FitType.DynamicColumns || (FitType)_gridFitSelection.enumValueIndex == FitType.DynamicRows)
                    {
                        EditorGUILayout.PropertyField(_minCellSize);
                        EditorGUILayout.PropertyField(_maxCellSize);
                        EditorGUILayout.PropertyField(_preferredCellSize);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(_maxCellSize);
                    }
                    EditorGUILayout.Space();
                    break;
                case CellSizeType.AspectRatio:
                    EditorGUILayout.PropertyField(_cellAspectRatio);
                    EditorGUILayout.Space();
                    break;
                case CellSizeType.BasedOnPrefab:
                    EditorGUILayout.PropertyField(_cellPrefab);
                    EditorGUILayout.Space();
                    break;
            }

            // Place cell spacing property field
            EditorGUILayout.PropertyField(_cellSpacing);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif


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
