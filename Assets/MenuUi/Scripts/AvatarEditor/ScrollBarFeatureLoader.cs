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
        [SerializeField] private Transform _featureGridContent;
        [SerializeField] private GameObject _gridCellPrefab;
        [SerializeField] private FeaturePicker _featurePicker;

        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allAvatarCategoryIds;
        private Dictionary<string, int> _featureCategoryIdToFeatureSlotInt;

        // Start is called before the first frame update
        void Start()
        {
            // This feels stupid, but the old code uses ints that don't seem related to the feature slot id:s in any way
            // to set the features so I can't figure out a better way to do this for now.
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

            _allAvatarCategoryIds = _avatarPartsReference.GetAllCategoryIds();
            // Load default features for featuregrid
            if (_allAvatarCategoryIds != null)
                RefreshFeatureListItems(_allAvatarCategoryIds[0]);
        }

        private void AddFeatureCell(Sprite sprite, AvatarPartInfo part)
        {
            GameObject _gridCell = Instantiate(_gridCellPrefab, _featureGridContent);
            Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
            // Need to find a way to make sprites look same size in grid
            _avatarPart.sprite = sprite;

            Button _button = _gridCell.GetComponent<Button>();
                                                // This could propably be more readable
            _button.onClick.AddListener(() => _featurePicker.SetFeature(part, _featureCategoryIdToFeatureSlotInt[GetFeatureCategoryFromFeatureId(part.Id)]));
        }

        public void RefreshFeatureListItems(string categoryId)
        {
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
            if (featureId == null)
            {
                Debug.LogError("featureId is null");
                return "";
            }
            else
            {
                return featureId.Substring(0, 2);
            }
        }
    }
}
