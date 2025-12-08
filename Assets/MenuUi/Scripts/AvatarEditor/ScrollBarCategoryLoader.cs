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
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _avatarPartCategoryGridCellPrefab;
        private GameObject _emptyCell;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allCategoryIds;
        // Start is called before the first frame update
        void Start()
        {
            _allCategoryIds = _avatarPartsReference.GetAllCategoryIds();

            foreach (var categoryID in _allCategoryIds)
            {
                _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryID);
                AddCategoryCell(_avatarPartInfo[0].IconImage, categoryID);
            }
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
    }
}
