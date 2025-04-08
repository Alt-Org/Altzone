using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterStatsWindow;
using UnityEngine.UI;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    [RequireComponent(typeof(Button))]
    public class StatPopUpToShowButton : MonoBehaviour
    {
        private CharacterID CharacterStatWindowToShowValue;

        private StatsWindowController _controller;
        private Button _button;

        private void OnEnable()
        {
            if (_controller == null) _controller = FindObjectOfType<StatsWindowController>(true);
            if (_button == null)
            {
                _button = GetComponent<Button>();
                _button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        protected void OnButtonClick()
        {
            IGalleryCharacterData data = GetComponent<IGalleryCharacterData>();

            CharacterStatWindowToShowValue = data.Id;

            SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow = CharacterStatWindowToShowValue;

            _controller.OpenPopup();
        }
    }
}
