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

    public enum StartCorner
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight,
    }

    const float ShortestAspectRatio = 4.0f / 3.0f;
    const float TallestAspectRatio = 22.0f / 9.0f;

    [SerializeField]
    private StartCorner _startCorner;

    [SerializeField, Tooltip("How the grid will fit its children." +
        "\n\nDynamicColumns: Determine column amount based on screen's aspect ratio, or if cell size is Manual determine column amount based on cell size." +
        "\nDynamicRows: Determine row amount based on screen's aspect ratio, or if cell size is Manual determine row amount based on cell size." +
        "\nFixed Columns: Column amount stays the same." +
        "\nFixed Rows: Row amount stays the same.")]
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

    [SerializeField, Tooltip("How the grid's cell size is determined." +
        "\n\nManual: Give cell size values manually." +
        "\nAspect Ratio: Cell size is automatically calculated to fit this aspect ratio." +
        "\nBased On Prefab: Cell size is set based on the prefab's RectTransform component.")]
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

    [SerializeField, Tooltip("Prevent cells from scaling larger than the boundaries of this grid's parent rect transform. Useful for non-scrollable grids." +
        "\nNote: Overrides minimum and maximum cell size values if cell size is set to Manual.")]
    bool _preventScalingOutOfParentBounds = false;

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
                if (_gridCellSize != CellSizeType.Manual) // if cell size is given manually columns and rows are set later in CalculateChildSize function
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

        // Calculating cell size
        CalculateCellSize();

        // Scaling the rectTransform to fit cells
        Vector2 offset = CalculateChildAlignmentOffset();
        rectTransform.pivot = new Vector2(0, 1);

        if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns) // Vertical
        {
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = new Vector2(0, 1);

            rectTransform.sizeDelta = new Vector2(0, (_rows * _cellSize.y) + (Mathf.Clamp(_rows - 1, 0, float.MaxValue) * _cellSpacing.y) + padding.top + padding.bottom + offset.y);
        }
        else if (_gridFit == FitType.DynamicRows || _gridFit == FitType.FixedRows) // Horizontal
        {
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.pivot = new Vector2(0, 1);

            rectTransform.sizeDelta = new Vector2((_columns * _cellSize.x) + (Mathf.Clamp(_columns - 1, 0, float.MaxValue) * _cellSpacing.x) + padding.left + padding.right + offset.x, 0);
        }

        // Placing children
        for (int i = 0; i < rectChildren.Count; i++)
        {
            // Calculating the row and column index for child placement
            int rowIndex;
            int columnIndex;

            if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns) // Vertical
            {
                rowIndex = i / _columns;
                columnIndex = i % _columns;
            }
            else // Horizontal
            {
                rowIndex = i % _rows;
                columnIndex = i / _rows;
            }

            // Placing child
            var child = rectChildren[i];
            Vector2 childPos = CalculateChildPosition(rowIndex, columnIndex, offset);

            SetChildAlongAxis(child, 0, childPos.x, _cellSize.x);
            SetChildAlongAxis(child, 1, childPos.y, _cellSize.y);
        }
    }


    private float CalculateContentWidth()
    {
        return _cellSize.x * _columns + _cellSpacing.x * _columns - _cellSpacing.x + padding.left + padding.right;
    }


    private float CalculateContentHeight()
    {
        return _cellSize.y * _rows + _cellSpacing.y * _rows - _cellSpacing.y + padding.top + padding.bottom;
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


    private float CalculateMaxCellWidth(int columns, bool useParentRect = false) // calculating maximum width for a cell based on column amount
    {
        if (useParentRect)
        {
            RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                return parentRect.rect.width / (float)columns - ((_cellSpacing.x / (float)columns) * (columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
            }
        }

        return rectTransform.rect.width / (float)columns - ((_cellSpacing.x / (float)columns) * (columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
    }


    private float CalculateMaxCellHeight(int rows, bool useParentRect = false) // calculating maximum height for a cell based on row amount
    {
        if (useParentRect)
        {
            RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                return parentRect.rect.height / (float)rows - ((_cellSpacing.y / (float)rows) * (rows - 1)) - (padding.top / (float)rows) - (padding.bottom / (float)rows);
            }
        }

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


    private void CalculateCellSize() // calculate child size (if set to manual and dynamiccolumns/rows also set column and row amount)
    {
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
                            _cellSize.x = shrunkCellWidth;
                            _cellSize.y = _cellSize.x / GetCellAspectRatio(_preferredCellSize);
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

                            _cellSize.x = grownCellWidth;
                            _cellSize.y = _cellSize.x / GetCellAspectRatio(_preferredCellSize);
                        }
                        else
                        {
                            _cellSize.x = _preferredCellSize.x;
                            _cellSize.y = _preferredCellSize.y;
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
                            _cellSize.y = shrunkCellHeight;
                            _cellSize.x = GetCellAspectRatio(_preferredCellSize) * _cellSize.y;
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

                            _cellSize.y = grownCellHeight;
                            _cellSize.x = GetCellAspectRatio(_preferredCellSize) * _cellSize.y;
                        }
                        else
                        {
                            _cellSize.x = _preferredCellSize.x;
                            _cellSize.y = _preferredCellSize.y;
                        }
                    }

                    _columns = GetColumnsBasedOnRows(_rows);
                }

                else if (_gridFit == FitType.FixedColumns)
                {
                    // Get widest possible cell and clamp it to max cell width
                    _cellSize.x = CalculateMaxCellWidth(_columns);
                    _cellSize.x = Mathf.Clamp(_cellSize.x, 0, _maxCellSize.x);

                    // Calculate height for the cell
                    _cellSize.y = _cellSize.x / GetCellAspectRatio(_maxCellSize);
                }

                else if (_gridFit == FitType.FixedRows)
                {
                    // Get tallest possible cell and clamp it to max cell height
                    _cellSize.y = CalculateMaxCellHeight(_rows);
                    _cellSize.y = Mathf.Clamp(_cellSize.y, 0, _maxCellSize.y);

                    // Calculate width for the cell
                    _cellSize.x = GetCellAspectRatio(_maxCellSize) * _cellSize.y;
                }
                break;

            case CellSizeType.AspectRatio: // Calculating cell size from aspect ratio value

                if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns)
                {
                    _cellSize.x = CalculateMaxCellWidth(_columns);
                    _cellSize.y = _cellSize.x / _cellAspectRatio;
                }
                else if (_gridFit == FitType.DynamicRows || _gridFit == FitType.FixedRows)
                {
                    _cellSize.y = CalculateMaxCellHeight(_rows);
                    _cellSize.x = _cellAspectRatio * _cellSize.y;
                }
                break;

            case CellSizeType.BasedOnPrefab: // Getting cell width and height from prefab
                if (_prefab != null)
                {
                    RectTransform prefabRect = _prefab.GetComponent<RectTransform>();
                    if (prefabRect != null)
                    {
                        _cellSize.x = prefabRect.rect.width;
                        _cellSize.y = prefabRect.rect.height;
                    }
                }
                break;
        }

        // If prevent scaling out of parent bounds is checked scaling children inside parent boundaries
        if (_preventScalingOutOfParentBounds)
        {
            float cellAspectRatio = GetCellAspectRatio(_cellSize);
            Vector2 maxCellSizeToFitParent = new Vector2(CalculateMaxCellWidth(_columns, true), CalculateMaxCellHeight(_rows, true));

            if (cellAspectRatio != GetCellAspectRatio(maxCellSizeToFitParent))
            {
                // Checking which axis was shrunk more
                if (_cellSize.x - maxCellSizeToFitParent.x > _cellSize.y - maxCellSizeToFitParent.y)
                {
                    maxCellSizeToFitParent.y = maxCellSizeToFitParent.x / _cellAspectRatio;
                }
                else
                {
                    maxCellSizeToFitParent.x = _cellAspectRatio * maxCellSizeToFitParent.y;
                }
            }

            _cellSize.x = Mathf.Clamp(_cellSize.x, 0, maxCellSizeToFitParent.x);
            _cellSize.y = Mathf.Clamp(_cellSize.y, 0, maxCellSizeToFitParent.y);
        }
    }


    private Vector2 CalculateChildAlignmentOffset()
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        Vector2 offset = Vector2.zero;

        if (parentRect != null)
        {
            if (_gridFit == FitType.DynamicColumns || _gridFit == FitType.FixedColumns) // Vertical
            {
                // X Offset
                if (m_ChildAlignment == TextAnchor.UpperCenter || m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.LowerCenter)
                {
                    offset.x = (rectTransform.rect.width - CalculateContentWidth()) / 2;
                }

                if (m_ChildAlignment == TextAnchor.UpperRight || m_ChildAlignment == TextAnchor.MiddleRight || m_ChildAlignment == TextAnchor.LowerRight)
                {
                    offset.x = rectTransform.rect.width - CalculateContentWidth();
                }

                // Y Offset
                if (m_ChildAlignment == TextAnchor.MiddleLeft || m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.MiddleRight)
                {
                    if (CalculateContentHeight() < parentRect.rect.height) // Only offset if content height is smaller than parent rect
                    {
                        offset.y = (parentRect.rect.height - CalculateContentHeight()) / 2;
                    }
                }

                if (m_ChildAlignment == TextAnchor.LowerLeft || m_ChildAlignment == TextAnchor.LowerCenter || m_ChildAlignment == TextAnchor.LowerRight)
                {
                    if (CalculateContentHeight() < parentRect.rect.height)
                    {
                        offset.y = parentRect.rect.height - CalculateContentHeight();
                    }
                }
            }

            else if (_gridFit == FitType.DynamicRows || _gridFit == FitType.FixedRows) // Horizontal
            {
                // X Offset
                if (m_ChildAlignment == TextAnchor.UpperCenter || m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.LowerCenter)
                {
                    if (CalculateContentWidth() < parentRect.rect.width) // Only offset if content width is smaller than parent rect
                    {
                        offset.x = (parentRect.rect.width - CalculateContentWidth()) / 2;
                    }
                }

                if (m_ChildAlignment == TextAnchor.UpperRight || m_ChildAlignment == TextAnchor.MiddleRight || m_ChildAlignment == TextAnchor.LowerRight)
                {
                    if (CalculateContentWidth() < parentRect.rect.width)
                    {
                        offset.x = parentRect.rect.width - CalculateContentWidth();
                    }
                }

                // Y Offset
                if (m_ChildAlignment == TextAnchor.MiddleLeft || m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.MiddleRight)
                {
                    offset.y = (rectTransform.rect.height - CalculateContentHeight()) / 2;
                }

                if (m_ChildAlignment == TextAnchor.LowerLeft || m_ChildAlignment == TextAnchor.LowerCenter || m_ChildAlignment == TextAnchor.LowerRight)
                {
                    offset.y = rectTransform.rect.height - CalculateContentHeight();
                }
            }
        }

        return offset;
    }


    private Vector2 CalculateChildPosition(int rowIndex, int columnIndex, Vector2 offset)
    {
        Vector2 position = Vector2.zero;

        switch (_startCorner)
        {
            case StartCorner.UpperLeft:
                position.x = (_cellSize.x * columnIndex) + (_cellSpacing.x * columnIndex) + padding.left + offset.x;
                position.y = (_cellSize.y * rowIndex) + (_cellSpacing.y * rowIndex) + padding.top + offset.y;
                break;

            case StartCorner.UpperRight:
                position.x = (CalculateContentWidth() - _cellSize.x) - ((_cellSize.x * columnIndex) + (_cellSpacing.x * columnIndex) + padding.left) + offset.x;
                position.y = (_cellSize.y * rowIndex) + (_cellSpacing.y * rowIndex) + padding.top + offset.y;
                break;

            case StartCorner.LowerLeft:
                position.x = (_cellSize.x * columnIndex) + (_cellSpacing.x * columnIndex) + padding.left + offset.x;
                position.y = (CalculateContentHeight() - _cellSize.y) - ((_cellSize.y * rowIndex) + (_cellSpacing.y * rowIndex) + padding.top) + offset.y;
                break;

            case StartCorner.LowerRight:
                position.x = (CalculateContentWidth() - _cellSize.x) - ((_cellSize.x * columnIndex) + (_cellSpacing.x * columnIndex) + padding.left) + offset.x;
                position.y = (CalculateContentHeight() - _cellSize.y) - ((_cellSize.y * rowIndex) + (_cellSpacing.y * rowIndex) + padding.top) + offset.y;
                break;
        }

        return position;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(FlexibleGridLayout))]
    public class FlexibleGridLayoutEditor : Editor
    {
        // LayoutGroup properties
        SerializedProperty _padding;
        SerializedProperty _startCornerEnum;
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

        SerializedProperty _preventScalingOutOfBounds;

        void OnEnable()
        {
            // Getting LayoutGroup properties
            _padding = serializedObject.FindProperty(nameof(m_Padding));
            _startCornerEnum = serializedObject.FindProperty(nameof(_startCorner));
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

            _preventScalingOutOfBounds = serializedObject.FindProperty(nameof(_preventScalingOutOfParentBounds));
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Padding
            EditorGUILayout.PropertyField(_padding);
            EditorGUILayout.Space();

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
                    break;
                case CellSizeType.AspectRatio:
                    EditorGUILayout.PropertyField(_cellAspectRatio);
                    break;
                case CellSizeType.BasedOnPrefab:
                    EditorGUILayout.PropertyField(_cellPrefab);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_cellSpacing);
            EditorGUILayout.Space();

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
                    }
                    break;
                case FitType.DynamicRows:
                    if ((CellSizeType)_cellSizeSelection.enumValueIndex != CellSizeType.Manual)
                    {
                        EditorGUILayout.PropertyField(_gridFitMinRows);
                        EditorGUILayout.PropertyField(_gridFitMaxRows);
                    }
                    break;
                case FitType.FixedColumns:
                    EditorGUILayout.PropertyField(_gridFitColumns);
                    break;
                case FitType.FixedRows:
                    EditorGUILayout.PropertyField(_gridFitRows);
                    break;
            }

            EditorGUILayout.Space();

            // Place alignment property fields
            EditorGUILayout.PropertyField(_startCornerEnum);
            EditorGUILayout.PropertyField(_childAlignment);

            EditorGUILayout.PropertyField(_preventScalingOutOfBounds);

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
