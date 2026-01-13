using System;
using Altzone.Scripts.AvatarPartsInfo;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeatureSetter : MonoBehaviour
    {

        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
        [SerializeField] private AvatarPartsReference _avatarPartsReference;

        public void SetFeature(AvatarPartInfo feature, int slot)
        {
            _avatarEditorController.PlayerAvatar.SortAndAssignByID(feature.Id);
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, feature.AvatarImage);
        }

        public void SetLoadedFeatures(PlayerAvatar avatar)
        {
            foreach (FeatureSlot slot in Enum.GetValues(typeof(FeatureSlot)))
            {
                string partId = avatar.GetPartId(slot);

                if (string.IsNullOrEmpty(partId))
                {
                    continue;
                }

                AvatarPartInfo avatarPart = _avatarPartsReference.GetAvatarPartById(partId);

                if (avatarPart != null)
                {
                    _avatarEditorCharacterHandle.SetMainCharacterImage(slot, avatarPart.AvatarImage);
                }
            }
        }

        public Sprite GetCurrentlySelectedFeatureSprite(AvatarPiece pieceSlot)
        {
            string partId = _avatarEditorController.PlayerAvatar.GetPartId((FeatureSlot)pieceSlot);
            Sprite partSprite = _avatarPartsReference.GetAvatarPartById(partId).AvatarImage;
            return partSprite;
        }
    }
}
