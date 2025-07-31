/// @file BattleUiCharacterButtonComponent.cs
/// <summary>
/// Has a class BattleUiCharacterButtonComponent which handles character button visuals and component references.
/// </summary>
///
/// This script:<br/>
/// Handles a BattleUiPlayerInfo prefab's character button visuals and component references.

using UnityEngine;
using UnityEngine.UI;

using Altzone.Scripts.ModelV2;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">CharacterButton @uicomponentlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles a BattleUiPlayerInfo prefab's character button visuals and component references. Attached to each of BattleUiPlayerInfo prefab's character button <a href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObjects@u-exlink</a>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattleUiCharacterButtonComponent : MonoBehaviour
    {
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to the button component of the character button.</value>
        [SerializeField] private Button _button;
        [SerializeField] private OnPointerDownButton _eventSender;

        /// <value>[SerializeField] Reference to the character image of the character button.</value>
        [SerializeField] private Image _characterImage;

        /// <value>[SerializeField] Reference to the damage fill image of the character button. It is used to display the character's current Hp.</value>
        [SerializeField] private Image _damageFill;

        /// <value>[SerializeField] Reference to the shield fill image of the character button. It is used to display the character's current shield health.</value>
        [SerializeField] private Image _shieldFill;
        
        [SerializeField] private float _damageFillAnimationDuration = 0.5f;

        /// @}

        private float _startDamageFillAmount;
        private float _targetDamageFillAmount = 0f;
        private float t = 0f;

        /// <value>Public getter for #_button.</value>
        public Button ButtonComponent => _button;
        public OnPointerDownButton EventSender => _eventSender;

        /// <summary>
        /// Sets the character image from PlayerCharacterPrototype to #_characterImage.
        /// </summary>
        /// <param name="characterId">The CharacterId of the character as a int.</param>
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
            t = 0f;
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

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnDisable.html">OnDisable@u-exlink</a> function. Removed all listeners from #_button's <a href="https://docs.unity3d.com/530/Documentation/ScriptReference/UI.Button-onClick.html">onClick@u-exlink</a> event.
        /// </summary>
        private void OnDisable()
        {
            ButtonComponent.onClick.RemoveAllListeners();
            EventSender.onClick.RemoveAllListeners();
        }
    }
}
