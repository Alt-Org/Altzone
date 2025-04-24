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

        private RectTransform _parentRectTransform;

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
