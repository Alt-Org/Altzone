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
        [SerializeField] private Color _highlightColor = new(0f, 0f, 0f, 0.5f);
        [SerializeField] private Color _backgroundColor = new(168f, 168f, 168f, 0.7f);
        [SerializeField] private AvatarEditorController _avatarEditorController;

        private List<AvatarPartInfo> _avatarPartInfo;
        private List<string> _allAvatarCategoryIds;
        private Dictionary<string, int> _featureCategoryIdToFeatureSlotInt;
        private bool _isSelectedFeature = false;
        private PlayerAvatar _playeravatar;
        private Image _selectedCellImage;
        

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
        }

        private void AddFeatureCell(Sprite cellImage, AvatarPartInfo avatarPart)
        {
            GameObject gridCell = Instantiate(_gridCellPrefab, _featureGridContent);
            Image avatarPartImage = gridCell.transform.Find("FeatureImage").GetComponent<Image>();
            Image gridCellBackgroundImage = gridCell.transform.Find("BackgroundImage").GetComponent<Image>();
            Button button = gridCell.GetComponent<Button>();
            _playeravatar = _avatarEditorController.PlayerAvatar;
            string featureCategoryid = GetFeatureCategoryFromFeatureId(avatarPart.Id);
            int featurePickerPartSlot = _featureCategoryIdToFeatureSlotInt[featureCategoryid];
            // Need to find a way to make sprites look same size in grid
            avatarPartImage.sprite = cellImage;

            AddListeners(button, avatarPart, featurePickerPartSlot, gridCellBackgroundImage);

            if (avatarPart.Id == _playeravatar.GetPartId((FeatureSlot)featurePickerPartSlot))
            {
                _isSelectedFeature = true;
            }

            if (_isSelectedFeature)
            {
                gridCellBackgroundImage.color = _highlightColor;
                _isSelectedFeature = false;
                _selectedCellImage = gridCellBackgroundImage;
            }
            else
                gridCellBackgroundImage.color = _backgroundColor;
        }

        private void UpdateHighlightedCell(Image NewSelectedCellImage)
        {
            if (_selectedCellImage != null)
            {
                _selectedCellImage.color = _backgroundColor;
                _selectedCellImage = NewSelectedCellImage;
                _selectedCellImage.color = _highlightColor;
            }
        }

        private void AddListeners(Button button, AvatarPartInfo avatarPart, int featurePickerPartSlot, Image gridCellBackgroundImage)
        {
            button.onClick.AddListener(() =>
            {
                _featurePicker.SetFeature(avatarPart, featurePickerPartSlot);
                UpdateHighlightedCell(gridCellBackgroundImage);
            });
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
