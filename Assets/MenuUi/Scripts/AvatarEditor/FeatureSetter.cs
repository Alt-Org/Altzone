using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Altzone.Scripts.AvatarPartsInfo;
using Altzone.Scripts.Model.Poco.Game;
using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeatureSetter : MonoBehaviour
    {

        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        private List<string> _selectedFeatures = new List<string>(new string[7]);

        public void SetFeature(AvatarPartInfo feature, int slot)
        {
            SetSelectedFeature(slot, feature.Id);
            _avatarEditorController.PlayerAvatar.SortAndAssignByID(feature.Id);
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, feature.AvatarImage);
        }


        public string GetSelectedFeature(int index) =>
            index >= 0 && index < _selectedFeatures.Count ? _selectedFeatures[index] : null;

        public void SetSelectedFeature(int index, string value)
        {
            if (index >= 0 && index < _selectedFeatures.Count)
                _selectedFeatures[index] = value;
        }

        public void SetLoadedFeatures(PlayerAvatar avatar)
        {
            var featureTypes = Enum.GetValues(typeof(FeatureSlot));
            foreach (FeatureSlot feature in featureTypes)
            {
                string partId = avatar.GetPartId(feature);

                if (string.IsNullOrEmpty(partId))
                {
                    SetSelectedFeature((int)feature, "0");
                    continue;
                }

                var featureData = GetSpritesByCategory(feature);

                var part = featureData.Find(p => p.Id == partId);
                if (part == null)
                {
                    SetSelectedFeature((int)feature, "0");
                }
                else
                {
                    SetFeature(part, (int)feature);
                }
            }
        }

        private List<AvatarPartInfo> GetSpritesByCategory(FeatureSlot slot)
        {
            if (_avatarPartsReference == null)
                return new List<AvatarPartInfo>();

            var categoryId = CategoryController.GetCategoryId(slot);
            return string.IsNullOrEmpty(categoryId)
                ? new List<AvatarPartInfo>()
                : _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
        }

        public Sprite GetCurrentlySelectedFeatureSprite(AvatarPiece pieceSlot)
        {
            var featureData = GetSpritesByCategory((FeatureSlot)pieceSlot);
            var featureId = GetSelectedFeature((int)pieceSlot);

            if (string.IsNullOrEmpty(featureId)) return null;

            return featureData.Find(p => p.Id == featureId)?.AvatarImage;
        }
    }

    public class CategoryController
    {

        private static readonly Dictionary<FeatureSlot, string> CategoryIds = new()
        {
            { FeatureSlot.Hair, "10" }, { FeatureSlot.Eyes, "21" }, { FeatureSlot.Nose, "22" },
            { FeatureSlot.Mouth, "23" }, { FeatureSlot.Body, "31" }, { FeatureSlot.Hands, "32" },
            { FeatureSlot.Feet, "33" }
        };

        public event Action<FeatureSlot> OnCategoryChanged;


        public static string GetCategoryId(FeatureSlot slot) =>
            CategoryIds.TryGetValue(slot, out var id) ? id : null;
    }
}
