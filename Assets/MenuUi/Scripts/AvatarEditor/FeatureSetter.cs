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

        public void SetFeature(AvatarPartInfo feature, AvatarPiece slot)
        {
            _avatarEditorController.PlayerAvatar.SetPart(slot, feature.Id);
            _avatarEditorCharacterHandle.SetMainCharacterImage(slot, feature);
        }

        public void SetLoadedFeatures(PlayerAvatar avatar)
        {
            foreach (AvatarPiece slot in Enum.GetValues(typeof(AvatarPiece)))
            {
                string partId = avatar.GetPartId(slot);

                if (string.IsNullOrEmpty(partId))
                {
                    continue;
                }

                AvatarPartInfo avatarPart = _avatarPartsReference.GetAvatarPartById(partId);

                if (avatarPart != null)
                {
                    _avatarEditorCharacterHandle.SetMainCharacterImage(slot, avatarPart);
                }
            }
        }

        public Sprite GetCurrentlySelectedFeatureSprite(AvatarPiece pieceSlot)
        {
            string partId = _avatarEditorController.PlayerAvatar.GetPartId(pieceSlot);
            Sprite partSprite = _avatarPartsReference.GetAvatarPartById(partId).AvatarImage;
            return partSprite;
        }
    }
}
