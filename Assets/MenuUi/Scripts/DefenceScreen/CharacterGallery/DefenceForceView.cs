using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Control the visual functionality for DefenceForce tab in defence gallery
    /// </summary>
    public class DefenceForceView : MonoBehaviour
    {
        [SerializeField] private GalleryView _galleryView;
        [SerializeField] private SelectedCharSlot[] _selectedCharSlots;

        [SerializeField] private ClassReference _classReference;
        private void Awake()
        {
            _galleryView.OnGalleryCharactersSet += SetCharacters;
        }

        private void OnDestroy()
        {
            _galleryView.OnGalleryCharactersSet -= SetCharacters;
        }
        private void SetCharacters(CustomCharacter[] selectedCharacters)
        {
            for (int i = 0; i< selectedCharacters.Length; i++)
            {
                CharacterID charID = selectedCharacters[i] == null ? CharacterID.None : selectedCharacters[i].Id;

                string lookupId = ((int)charID).ToString();
                PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(lookupId);
                
                if (info.Id != lookupId)
                {
                    _selectedCharSlots[i].SetCharacterVisibility(false);
                    continue;
                }
                else
                {
                    _selectedCharSlots[i].SetCharacterVisibility(true);
                }

                CharacterClassID classID = CustomCharacter.GetClassID(charID);
                Color bgColor = _classReference.GetColor(classID);
                Color bgAltColor = _classReference.GetAlternativeColor(classID);

                _selectedCharSlots[i].SetInfo(selectedCharacters[i], info.GalleryImage,bgColor, bgAltColor, info.Name, _classReference.GetName(classID));
            }
          
        }

    }
}
