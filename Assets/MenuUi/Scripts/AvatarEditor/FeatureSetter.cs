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
        [SerializeField] private AvatarVisualDataScriptableObject _avatarVisuals;

        public void SetFeature(AvatarPartInfo feature, AvatarPiece slot)
        {
            _avatarEditorController.PlayerAvatar.SetPart(slot, feature.Id);
            _avatarEditorCharacterHandle.SetMainCharacterImage(slot, feature, _avatarVisuals.GetColor(slot));
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
                    _avatarEditorCharacterHandle.SetMainCharacterImage(slot, avatarPart, _avatarVisuals.GetColor(slot));
                }
            }
        }

        public AvatarPartInfo GetCurrentlySelectedFeaturePartInfo(AvatarPiece pieceSlot)
        {
            string partId = _avatarEditorController.PlayerAvatar.GetPartId(pieceSlot);
            AvatarPartInfo partInfo = _avatarPartsReference.GetAvatarPartById(partId);
            return partInfo;
        }
    }
}
