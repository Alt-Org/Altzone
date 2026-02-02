using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    /// <summary>
    /// Pure visuals for showing a selected character in a slot
    /// </summary>
    public class BattlePopupSelectedCharacterView : MonoBehaviour
    {
        [Header("Character slot references")]
        [SerializeField] private GameObject _backgroundPanel;
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _classColorBorderImage;
        [SerializeField] private Image _classColorImage;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] public Image _cornerIcon;
        [SerializeField] public Image _resistanceIcon;

        private CharacterID _characterId;

        public void SetInfo(Sprite galleryImage, CharacterID charID, int[] stats = null)
        {
            if (_backgroundPanel != null) _backgroundPanel.SetActive(true);

            _spriteImage.sprite = galleryImage;
            _spriteImage.enabled = true;

            CharacterClassType charClassType = CustomCharacter.GetClass(charID);

            if (_classColorBorderImage != null)
            {
                _classColorBorderImage.enabled = true;
                _classColorBorderImage.sprite = ClassReference.Instance.GetFrame(charClassType);
            }

            if (_classColorImage != null)
                _classColorImage.color = ClassReference.Instance.GetColor(charClassType);

            if (_cornerIcon != null)
            {
                _cornerIcon.enabled = true;
                _cornerIcon.sprite = ClassReference.Instance.GetCornerIcon(charClassType);
            }

            if (_resistanceIcon != null)
            {
                _resistanceIcon.enabled = true;
                _resistanceIcon.sprite = ClassReference.Instance.GetResistanceIcon(charClassType);
            }

            _characterId = charID;

            if (_piechartPreview == null) return;

            if (stats != null)
                _piechartPreview.UpdateChart(stats[3], stats[0], stats[4], stats[2], stats[1]);
            else
                _piechartPreview.UpdateChart(_characterId);
        }

        public void SetEmpty()
        {
            if (_backgroundPanel != null) _backgroundPanel.SetActive(false);
            _spriteImage.enabled = false;
            if (_classColorBorderImage != null) _classColorBorderImage.enabled = false;
            if (_classColorImage != null) _classColorImage.color = Color.gray;
            if (_cornerIcon != null) _cornerIcon.enabled = false;
            if (_resistanceIcon != null) _resistanceIcon.enabled = false;

            if (_piechartPreview != null) _piechartPreview.ClearChart();
            _characterId = CharacterID.None;
        }
    }
}
