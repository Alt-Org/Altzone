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
                // convert to base class id, 102 => 100 etc.
                int baseClassId = (classId / 100) * 100;

                Color color = _classReference.GetColor((CharacterClassType)baseClassId);
                _divanImage.color = color;
            }
        }
    }
}
