using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorGridLoader : MonoBehaviour
    {
        [SerializeField] private RectTransform _colorGridContent;
        [SerializeField] private RectTransform _colorSelection;
        [SerializeField] private GameObject _gridCellPrefab;
        [SerializeField] private HorizontalLayoutGroup _colorGrid;
        [SerializeField] private Color _backgroundColor = new(0, 0, 0);
        [SerializeField] private List<Color> _colors;

        private float _viewPortHeight;
        private float _cellHeight;
        private float _colorGridPadding;

        private void AddColorCell(Color color)
        {
            GameObject colorGridCell = Instantiate(_gridCellPrefab, _colorGridContent);
            GridCellHandler handler = colorGridCell.GetComponent<GridCellHandler>();

            Vector2 colorImageMinAnchors = new(0.05f, 0.05f);
            Vector2 colorImageMaxAnchors = new(0.95f, 0.95f);

            handler.SetValues(cellImageColor: color, backgroundColor: _backgroundColor,
                onClick: () => AddListener());
            handler.SetFeatureImageAnchors(colorImageMinAnchors, colorImageMaxAnchors);
        }

        public void SetColorCells()
        {
            foreach (Color color in _colors)
            {
                AddColorCell(color);
            }
        }

        public void UpdateCellSize()
        {
            _viewPortHeight = _colorSelection.rect.height;
            _cellHeight = _viewPortHeight * 0.9f;
            _colorGridPadding = _viewPortHeight * 0.05f;

            _colorGrid.padding.left = Mathf.CeilToInt(_colorGridPadding);
            _colorGrid.padding.right = Mathf.CeilToInt(_colorGridPadding);
            _colorGrid.padding.top = Mathf.CeilToInt(_colorGridPadding);
            _colorGrid.padding.bottom = Mathf.CeilToInt(_colorGridPadding);
            _colorGrid.spacing = 0.05f * _cellHeight;

            foreach (RectTransform child in _colorGridContent)
            {
                LayoutElement le = child.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredHeight = _cellHeight;
                    le.preferredWidth = _cellHeight;
                }
            }
        }

        private void AddListener()
        {

        }
    }
}
