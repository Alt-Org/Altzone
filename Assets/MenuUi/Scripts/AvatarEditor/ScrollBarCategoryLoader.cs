using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Altzone.Scripts.AvatarPartsInfo;
using NUnit.Framework.Constraints;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace MenuUi.Scripts.AvatarEditor
{

    public class ScrollBarCategoryLoader : MonoBehaviour, IEndDragHandler
    {
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private ScrollBarFeatureLoader _scrollBarFeatureLoader;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _avatarPartCategoryGridCellPrefab;
        private GameObject _emptyCell;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allCategoryIds;
        private float _cellHeight;
        [SerializeField] private RectTransform _CategoryGrid;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
        [SerializeField] private ScrollRect _scrollRect;
        private float _spacing = 0;
        private float _viewPortHeight;
        // Start is called before the first frame update
        void Start()
        {
            
            _allCategoryIds = _avatarPartsReference.GetAllCategoryIds();

            foreach (var categoryID in _allCategoryIds)
            {
                _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryID);
                AddCategoryCell(_avatarPartInfo[0].IconImage, categoryID);
            }
            UpdateCellSize();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ClampToCenter();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void AddCategoryCell(Sprite sprite, string categoryId)
        {
            GameObject _gridCell = Instantiate(_avatarPartCategoryGridCellPrefab, _content);
            Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
            _avatarPart.sprite = sprite;

            Button _button = _gridCell.GetComponent<Button>();
            if (_scrollBarFeatureLoader == null)
            {
                Debug.LogError("_scrollbarfeatureloader is null");
            }
            _button.onClick.AddListener(() => _scrollBarFeatureLoader.RefreshFeatureListItems(categoryId));
        }
        private void UpdateCellSize()
        {
            _viewPortHeight = _CategoryGrid.rect.height;
            _spacing = 0.05f * _viewPortHeight;
            _verticalLayoutGroup.spacing = _spacing;
            _verticalLayoutGroup.padding.left = Mathf.CeilToInt(0.1f * _CategoryGrid.rect.width);
            _verticalLayoutGroup.padding.right = Mathf.CeilToInt(0.1f * _CategoryGrid.rect.width);

            _cellHeight = (_viewPortHeight - _spacing * 2) / 3;
            foreach (RectTransform child in _content)
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
            float totalCellSize = _cellHeight + _spacing;

            float contentYPos = _scrollRect.content.anchoredPosition.y;

            int nearestIndex = Mathf.RoundToInt(contentYPos / totalCellSize);

            float targetYPos = nearestIndex * totalCellSize;

            StartCoroutine(Clamp(targetYPos));
        }

        IEnumerator Clamp(float targetYPos)
        {
            Vector2 pos = _scrollRect.content.anchoredPosition;
            Vector2 target = new(pos.x, targetYPos);

            float t = 0f;

            while (t < 1)
            {
                t += Time.deltaTime * 5f;
                _scrollRect.content.anchoredPosition = Vector2.Lerp(pos, target, t);
                yield return null;
            }

            _scrollRect.content.anchoredPosition = target;

            OnClamp(targetYPos);
        }

        private void OnClamp(float targetYPos)
        {
            float totalCellSize = _cellHeight + _spacing;
            int topIndex = Mathf.RoundToInt(targetYPos /totalCellSize);
            int centerIndex = topIndex + 1;

            var centerCell = _scrollRect.content.GetChild(centerIndex);

            centerCell.GetComponent<Button>().onClick.Invoke();
        }
    }
}
