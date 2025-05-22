using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.Signals;
using System;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    /// <summary>
    /// Added to the individual selected character slots in Battle Popup SelectedCharacters prefab.
    /// Handles setting the visuals for the slot, and has functionality for the dropdown from which you can select a new defence character.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattlePopupSelectedCharacter : AltMonoBehaviour
    {
        [Header("Character slot references")]
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _bordersBackgroundImage;
        [SerializeField] private Image _upperBackgroundImage;
        [SerializeField] private Image _lowerBackgroundImage;
        [SerializeField] private PieChartPreview _piechartPreview;

        [Header("Reference sheet")]
        [SerializeField] private ClassColorReference _classColorReference;

        private CharacterID _characterId;

        private Button _button;
        public Button ButtonComponent => _button;


        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }


        // Method for adding the edit panel listener because in KotiView the button should open defence gallery using SetMainMenuWindowIndex script instead
        public void SetOpenEditPanelListener() 
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(() => SignalBus.OnDefenceGalleryEditPanelRequestedSignal());
        }


        /// <summary>
        /// Set character info to the battle popup selected character slot.
        /// </summary>
        /// <param name="galleryImage">Character's gallery sprite.</param>
        /// <param name="charID">Character id.</param>
        /// <param name="isEditable">If the character is editable for the local player.</param>
        /// <param name="stats">The stats for the character in an int array. Order: Hp, Speed, CharacterSize, Attack, Defence.</param>
        public void SetInfo(Sprite galleryImage, CharacterID charID, bool isEditable, int[] stats = null)
        {
            _spriteImage.sprite = galleryImage;
            _spriteImage.enabled = true;

            //CharacterClassID charClassID = CustomCharacter.GetClassID(charID);
            //_upperBackgroundImage.color = _classColorReference.GetAlternativeColor(charClassID);
            //_lowerBackgroundImage.color = _classColorReference.GetColor(charClassID);

            _characterId = charID;

            if (_button == null) _button = GetComponent<Button>();
            _button.enabled = isEditable;

            if (stats != null)
            {
                _piechartPreview.UpdateChart(stats[3], stats[0], stats[4], stats[2], stats[1]);
            }
            else
            {
                _piechartPreview.UpdateChart(_characterId);
            }
        }


        /// <summary>
        /// Set character slot showing as empty.
        /// </summary>
        /// <param name="isEditable">If slot is editable for the local player or not.</param>
        /// <param name="slotIdx">Slot's index.</param>
        public void SetEmpty(bool isEditable)
        {
            _spriteImage.enabled = false;
            //_upperBackgroundImage.color = Color.white;
            //_lowerBackgroundImage.color = Color.white;

            _piechartPreview.ClearChart();
            _characterId = CharacterID.None;

            if (_button == null) _button = GetComponent<Button>();
            _button.enabled = isEditable;
        }
    }
}
