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
            Image colorImage = colorGridCell.transform.Find("FeatureImage").GetComponent<Image>();
            Image backgroundImage = colorGridCell.transform.Find("BackgroundImage").GetComponent<Image>();
            RectTransform colorImageRect = colorGridCell.transform.Find("FeatureImage").GetComponent<RectTransform>();

            colorImage.color = color;
            colorImageRect.anchorMin = new Vector2(0.05f, 0.05f);
            colorImageRect.anchorMax = new Vector2(0.95f, 0.95f);
            backgroundImage.color = _backgroundColor;
            AddListener();
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
