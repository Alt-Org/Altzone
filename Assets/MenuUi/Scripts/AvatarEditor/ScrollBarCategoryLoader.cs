using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Altzone.Scripts.AvatarPartsInfo;
using NUnit.Framework.Constraints;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Composites;
using System;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ScrollBarCategoryLoader : MonoBehaviour
    {
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private RectTransform _categoryGridContent;
        [SerializeField] private GameObject _avatarPartCategoryGridCellPrefab;
        [SerializeField] private RectTransform _categoryGrid;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
        [SerializeField] private ScrollRect _categoryGridScrollRect;
        [SerializeField, Range(0f, 0.3f)] private float _horizontalPadding = 0.1f;
        [SerializeField, Range(0f, 0.3f)] private float _spacing = 0.05f;
        [SerializeField, Range(0f, 0.2f)] private float _verticalPadding = 0.05f;
        [SerializeField] private Color _backgroundColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allCategoryIds;
        private float _cellHeight;
        private float _actualSpacing;
        private float _actualVerticalPadding;
        private float _viewPortHeight;
        private int _uniqueCellAmount;
        private readonly int _cellsShownAtATime = 3;
        public float cellHeight => _cellHeight;
        public float spacing => _actualSpacing;
        public int uniqueCellAmount => _uniqueCellAmount;
        private string _currentlySelectedCategory = "10";
        public string CurrentlySelectedCategory => _currentlySelectedCategory;

        public void UpdateCellSize()
        {
            _viewPortHeight = _categoryGrid.rect.height;
            _actualSpacing = _spacing * _viewPortHeight;
            _actualVerticalPadding = _verticalPadding * _categoryGrid.rect.height;
            _verticalLayoutGroup.spacing = _actualSpacing;
            _verticalLayoutGroup.padding.left = Mathf.CeilToInt(_horizontalPadding * _categoryGrid.rect.width);
            _verticalLayoutGroup.padding.right = Mathf.CeilToInt(_horizontalPadding * _categoryGrid.rect.width);
            _verticalLayoutGroup.padding.top = Mathf.CeilToInt(_actualVerticalPadding);
            _verticalLayoutGroup.padding.bottom = Mathf.CeilToInt(_actualVerticalPadding);

            _cellHeight = (_viewPortHeight - _actualSpacing * (_cellsShownAtATime -1) - _verticalLayoutGroup.padding.top - _verticalLayoutGroup.padding.bottom) / _cellsShownAtATime;
            foreach (RectTransform child in _categoryGridContent)
            {
                LayoutElement le = child.GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredHeight = _cellHeight;
                }
                else
                {
                    Debug.LogError("layoutelement is null");
                }
            }

            _uniqueCellAmount = _categoryGridContent.transform.childCount / 3;

            // Grid position starts in the middle
            SetGridPosition();
        }

        private void SetGridPosition()
        {
            _categoryGridContent.anchoredPosition = new Vector2(_categoryGrid.anchoredPosition.x,
                _categoryGrid.anchoredPosition.y
                + (_cellHeight + _actualSpacing)
                * _uniqueCellAmount);
        }

        public void ClickMiddleCategoryCell()
        {
            float CellHeightWithSpacing = _cellHeight + _actualSpacing;
            int topCellIndex = Mathf.RoundToInt(_categoryGridContent.anchoredPosition.y / CellHeightWithSpacing);
            int middleCellIndex = topCellIndex + _cellsShownAtATime / 2;

            Transform middleCell = _categoryGridContent.GetChild(middleCellIndex);
            Button button = middleCell.GetComponent<Button>();
            button.onClick.Invoke();
        }

        //Planning to add better way to add the category images later
        public void SetCategoryCells(Action<string> buttonFunction)
        {
            DestroyCategoryCells();
            _allCategoryIds = _avatarPartsReference.GetAllCategoryIds();

            //make 3 sets of the available features for InfiniteScroll to work
            for (int i = 0; i < 3; i++)
            {
                foreach (string categoryId in _allCategoryIds)
                {
                    _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
                    AddCategoryCell(categoryId, _avatarPartInfo[0].IconImage, buttonFunction);
                }
            }
        }

        private void AddCategoryCell(string FeatureCategoryId, Sprite CellImage, Action<string> buttonFunction)
        {
            GameObject gridCell = Instantiate(_avatarPartCategoryGridCellPrefab, _categoryGridContent);
            GridCellHandler handler = gridCell.GetComponent<GridCellHandler>();

            handler.SetValues(CellImage, _backgroundColor, () =>
            {
                buttonFunction.Invoke(FeatureCategoryId);
                _currentlySelectedCategory = FeatureCategoryId;
            },
                false);
        }

        private void DestroyCategoryCells()
        {
            foreach (Transform child in _categoryGridContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
