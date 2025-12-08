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
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _gridCellPrefab;
        [SerializeField] private Sprite _testSprite;
        [SerializeField] private FeaturePicker _featurePicker;
        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allAvatarCategoryIds;

        // Start is called before the first frame update
        void Start()
        {
            _allAvatarCategoryIds = _avatarPartsReference.GetAllCategoryIds();
            // Load default features for featuregrid
            if (_allAvatarCategoryIds != null)
                RefreshFeatureListItems(_allAvatarCategoryIds[0]);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {

        }

        private void AddFeatureCell(Sprite sprite, AvatarPartInfo part)
        {
            GameObject _gridCell = Instantiate(_gridCellPrefab, _content);
            Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
            // Need to find a way to make sprites look same size in grid
            _avatarPart.sprite = sprite;

            Button _button = _gridCell.GetComponent<Button>();

            _button.onClick.AddListener(() => _featurePicker.SetFeature(part, 0));
        }

        public void RefreshFeatureListItems(string categoryId)
        {
            DestroyFeatureListItems();
            _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
            foreach (var part in _avatarPartInfo)
            {
                AddFeatureCell(part.IconImage, part);
            }
        }

        private void DestroyFeatureListItems()
        {
            foreach (Transform child in _content.transform)
            {
                Destroy(child.gameObject);
            }
        }  
    }
}
