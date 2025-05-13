
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SelectedCharacterEditingSlot : SlotBase
    {
        [HideInInspector] public int SlotIndex = 0;
        [HideInInspector] public GalleryCharacter SelectedCharacter = null;
    }
}
