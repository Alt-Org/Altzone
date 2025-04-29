using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Settings.BattleUiEditor
{
    /// <summary>
    /// Instantiate a certain amount of grid line prefabs to vertical and horizontal layout groups to display grid rows and columns,
    /// and adjust layout group padding and spacing accordingly.
    /// </summary>
    public class GridController : MonoBehaviour
    {
        [Header("Grid options")]
        [SerializeField] private int _gridLineThickness = 5;
        [SerializeField] private Color _lineDefaultColor = Color.black;
        [SerializeField] private Color _lineHighlightedColor = Color.blue;

        [Header("GameObject references.")]
        [SerializeField] private Transform _gridColumns;
        [SerializeField] private Transform _gridRows;
        [SerializeField] private GameObject _gridLinePrefab;

        public static float GridCellWidth => Screen.width / s_columns;
        public static float GridCellHeight => Screen.height / s_rows;

        public static int GetGridColumnIndex(float xPos)
        {
            return Mathf.RoundToInt(xPos / GridCellWidth) - 1;
        }

        public static float GetGridSnapPositionX(int gridColumnIndex)
        {
            return (gridColumnIndex + 1) * GridCellWidth;
        }

        public static int GetGridRowIndex(float yPos)
        {
            return Mathf.RoundToInt(yPos / GridCellHeight) - 1;
        }

        public static float GetGridSnapPositionY(int gridRowIndex)
        {
            return (gridRowIndex + 1) * GridCellHeight;
        }

        /// <summary>
        /// Set grid visibility.
        /// </summary>
        /// <param name="show">If grid should show or not.</param>
        public void SetShow(bool show)
        {
            _gridColumns.gameObject.SetActive(show);
            _gridRows.gameObject.SetActive(show);
        }

        /// <summary>
        /// Set column grid lines.
        /// </summary>
        /// <param name="columns">The amount of columns as int.</param>
        public bool SetColumns(int columns)
        {
            if (s_columns == columns) return false; // If the value didn't change we return
            s_columns = columns;

            int lineAmount = columns - 1;
            InstantiateGridLines(lineAmount, _gridColumns.transform);

            // Positioning column lines
            for (int i = 0; i < _gridColumns.childCount; i++)
            {
                Transform child = _gridColumns.GetChild(i);
                RectTransform childRectTransform = child.GetComponent<RectTransform>();

                Vector2 size = new(
                    _gridLineThickness,
                    _editorHeight
                );

                Vector2 pos = new(
                    GetGridSnapPositionX(i),
                    _editorHeight * 0.5f
                );

                (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, pos);

                childRectTransform.anchorMin = anchorMin;
                childRectTransform.anchorMax = anchorMax;
            }

            return true;
        }

        /// <summary>
        /// Set row grid lines.
        /// </summary>
        /// <param name="rows">The amount of rows as int.</param>
        public bool SetRows(int rows)
        {
            if (s_rows == rows) return false;
            s_rows = rows;

            int lineAmount = rows - 1;
            InstantiateGridLines(lineAmount, _gridRows.transform);

            // Positioning row lines
            for (int i = 0; i < _gridRows.childCount; i++)
            {
                Transform child = _gridRows.GetChild(i);
                RectTransform childRectTransform = child.GetComponent<RectTransform>();

                Vector2 size = new(
                    _editorWidth,
                    _gridLineThickness
                );

                Vector2 pos = new(
                    _editorWidth * 0.5f,
                    GetGridSnapPositionY(i)
                );

                (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, pos);

                childRectTransform.anchorMin = anchorMin;
                childRectTransform.anchorMax = anchorMax;
            }

            return true;
        }

        /// <summary>
        /// Highlight grid lines which are near the position.
        /// </summary>
        public void HighlightLinesNearPosition(int gridColumnIndex, int gridRowIndex)
        {
            RemoveLineHighlight();

            // Highlighting the row color
            Image highlightedRow = _gridRows.transform.GetChild(gridRowIndex).GetComponent<Image>();
            highlightedRow.color = _lineHighlightedColor;

            // Highlighting the column color
            Image highlightedColumn = _gridColumns.transform.GetChild(gridColumnIndex).GetComponent<Image>();
            highlightedColumn.color = _lineHighlightedColor;

            _highlightedLines = new[] { highlightedRow, highlightedColumn };
        }

        /// <summary>
        /// Changes highlighted lines back to the default color.
        /// </summary>
        public void RemoveLineHighlight()
        {
            if (_highlightedLines != null)
            {
                foreach (var line in _highlightedLines)
                {
                    line.color = _lineDefaultColor;
                }
                _highlightedLines = null;
            }
        }

        private static int s_rows = -1;
        private static int s_columns = -1;

        private float _editorHeight => Screen.height;
        private float _editorWidth => Screen.width;

        private RectTransform _parentRectTransform;
        private Image[] _highlightedLines;

        private void Awake()
        {
            _parentRectTransform = GetComponentInParent<RectTransform>();
        }

        private void InstantiateGridLines(int lineAmount, Transform lineParent)
        {
            // If we don't need to add or remove lines we return.
            if (lineAmount == lineParent.childCount) return;

            // Adding missing lines
            int linesToInstantiate = lineAmount - lineParent.childCount;
            if (linesToInstantiate > 0)
            {
                for (int i = 0; i < linesToInstantiate; i++)
                {
                    GameObject line = Instantiate(_gridLinePrefab);
                    line.GetComponent<Image>().color = _lineDefaultColor;
                    line.transform.SetParent(lineParent.transform, false);
                }
                return;
            }

            // Removing extra lines
            int linesToRemove = lineParent.childCount - lineAmount;
            for (int i = 0; i < linesToRemove; i++)
            {
                Destroy(lineParent.GetChild(i).gameObject);
            }
        }
    }
}
