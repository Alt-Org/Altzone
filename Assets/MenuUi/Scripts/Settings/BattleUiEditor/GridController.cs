using System;
using System.Collections;

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

        /// <summary>
        /// Get the grid column line's index which is nearest to the x axis position.
        /// </summary>
        /// <param name="xPos">The x axis position.</param>
        /// <returns>The column line index as float.</returns>
        public static int GetGridColumnIndex(float xPos)
        {
            return Mathf.Clamp(Mathf.RoundToInt(xPos / s_gridCellWidth) - 1, 0, s_columns - 2);
        }

        /// <summary>
        /// Get grid snap position in the y axis according to the grid column line's index.
        /// </summary>
        /// <param name="gridColumnIndex">The grid column line index which position to get.</param>
        /// <returns>Grid y axis snap position as float.</returns>
        public static float GetGridSnapPositionX(int gridColumnIndex)
        {
            return (gridColumnIndex + 1) * s_gridCellWidth;
        }

        /// <summary>
        /// Get the grid row line's index which is nearest to the y axis position.
        /// </summary>
        /// <param name="yPos">The y axis position.</param>
        /// <returns>The row line index as float.</returns>
        public static int GetGridRowIndex(float yPos)
        {
            return Mathf.Clamp(Mathf.RoundToInt(yPos / s_gridCellHeight) - 1, 0, s_rows - 2);
        }

        /// <summary>
        /// Get grid snap position in the x axis according to the grid row line's index.
        /// </summary>
        /// <param name="gridRowIndex">The grid row line index which position to get.</param>
        /// <returns>Grid x axis snap position as float.</returns>
        public static float GetGridSnapPositionY(int gridRowIndex)
        {
            return (gridRowIndex + 1) * s_gridCellHeight;
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
            if (s_columns == columns && _gridColumns.transform.childCount > 0) return false; // If the value didn't change we return
            s_columns = columns;

            int lineAmount = columns - 1;
            StartCoroutine(InstantiateGridLines(lineAmount, _gridColumns.transform, () =>
            {
                // Positioning column lines
                for (int i = 0; i < _gridColumns.childCount; i++)
                {
                    Transform child = _gridColumns.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();

                    Vector2 size = new(
                        _gridLineThickness,
                        Screen.height
                    );

                    Vector2 pos = new(
                        GetGridSnapPositionX(i) - _gridLineThickness * 0.5f,
                        Screen.height * 0.5f
                    );

                    (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, pos);

                    childRectTransform.anchorMin = anchorMin;
                    childRectTransform.anchorMax = anchorMax;
                }
            }));

            return true;
        }

        /// <summary>
        /// Set row grid lines.
        /// </summary>
        /// <param name="rows">The amount of rows as int.</param>
        public bool SetRows(int rows)
        {
            if (s_rows == rows && _gridRows.transform.childCount > 0) return false;
            s_rows = rows;

            int lineAmount = rows - 1;
            StartCoroutine(InstantiateGridLines(lineAmount, _gridRows.transform, () =>
            {
                // Positioning row lines
                for (int i = 0; i < _gridRows.childCount; i++)
                {
                    Transform child = _gridRows.GetChild(i);
                    RectTransform childRectTransform = child.GetComponent<RectTransform>();

                    Vector2 size = new(
                        Screen.width,
                        _gridLineThickness
                    );

                    Vector2 pos = new(
                        Screen.width * 0.5f,
                        GetGridSnapPositionY(i) - _gridLineThickness * 0.5f
                    );

                    (Vector2 anchorMin, Vector2 anchorMax) = BattleUiEditor.CalculateAnchors(size, pos);

                    childRectTransform.anchorMin = anchorMin;
                    childRectTransform.anchorMax = anchorMax;
                }
            }));

            return true;
        }

        /// <summary>
        /// Highlight grid lines.
        /// </summary>
        /// <param name="gridColumnIndex">The grid column line index which to highlight.</param>
        /// <param name="gridRowIndex">The grid row line index which to highlight.</param>
        public void HighlightLines(int gridColumnIndex, int gridRowIndex)
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
                foreach (Image line in _highlightedLines)
                {
                    line.color = _lineDefaultColor;
                }
                _highlightedLines = null;
            }
        }

        private static float s_gridCellWidth => Screen.width / s_columns;
        private static float s_gridCellHeight => Screen.height / s_rows;

        private static int s_rows = -1;
        private static int s_columns = -1;

        private Image[] _highlightedLines;

        private IEnumerator InstantiateGridLines(int lineAmount, Transform lineParent, Action callback)
        {
            // If we don't need to add or remove lines we stop coroutine.
            if (lineAmount == lineParent.childCount) yield break;

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

                // We don't need to wait 1 frame when adding lines and it will cause flickering
                callback();
                yield break;
            }

            // Removing extra lines
            int linesToRemove = lineParent.childCount - lineAmount;
            for (int i = 0; i < linesToRemove; i++)
            {
                Destroy(lineParent.GetChild(i).gameObject);
            }

            // Waiting 1 frame after deleting lines so that they are really deleted and can be positioned correctly
            yield return null;
            callback();
        }
    }
}
