using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    public class BattlePopupSelectedCharacter : MonoBehaviour
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private PieChartPreview _piechartPreview;

        private CharacterID _characterId;

        public void SetInfo(Sprite galleryImage, Color backgroundColor, CharacterID charID)
        {
            _spriteImage.sprite = galleryImage;
            _backgroundImage.color = backgroundColor;
            _characterId = charID;
            _piechartPreview.UpdateChart(_characterId);
        }
    }
}
