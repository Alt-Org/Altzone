using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ScrollBarFeatureLoader : MonoBehaviour
    {
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private RectTransform _featureGridContent;
        [SerializeField] private GameObject _gridCellPrefab;
        [SerializeField] private FeatureSetter _featureSetter;
        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private RectTransform _featureGrid;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private ScrollRect _scrollrect;
        [SerializeField] private RectTransform _viewPort;
        [SerializeField] private Image _leftFade;
        [SerializeField] private Image _rightFade;
        [SerializeField] private ColorGridLoader _colorSelection;
        [SerializeField, Range(0f, 0.3f)] private float _horizontalPadding = 0.1f;
        [SerializeField, Range(0f, 0.2f)] private float _verticalPadding = 0.05f;
        [SerializeField, Range(0f, 0.3f)] private float _verticalSpacing = 0.05f;
        [SerializeField, Range(0f, 0.3f)] private float _horizontalSpacing = 0.05f;
        [SerializeField, Range(0f, 0.3f)] private float _fadeRange = 0.1f;
        [SerializeField] private Color _highlightColor = new(0f, 0f, 0f, 0.5f);
        [SerializeField] private Color _backgroundColor = new(0.5f, 0.5f, 0.5f, 0.7f);

        private List<AvatarPartInfo> _avatarPartInfo;
        private Dictionary<string, int> _featureCategoryIdToFeatureSlotInt;
        private PlayerAvatar _playeravatar;
        private FeatureCellHandler _selectedCellHandler;
        private bool _isSelectedFeature = false;
        private float _cellHeight;
        private float _actualVerticalSpacing;
        private float _actualHorizontalSpacing;
        private float _actualVerticalPadding;
        private float _viewPortHeight;
        private readonly int _rowAmount = 2;

        void Start()
        {
            _featureCategoryIdToFeatureSlotInt = new Dictionary<string, int>
            {
                { "10", 0 }, // Hair
                { "21", 1 }, // Eyes
                { "22", 2 }, // Nose
                { "23", 3 }, // Mouth
                { "31", 4 }, // Body
                { "32", 5 }, // Hands
                { "33", 6 }  // Feet
            };

            _scrollrect.onValueChanged.AddListener(UpdateFade);
        }

        private void UpdateFade(Vector2 normalizedPos)
        {
            if (_featureGridContent.rect.width < _scrollrect.viewport.rect.width)
            {
                _leftFade.gameObject.SetActive(false);
                _rightFade.gameObject.SetActive(false);
                return;
            }

            _leftFade.gameObject.SetActive(true);
            _rightFade.gameObject.SetActive(true);

            float pos = normalizedPos.x;

            float leftAlpha = Mathf.Clamp01(pos / _fadeRange);
            float rightAlpha = Mathf.Clamp01((1f - pos) / _fadeRange);

            Color leftColor = _leftFade.color;
            Color rightColor = _rightFade.color;

            leftColor.a = leftAlpha;
            rightColor.a = rightAlpha;

            _leftFade.color = leftColor;
            _rightFade.color = rightColor;
        }

        public void UpdateCellSize()
        {
            _viewPortHeight = _viewPort.rect.height;
            _actualHorizontalSpacing = _horizontalSpacing * _scrollrect.viewport.rect.width;
            _actualVerticalSpacing = _verticalSpacing * _viewPortHeight;
            _actualVerticalPadding = _verticalPadding * _featureGrid.rect.height;
            _gridLayoutGroup.spacing = new Vector2(_actualHorizontalSpacing, _actualVerticalSpacing);
            _gridLayoutGroup.padding.left = Mathf.CeilToInt(_horizontalPadding * _scrollrect.viewport.rect.width);
            _gridLayoutGroup.padding.right = Mathf.CeilToInt(_horizontalPadding * _scrollrect.viewport.rect.width);
            _gridLayoutGroup.padding.top = Mathf.CeilToInt(_actualVerticalPadding);
            _gridLayoutGroup.padding.bottom = Mathf.CeilToInt(_actualVerticalPadding);

            _cellHeight = (_viewPortHeight
                - _gridLayoutGroup.padding.top
                - _gridLayoutGroup.padding.bottom
                - (_rowAmount - 1) * _actualVerticalSpacing)
                / _rowAmount;

            _gridLayoutGroup.cellSize = new Vector2(_cellHeight, _cellHeight);
        }

        private void AddFeatureCell(Sprite cellImage, AvatarPartInfo avatarPart)
        {
            GameObject gridCell = Instantiate(_gridCellPrefab, _featureGridContent);
            FeatureCellHandler handler = gridCell.GetComponent<FeatureCellHandler>();

            _playeravatar = _avatarEditorController.PlayerAvatar;
            string featureCategoryid = GetFeatureCategoryFromFeatureId(avatarPart.Id);
            int featurePickerPartSlot = _featureCategoryIdToFeatureSlotInt[featureCategoryid];

            handler.SetValues(cellImage, _highlightColor, _backgroundColor);

            AddListeners(handler, avatarPart, featurePickerPartSlot);

            if (avatarPart.Id == _playeravatar.GetPartId((FeatureSlot)featurePickerPartSlot))
            {
                _isSelectedFeature = true;
            }

            if (_isSelectedFeature)
            {
                handler.Highlight(true);
                _selectedCellHandler = handler;
                _isSelectedFeature = false;
            }
            else
            {
                handler.Highlight(false);
            }
        }

        private void UpdateHighlightedCell(FeatureCellHandler handler)
        {
            if (_selectedCellHandler != null)
            {
                _selectedCellHandler.Highlight(false);
            }
            
            _selectedCellHandler = handler;
            handler.Highlight(true);
        }

        private void AddListeners(FeatureCellHandler handler, AvatarPartInfo avatarPart, int featurePickerPartSlot)
        {
            handler.SetOnClick(onClick: () =>
            {
                _featureSetter.SetFeature(avatarPart, featurePickerPartSlot);
                UpdateHighlightedCell(handler);
            });
        }

        public void RefreshFeatureListItems(string categoryId)
        {
            // Don't Show Color Selection on hair
            if (categoryId == "10")
            {
                _colorSelection.gameObject.SetActive(false);
            }
            else
            {
                _colorSelection.gameObject.SetActive(true);
            }

            DestroyFeatureListItems();
            _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
            foreach (AvatarPartInfo part in _avatarPartInfo)
            {
                AddFeatureCell(part.IconImage, part);
            }
        }

        private void DestroyFeatureListItems()
        {
            foreach (Transform child in _featureGridContent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private string GetFeatureCategoryFromFeatureId(string featureId)
        {
            if (featureId.Length != 7)
            {
                Debug.LogError("featureId is invalid");
                return "";
            }
            else
            {
                return featureId.Substring(0, 2);
            }
        }
    }
}
