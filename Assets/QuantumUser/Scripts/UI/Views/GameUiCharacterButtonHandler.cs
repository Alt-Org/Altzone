using Altzone.Scripts.ModelV2;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui character button visuals and component references.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GameUiCharacterButtonHandler : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private Image _damageFill;
        [SerializeField] private Image _shieldFill;

        [HideInInspector] public Button ButtonComponent;

        private void OnEnable()
        {
            ButtonComponent = GetComponent<Button>();
        }

        public void SetCharacterIcon(int characterId)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());

            if (info == null) return;

            _characterImage.sprite = info.GalleryImage;
        }
    }
}

