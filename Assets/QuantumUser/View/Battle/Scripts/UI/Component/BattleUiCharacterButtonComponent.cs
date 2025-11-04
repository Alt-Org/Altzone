/// @file BattleUiCharacterButtonComponent.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiCharacterButtonComponent} class which handles character button visuals and component references.
/// </summary>
///
/// This script:<br/>
/// Handles a BattleUiPlayerInfo prefab's character button visuals and component references.

// Unity usings
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Altzone usings
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
        /// @anchor BattleUiCharacterButtonComponent-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the button component of the character button.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private Button _button;

        /// <summary>[SerializeField] Reference OnPointerDownButton event sender.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private OnPointerDownButton _eventSender;

        /// <summary>[SerializeField] Reference to the character image of the character button.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private Image _characterImage;

        /// <summary>[SerializeField] Reference to the selected character indicator of the character button.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private GameObject _selectedCharacterIndicator;

        /// <summary>[SerializeField] Reference to the damage fill image of the character button. It is used to display the character's current Hp.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private Image _damageFill;

        /// <summary>[SerializeField] Reference to the defence value text of the character button.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private TextMeshProUGUI _defenceValue;

        /// <summary>[SerializeField] The duration for damage fill animation.</summary>
        /// @ref BattleUiCharacterButtonComponent-SerializeFields
        [SerializeField] private float _damageFillAnimationDuration = 0.5f;

        /// @}

        /// <value>Public getter for #_button.</value>
        public Button ButtonComponent => _button;

        /// <value>Public getter for #_eventSender.</value>
        public OnPointerDownButton EventSender => _eventSender;

        /// <summary>
        /// Sets the character image from PlayerCharacterPrototype to #_characterImage.
        /// </summary>
        ///
        /// <param name="characterId">The CharacterId of the character as a int.</param>
        public void SetCharacterIcon(int characterId)
        {
            PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(characterId.ToString());

            if (info == null) return;

            Sprite characterSprite = info.BattleUiSprite;

            if (characterSprite == null)
            {
                characterSprite = info.GalleryHeadImage;
            }

            _characterImage.sprite = characterSprite;
        }

        /// <summary>
        /// Sets the selected character indicator for this character button active or inactive
        /// </summary>
        ///
        /// <param name="selected">Whether this button's corresponding character was selected or unselected.</param>
        public void SetSelected(bool selected)
        {
            _selectedCharacterIndicator.SetActive(selected);
        }

        /// <summary>
        /// Sets the damage fill variables to start updating damage fill to the updated percentage.
        /// </summary>
        ///
        /// <param name="percentage">The updated percentage for damage fill.</param>
        public void SetDamageFill(float percentage)
        {
            _t = 0f;
            _startDamageFillAmount = _damageFill.fillAmount;
            _targetDamageFillAmount = 1 - percentage;
        }

        /// <summary>
        /// Sets the number on the UI to match the defence value.<br/>
        /// The number does not go below 0.
        /// </summary>
        ///
        /// <param name="defenceValue">The defence value of the character.</param>
        public void SetDefenceNumber(float defenceValue)
        {
            defenceValue = Mathf.Max(defenceValue, 0f);
            _defenceValue.text = defenceValue.ToString();
        }

        /// <value>The starting damage fill amount for the fill animation.</value>
        private float _startDamageFillAmount;

        /// <value>The target damage fill amount for the fill animation.</value>
        private float _targetDamageFillAmount = 0f;

        /// <value>The damage fill animation time passed.</value>
        private float _t = 0f;


        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles progressing the damage fill animation.
        /// </summary>
        private void Update()
        {
            if (_targetDamageFillAmount > _damageFill.fillAmount)
            {
                _t += Time.deltaTime / _damageFillAnimationDuration;
                _damageFill.fillAmount = Mathf.Lerp(_startDamageFillAmount, _targetDamageFillAmount, _t);
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
