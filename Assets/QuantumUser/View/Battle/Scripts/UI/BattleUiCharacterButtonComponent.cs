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
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Image _shieldFill;

        private Button _button;
        public Button ButtonComponent {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }
                return _button;
            }
        }

        private void OnDisable()
        {
            ButtonComponent.onClick.RemoveAllListeners();
        }

        public void SetCharacterIcon(int characterId)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());

            if (info == null) return;

            _characterImage.sprite = info.GalleryImage;
        }
    }
}
