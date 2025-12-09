using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Altzone.Scripts.AvatarPartsInfo;
using NUnit.Framework.Constraints;

namespace MenuUi.Scripts.AvatarEditor
{

    public class ScrollBarCategoryLoader : MonoBehaviour
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
        public void UpdateCellSize()
        {
            float viewPortHeight = _CategoryGrid.rect.height;
            float spacing = 0.05f * viewPortHeight;
            _verticalLayoutGroup.spacing = spacing;
            _verticalLayoutGroup.padding.left = Mathf.CeilToInt(0.1f * _CategoryGrid.rect.width);
            _verticalLayoutGroup.padding.right = Mathf.CeilToInt(0.1f * _CategoryGrid.rect.width);

            _cellHeight = (viewPortHeight - spacing * 2) / 3;
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
    }
}
