using System;
using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
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
        [SerializeField] private List<Color> _colors;
        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private AvatarEditorCharacterHandle _characterHandle;
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] private ScrollBarFeatureLoader _featureLoader;
        [SerializeField] private List<Color> _skinColors;

        private float _viewPortHeight;
        private float _cellHeight;
        private float _horizontalPadding;
        private float _verticalPadding;
        private bool _skinColorSelectActive = false;
        private ColorCellHandler _lastSelectedHandler;

        private Dictionary<string, ColorCellHandler> _colorToHandler = new();

        public List<Color> Colors { get { return _colors; } }
        public List<Color> SkinColors { get { return _skinColors; } }
        public bool SkinColorSelectActive { get { return _skinColorSelectActive; } }

        public void UpdateHighlight(string colorString)
        {
            if (_lastSelectedHandler != null) _lastSelectedHandler.Highlight(false);

            // This seems kind of brittle, but it works.
            // In a perfect world playeravatar colors should always be the same format
            ColorUtility.TryParseHtmlString(colorString, out Color color);
            string key = ColorUtility.ToHtmlStringRGBA(color).ToLower();

            if (_colorToHandler.TryGetValue(key, out ColorCellHandler handler))
            {
                _lastSelectedHandler = handler;
                _lastSelectedHandler.Highlight(true);
            }
        }

        private void AddColorCell(Color color, Action<Color, ColorCellHandler> callback)
        {
            GameObject colorGridCell = Instantiate(_gridCellPrefab, _colorGridContent);
            ColorCellHandler handler = colorGridCell.GetComponent<ColorCellHandler>();

            handler.SetColor(color);
            handler.SetOnClick(() => callback(color, handler));

            _colorToHandler[ColorUtility.ToHtmlStringRGBA(color).ToLower()] = handler;
        }

        private void DestroyColorCells()
        {
            foreach (RectTransform child in _colorGridContent)
            {
                Destroy(child.gameObject);
            }
            _colorToHandler.Clear();
            _lastSelectedHandler = null;
        }

        public void SetColorCells()
        {
            DestroyColorCells();
            foreach (Color color in _colors)
            {
                AddColorCell(color, AddListener);
            }
            _skinColorSelectActive = false;
            UpdateCellSize();
        }

        public void SetSkinColorCells()
        {
            DestroyColorCells();
            foreach (Color color in _skinColors)
            {
                AddColorCell(color, AddSkinColorListener);
            }
            _skinColorSelectActive = true;
            UpdateCellSize();
        }

        public void UpdateCellSize()
        {
            _viewPortHeight = _colorSelection.rect.height;
            _cellHeight = _viewPortHeight * 0.8f;
            _horizontalPadding = _viewPortHeight * 0.05f;
            _verticalPadding = (_viewPortHeight - _cellHeight) / 2f;

            _colorGrid.padding.left = Mathf.CeilToInt(_horizontalPadding);
            _colorGrid.padding.right = Mathf.CeilToInt(_horizontalPadding);
            _colorGrid.padding.top = Mathf.CeilToInt(_verticalPadding);
            _colorGrid.padding.bottom = Mathf.CeilToInt(_verticalPadding);
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

        private void AddListener(Color color, ColorCellHandler handler)
        {
            AvatarPiece? slot = _featureLoader.CurrentCategory;

            if (slot == null) return;

            AvatarPiece actualSlot = (AvatarPiece)slot;

            _characterHandle.SetPartColor(actualSlot, color);

            _avatarEditorController.PlayerAvatar.SetPartColor(actualSlot, ColorUtility.ToHtmlStringRGBA(color));

            if (_lastSelectedHandler != null)
            {
                _lastSelectedHandler.Highlight(false);
            }
            _lastSelectedHandler = handler;
            _lastSelectedHandler.Highlight(true);
        }

        private void AddSkinColorListener(Color color, ColorCellHandler handler)
        {
            _characterHandle.SetSkinColor(color);

            _avatarEditorController.PlayerAvatar.SkinColor = ColorUtility.ToHtmlStringRGBA(color);

            if (_lastSelectedHandler != null)
            {
                _lastSelectedHandler.Highlight(false);
            }
            _lastSelectedHandler = handler;
            _lastSelectedHandler.Highlight(true);
        }
    }
}
