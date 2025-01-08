using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    [RequireComponent(typeof(Button))]
    public class CharacterGalleryCharacterStatWindowToShowButton : NaviButton
    {
        private CharacterID CharacterStatWindowToShowValue;

        protected override void OnNaviButtonClick()
        {
            IGalleryCharacterData data = GetComponent<IGalleryCharacterData>();

            CharacterStatWindowToShowValue = data.Id;

            SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow = CharacterStatWindowToShowValue;
            base.OnNaviButtonClick();
            Debug.Log("changed to " + CharacterStatWindowToShowValue);
        }
    }
}


