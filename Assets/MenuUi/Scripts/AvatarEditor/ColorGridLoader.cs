using System.Collections.Generic;
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

        [SerializeField] private Material _featureMaterial;
        [SerializeField] private Color _skinColor = new(1, 0.984f, 0);
        [SerializeField] private Color _bodyColor = new(0, 0.784f, 1);
        [SerializeField] private Color _classColor = new(1, 0, 0.937f);
        [SerializeField] private Color _hairColor = new(0, 1, 0);

        private float _viewPortHeight;
        private float _cellHeight;
        private float _colorGridPadding;

        Texture2D CreateSwapTexture(Color skinColor, Color bodyColor, Color classColor)
        {
            Texture2D swapTex = new(256, 1, TextureFormat.RGBA32, false);
            swapTex.filterMode = FilterMode.Point;
            swapTex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            swapTex.SetPixels(pixels);

            swapTex.SetPixel(0, 0, Color.clear);
            swapTex.SetPixel(1, 0, classColor);
            swapTex.SetPixel(2, 0, skinColor);
            swapTex.SetPixel(3, 0, bodyColor);
            swapTex.Apply();
            return swapTex;
        }

        private void Start()
        {
            Texture2D swapTex = CreateSwapTexture(_skinColor, _bodyColor, _classColor);
            _featureMaterial.SetTexture("_SwapTex", swapTex);
        }

        private void AddColorCell(Color color)
        {
            GameObject colorGridCell = Instantiate(_gridCellPrefab, _colorGridContent);
            ColorCellHandler handler = colorGridCell.GetComponent<ColorCellHandler>();

            handler.SetColor(color);
            handler.SetOnClick(() => AddListener());
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


        // Not implemented yet
        private void AddListener()
        {

        }
    }
}
