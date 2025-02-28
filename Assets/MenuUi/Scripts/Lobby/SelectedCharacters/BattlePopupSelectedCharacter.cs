using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    [RequireComponent(typeof(Button))]
    public class BattlePopupSelectedCharacter : MonoBehaviour
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] private GameObject _selectionDropdown;
        [SerializeField] private Transform _selectionDropdownContent;

        private CharacterID _characterId;
        private Button _button;
        private bool _isOwnCharacter;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_isOwnCharacter)
            {
                _button.onClick.AddListener(OpenSelectionDropdown);
            }
        }


        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }


        public void SetInfo(Sprite galleryImage, Color backgroundColor, CharacterID charID, bool isOwnCharacter)
        {
            _spriteImage.sprite = galleryImage;
            _backgroundImage.color = backgroundColor;
            _characterId = charID;
            _piechartPreview.UpdateChart(_characterId);
            _isOwnCharacter = isOwnCharacter;
        }


        private void OpenSelectionDropdown()
        {

        }
    }
}
