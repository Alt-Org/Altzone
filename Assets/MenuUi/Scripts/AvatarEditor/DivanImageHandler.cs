using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class DivanImageHandler : MonoBehaviour
    {
        [SerializeField] private ClassReference _classReference;
        [SerializeField] private Image _divanImage;

        public void UpdateDivanImage(PlayerData playerData)
        {
            if (playerData != null)
            {
                int classId = playerData.SelectedCharacterId;

                Color color = _classReference.GetColor(BaseCharacter.GetClass((CharacterID)classId));
                _divanImage.color = color;
            }
        }
    }
}
