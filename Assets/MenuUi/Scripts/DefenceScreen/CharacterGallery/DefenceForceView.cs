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
        private void SetCharacters(int[] selectedCharacterIds)
        {
            for (int i = 0; i<selectedCharacterIds.Length; i++)
            {
                PlayerCharacterPrototype info = PlayerCharacterPrototypes.GetCharacter(selectedCharacterIds[i].ToString());
                if (info == null)
                {
                    _selectedCharSlots[i].SetCharacterVisibility(false);
                    continue;
                }
                else
                {
                    _selectedCharSlots[i].SetCharacterVisibility(true);
                }

                CharacterID charID =(CharacterID)selectedCharacterIds[i];
                CharacterClassID classID = CustomCharacter.GetClassID(charID);
                Color bgColor = _classReference.GetColor(classID);
                Color bgAltColor = _classReference.GetAlternativeColor(classID);


                _selectedCharSlots[i].SetInfo(info.GalleryImage,bgColor, bgAltColor, info.Name, _classReference.GetName(classID), charID);
            }
          
        }

    }
}
