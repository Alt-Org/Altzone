using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the GridLayoutGroup's cell width and height to fill the constrainted axis.
/// </summary>
// [ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
public class GridAutoScale : MonoBehaviour
{
    [SerializeField] private float _aspectRatio = 1f;

    private GridLayoutGroup _gridLayout;
    private RectTransform _gridContainer;

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        if (_gridLayout == null) _gridLayout = GetComponent<GridLayoutGroup>();
        if (_gridContainer == null) _gridContainer = GetComponent<RectTransform>();

        if ((_gridContainer.rect.width == 0 && _gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            || (_gridContainer.rect.height == 0 && _gridLayout.constraint == GridLayoutGroup.Constraint.FixedRowCount)) return;
        if (_aspectRatio <= 0) _aspectRatio = 0.000001f;

        StartCoroutine(SetCellSizes());
    }

    private IEnumerator SetCellSizes()
    {
        yield return null;

        switch (_gridLayout.constraint)
        {
            case GridLayoutGroup.Constraint.FixedColumnCount:
                float gridCellWidth = _gridContainer.rect.width / _gridLayout.constraintCount - _gridLayout.spacing.x / _gridLayout.constraintCount * (_gridLayout.constraintCount - 1);
                if (gridCellWidth <= 0) gridCellWidth = 0.000001f;
                _gridLayout.cellSize = new Vector2(gridCellWidth, gridCellWidth / _aspectRatio);
                break;
            case GridLayoutGroup.Constraint.FixedRowCount:
                float gridCellHeight = _gridContainer.rect.width / _gridLayout.constraintCount - _gridLayout.spacing.y / _gridLayout.constraintCount * (_gridLayout.constraintCount - 1);
                if (gridCellHeight <= 0) gridCellHeight = 0.000001f;
                _gridLayout.cellSize = new Vector2(gridCellHeight * _aspectRatio, gridCellHeight);
                break;
        }
    }
}
