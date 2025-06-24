using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.ModelV2;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles Battle Ui character button visuals and component references.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattleUiCharacterButtonComponent : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private OnPointerDownButton _eventSender;
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Image _shieldFill;

        public Button ButtonComponent => _button;
        public OnPointerDownButton EventSender => _eventSender;

        public void SetCharacterIcon(int characterId)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());

            if (info == null) return;

            _characterImage.sprite = info.GalleryImage;
        }

        private void OnDisable()
        {
            ButtonComponent.onClick.RemoveAllListeners();
            EventSender.onClick.RemoveAllListeners();
        }
    }
}
