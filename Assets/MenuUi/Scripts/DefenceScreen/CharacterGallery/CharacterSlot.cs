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

        private string _name = string.Empty;

        [HideInInspector] public bool IsLocked = false;
        [HideInInspector] public bool IsUsed = false;

        public string Name { get => _name;}

        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, string className, Sprite classIcon, CharacterID id)
        {
            _id = id;
            _name = name;
            Character.SetInfo(sprite, bgColor, bgAltColor, name, className, id, this);
        }

        public void GradientOverride(Sprite bgImage)
        {
            Character.GradientOverride(bgImage);
        }
    }
}
