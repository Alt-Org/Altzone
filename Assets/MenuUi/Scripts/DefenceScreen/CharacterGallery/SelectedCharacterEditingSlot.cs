using MenuUi.Scripts.Lobby.SelectedCharacters;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Holds the info which a SelectedCharacterEditingSlot needs, a slot's index and the currently selected character in this slot.
    /// </summary>
    public class SelectedCharacterEditingSlot : SlotBase
    {
        [HideInInspector] public int SlotIndex = 0;
        [HideInInspector] public GalleryCharacter SelectedCharacter = null;

        [SerializeField] private BattlePopupSelectedCharacterView _battleView;
        [SerializeField] private Image _emptyBackgroundImage;
        public BattlePopupSelectedCharacterView BattleView => _battleView;

        private Sprite _defaultEmptyBgSprite;
        private Color _defaultEmptyBgColor;
        private bool _defaultEmptyBgCached;

        private void CacheDefaultEmptyBackground()
        {
            if (_defaultEmptyBgCached || _emptyBackgroundImage == null) return;
            _defaultEmptyBgSprite = _emptyBackgroundImage.sprite;
            _defaultEmptyBgColor = _emptyBackgroundImage.color;
            _defaultEmptyBgCached = true;
        }

        public void SetEmptyGradientOverride(Sprite bgImage)
        {
            if (_emptyBackgroundImage == null) return;

            CacheDefaultEmptyBackground();

            if (bgImage == null)
            {
                _emptyBackgroundImage.sprite = _defaultEmptyBgSprite;
                _emptyBackgroundImage.color = _defaultEmptyBgColor;
                return;
            }

            _emptyBackgroundImage.sprite = bgImage;
            _emptyBackgroundImage.color = Color.white;

        }
    }
}
