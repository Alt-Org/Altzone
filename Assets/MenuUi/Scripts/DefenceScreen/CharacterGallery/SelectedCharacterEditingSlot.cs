using MenuUi.Scripts.Lobby.SelectedCharacters;
using UnityEngine;

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
        public BattlePopupSelectedCharacterView BattleView => _battleView;

    }
}
