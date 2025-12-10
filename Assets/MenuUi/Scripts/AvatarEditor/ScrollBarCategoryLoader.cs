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

    public class ScrollBarCategoryLoader : MonoBehaviour, IEndDragHandler
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

        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allCategoryIds;
        private float _cellHeight;
        private float _actualSpacing;
        private float _actualVerticalPadding;
        private float _viewPortHeight;
        private readonly int _cellsShownAtATime = 3;
        // Start is called before the first frame update
        void Start()
        {
            // Used to "start the scroll position not at the top"
            //_categoryGridContent.anchoredPosition = new Vector2(_categoryGrid.anchoredPosition.x, _categoryGrid.anchoredPosition.y + (_cellHeight + _actualSpacing) * 3);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ClampToCenter();
        }

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
                    Debug.LogError("is null");
                }
            }
        }

        private void ClampToCenter()
        {
            float totalCellSize = _cellHeight + _actualSpacing;

            float contentYPos = _categoryGridScrollRect.content.anchoredPosition.y;

            int nearestIndex = Mathf.RoundToInt(contentYPos / totalCellSize);

            float targetYPos = nearestIndex * totalCellSize;

            StartCoroutine(Clamp(targetYPos));
        }

        IEnumerator Clamp(float targetYPos)
        {
            Vector2 pos = _categoryGridScrollRect.content.anchoredPosition;
            Vector2 target = new(pos.x, targetYPos);

            float t = 0f;

            while (t < 1)
            {
                t += Time.deltaTime * 5f;
                _categoryGridScrollRect.content.anchoredPosition = Vector2.Lerp(pos, target, t);
                yield return null;
            }

            _categoryGridScrollRect.content.anchoredPosition = target;

            OnClamp(targetYPos);
        }

        private void OnClamp(float targetYPos)
        {
            float totalCellSize = _cellHeight + _actualSpacing;
            int topIndex = Mathf.RoundToInt(targetYPos / totalCellSize);
            int centerIndex = topIndex + 1;

            var centerCell = _categoryGridScrollRect.content.GetChild(centerIndex);

            centerCell.GetComponent<Button>().onClick.Invoke();
        }

        //Planning to add better way to add the category images later
        public void SetCategoryCells(Action<string> buttonFunction)
        {
            DestroyCategoryCells();
            _allCategoryIds = _avatarPartsReference.GetAllCategoryIds();

            foreach (string categoryId in _allCategoryIds)
            {
                _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
                AddCategoryCell(categoryId, _avatarPartInfo[0].IconImage, buttonFunction);
            }
        }

        private void AddCategoryCell(string FeatureCategoryId, Sprite CellImage, Action<string> buttonFunction)
        {
            GameObject gridCell = Instantiate(_avatarPartCategoryGridCellPrefab, _categoryGridContent);
            Image avatarpart = gridCell.transform.Find("FeatureImage").GetComponent<Image>();
            Button button = gridCell.GetComponent<Button>();

            avatarpart.sprite = CellImage;
            button.interactable = false;
            button.onClick.AddListener(() => buttonFunction.Invoke(FeatureCategoryId)); 
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
