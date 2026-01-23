using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorGridLoader : MonoBehaviour
    {
        // Now just makes the color selection cells. Functionality not implemented yet

        [SerializeField] private RectTransform _colorGridContent;
        [SerializeField] private RectTransform _colorSelection;
        [SerializeField] private GameObject _gridCellPrefab;
        [SerializeField] private HorizontalLayoutGroup _colorGrid;
        [SerializeField] private List<Color> _colors;
        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] private ScrollBarFeatureLoader _featureLoader;

        private float _viewPortHeight;
        private float _cellHeight;
        private float _colorGridPadding;
        public List<Color> Colors { get { return _colors; } }

        private void AddColorCell(Color color)
        {
            GameObject colorGridCell = Instantiate(_gridCellPrefab, _colorGridContent);
            ColorCellHandler handler = colorGridCell.GetComponent<ColorCellHandler>();

            handler.SetColor(color);
            handler.SetOnClick(() => AddListener(color));
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

        private void AddListener(Color color)
        {
            // don't know about this
            AvatarPiece slot = _featureLoader._featureCategoryIdToAvatarPiece[_categoryLoader.CurrentlySelectedCategory];

            _characterHandle.SetPartColor(slot, color);

            _avatarEditorController.PlayerAvatar.SetPartColor(slot, ColorUtility.ToHtmlStringRGBA(color));
        }
    }
}
