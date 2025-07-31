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

        [SerializeField] private float _damageFillAnimationDuration = 0.5f;

        private float _startDamageFillAmount;
        private float _targetDamageFillAmount = 0f;
        private float t;

        public Button ButtonComponent => _button;
        public OnPointerDownButton EventSender => _eventSender;

        public void SetCharacterIcon(int characterId)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());
            
            if (info == null) return;

            Sprite characterSprite = info.BattleUiSprite;

            if (characterSprite == null)
            {
                characterSprite = info.GalleryImage;
            }

            _characterImage.sprite = characterSprite;
        }

        public void SetDamageFill(float percentage)
        {
            _startDamageFillAmount = _damageFill.fillAmount;
            _targetDamageFillAmount = 1 - percentage;
        }

        private void Update()
        {
            if (_targetDamageFillAmount > _damageFill.fillAmount)
            {
                t += Time.deltaTime / _damageFillAnimationDuration;
                _damageFill.fillAmount = Mathf.Lerp(_startDamageFillAmount, _targetDamageFillAmount, t);
            }
        }

        private void OnDisable()
        {
            ButtonComponent.onClick.RemoveAllListeners();
            EventSender.onClick.RemoveAllListeners();
        }
    }
}
