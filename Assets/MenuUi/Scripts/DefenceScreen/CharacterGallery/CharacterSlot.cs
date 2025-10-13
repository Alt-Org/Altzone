using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Handles the visual functionality of CharacterSlot.
    /// Has a reference to GalleryCharacter, and the info to GalleryCharacter is also set through SetInfo function.
    /// Inherits SlotBase for editing selected characters.
    /// </summary>
    public class CharacterSlot : SlotBase
    {
        [SerializeField] public GalleryCharacter Character;

        [HideInInspector] public bool IsLocked = false;
        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, string className, Sprite classIcon, CharacterID id)
        {
            _id = id;
            Character.SetInfo(sprite, bgColor, bgAltColor, name, className, id, this);
        }
    }
}
