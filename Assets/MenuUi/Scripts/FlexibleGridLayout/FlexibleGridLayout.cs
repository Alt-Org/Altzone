using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Dynamic,
        FixedColumns,
        FixedRows,
    }

    public enum CellSizeType
    {
        Manual,
        AspectRatio,
        BasedOnChild
    }

    const int MinDynamicColumns = 2;
    const int MaxDynamicColumns = 4;
    const float ShortestAspectRatio = 4.0f / 3.0f;
    const float TallestAspectRatio = 22.0f / 9.0f;


    [Header("Flexible Grid")]

    [SerializeField, Tooltip("How the grid will fit its children.\n\nDynamic: Determine column amount based on game's aspect ratio and cell size.\nFixed Columns: Column amount stays the same.\nFixed Rows: Row amount stays the same.")]
    private FitType _gridFit = FitType.Dynamic;

    [SerializeField, Min(1)]
    private int _columnAmount = 1;

    [SerializeField, Min(1)]
    private int _rowAmount = 1;

    [SerializeField, Tooltip("How the grid's cell size is determined.\n\nManual: Give cell size values manually.\nAspect Ratio: Cell size is automatically calculated to fit this aspect ratio.\nBased On Child: Cell size is set automatically based on the first child object.")]
    private CellSizeType _gridCellSize = CellSizeType.Manual;

    [SerializeField, Min(0)]
    private Vector2 _minCellSize;

    [SerializeField, Min(0)]
    private Vector2 _maxCellSize;

    [SerializeField, Min(0)]
    private Vector2 _preferredCellSize;

    [SerializeField, Min(0)]
    private float _cellAspectRatio;

    [SerializeField, Tooltip("The spacing between grid cells."), Min(0)]
    private Vector2 _cellSpacing;

    private int _rows;
    private int _columns;
    private Vector2 _cellSize;

    private bool _fitX;
    private bool _fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        _cellSize = _preferredCellSize;
        
        if (_gridFit == FitType.Dynamic)
        {
            float screenAspectRatio = (float)Screen.currentResolution.height / Screen.currentResolution.width;
            screenAspectRatio = Mathf.Clamp(screenAspectRatio, ShortestAspectRatio, TallestAspectRatio);

            // the percentage at which point the current aspect ratio sits between shortest and tallest aspect ratios
            float aspectratioPercentage = (screenAspectRatio - ShortestAspectRatio) / (TallestAspectRatio - ShortestAspectRatio);

            // getting column amount between min and max amount of columns based on the percentage
            _columns = Mathf.RoundToInt(MaxDynamicColumns + (MinDynamicColumns - MaxDynamicColumns) * aspectratioPercentage);

            _rows = Mathf.CeilToInt(transform.childCount / (float)_columns);
        }

        if (_gridFit == FitType.FixedColumns)
        {
            _columns = _columnAmount;
            _rows = Mathf.CeilToInt(transform.childCount / (float)_columns);
        }

        if (_gridFit == FitType.FixedRows)
        {
            _rows = _rowAmount;
            _columns = Mathf.CeilToInt(transform.childCount / (float)_rows);
        }
        
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = parentWidth / (float)_columns - ((_cellSpacing.x / (float)_columns) * (_columns - 1))
            - (padding.left / (float)_columns) - (padding.right / (float)_columns);
        float cellHeight = parentHeight / (float)_rows - ((_cellSpacing.y / (float)_rows) * (_rows - 1))
            - (padding.top / (float)_rows) - (padding.bottom / (float)_rows); ;

        _cellSize.x = _fitX ? cellWidth : _cellSize.x;
        _cellSize.y = _fitY ? cellHeight : _cellSize.y;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / _columns;
            columnCount = i % _columns;

            var item = rectChildren[i];

            var xPos = (_cellSize.x * columnCount) + (_cellSpacing.x * columnCount) + padding.left;
            var yPos = (_cellSize.y * rowCount) + (_cellSpacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, _cellSize.x);
            SetChildAlongAxis(item, 1, yPos, _cellSize.y);

        }
        rectTransform.sizeDelta = new Vector2(0, rowCount * _cellSize.y);
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
        SerializedProperty _gridFitColumns;
        SerializedProperty _gridFitRows;

        // Cell size properties
        SerializedProperty _cellSizeSelection;
        SerializedProperty _minCellSize;
        SerializedProperty _maxCellSize;
        SerializedProperty _preferredCellSize;
        SerializedProperty _cellAspectRatio;

        // Cell spacing property
        SerializedProperty _cellSpacing;


        void OnEnable()
        {
            // Getting LayoutGroup properties
            _padding = serializedObject.FindProperty(nameof(m_Padding));
            _childAlignment = serializedObject.FindProperty(nameof(m_ChildAlignment));

            // Getting grid fit properties
            _gridFitSelection = serializedObject.FindProperty(nameof(_gridFit));
            _gridFitColumns = serializedObject.FindProperty(nameof(_columnAmount));
            _gridFitRows = serializedObject.FindProperty(nameof(_rowAmount));

            // Getting cell size properties
            _cellSizeSelection = serializedObject.FindProperty(nameof(_gridCellSize));
            _minCellSize = serializedObject.FindProperty(nameof(_minCellSize));
            _maxCellSize = serializedObject.FindProperty(nameof(_maxCellSize));
            _preferredCellSize = serializedObject.FindProperty(nameof(_preferredCellSize));
            _cellAspectRatio = serializedObject.FindProperty(nameof(_cellAspectRatio));

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
                    EditorGUILayout.PropertyField(_minCellSize);
                    EditorGUILayout.PropertyField(_maxCellSize);
                    EditorGUILayout.PropertyField(_preferredCellSize);
                    EditorGUILayout.Space();
                    break;
                case CellSizeType.AspectRatio:
                    EditorGUILayout.PropertyField(_cellAspectRatio);
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
