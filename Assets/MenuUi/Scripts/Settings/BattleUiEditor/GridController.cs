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
        [SerializeField] private HorizontalLayoutGroup _gridColumns;
        [SerializeField] private VerticalLayoutGroup _gridRows;
        [SerializeField] private GameObject _gridLinePrefab;

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
        public void SetColumns(int columns)
        {
            int lineAmount = columns - 1;
            SetGridLines(lineAmount, _gridColumns.transform);

            float spaceBetweenLines = _parentRectTransform.rect.width / columns - _gridLineThickness / 2;
            if (lineAmount > 1) _gridColumns.spacing = spaceBetweenLines;
            _gridColumns.padding.left = Mathf.FloorToInt(spaceBetweenLines);
            _gridColumns.padding.right = Mathf.FloorToInt(spaceBetweenLines);
        }

        /// <summary>
        /// Set row grid lines.
        /// </summary>
        /// <param name="rows">The amount of rows as int.</param>
        public void SetRows(int rows)
        {
            int lineAmount = rows - 1;
            SetGridLines(lineAmount, _gridRows.transform);

            float spaceBetweenLines = _parentRectTransform.rect.height / rows - _gridLineThickness / 2;
            if (lineAmount > 1) _gridRows.spacing = spaceBetweenLines;
            _gridRows.padding.top = Mathf.FloorToInt(spaceBetweenLines);
            _gridRows.padding.bottom = Mathf.FloorToInt(spaceBetweenLines);
        }

        /// <summary>
        /// Highlight grid lines which are near the position.
        /// </summary>
        /// <param name="position">Position as Vector2 in world space.</param>
        public void HighlightLinesNearPosition(Vector2 position)
        {
            RemoveLineHighlight();

            // Calculating editor height and width
            float editorHeight = _parentRectTransform.rect.height * (Screen.height / _parentRectTransform.rect.height);
            float editorWidth = _parentRectTransform.rect.width * (Screen.width / _parentRectTransform.rect.width);

            // Getting nearest row line index
            int rowCount = _gridRows.transform.childCount;
            int nearestRowLine = Mathf.CeilToInt(position.y / (editorHeight / rowCount));
            nearestRowLine = rowCount - nearestRowLine;
            nearestRowLine = Mathf.Clamp(nearestRowLine, 0, rowCount - 1);

            // Highlighting the row color
            Image highlightedRow = _gridRows.transform.GetChild(nearestRowLine).GetComponent<Image>();
            highlightedRow.color = _lineHighlightedColor;

            // Getting nearest column line index
            int columnCount = _gridColumns.transform.childCount;
            int nearestColumnLine = Mathf.CeilToInt(position.x / (editorWidth / columnCount)) - 1;
            nearestColumnLine = Mathf.Clamp(nearestColumnLine, 0, columnCount - 1);

            // Highlighting the column color
            Image highlightedColumn = _gridColumns.transform.GetChild(nearestColumnLine).GetComponent<Image>();
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

        private RectTransform _parentRectTransform;
        private Image[] _highlightedLines;

        private void Awake()
        {
            _parentRectTransform = GetComponentInParent<RectTransform>();
        }

        private void SetGridLines(int lineAmount, Transform lineParent)
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
