using System.Collections.Generic;
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
        [SerializeField] private LoadoutModeSwitcher _loadoutSwitcher;
        [SerializeField] private SelectedCharSlot[] _selectedCharSlots;
        [SerializeField] private bool gradientOverride = false;

        [SerializeField] private ClassReference _classReference;

        // Store existing sprites for CharacterClassTypes to not create duplicates and save memory
        private Dictionary<CharacterClassType, Sprite> _classBgSprites = new();

        private void Awake()
        {
            _galleryView.OnGalleryCharactersSet += SetCharacters;
        }

        private void OnEnable()
        {
            if(_loadoutSwitcher) _loadoutSwitcher.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (_loadoutSwitcher) _loadoutSwitcher.gameObject.SetActive(false);
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
                
                if (info == null ||info.Id != lookupId)
                {
                    _selectedCharSlots[i].SetCharacterVisibility(false);
                    continue;
                }
                else
                {
                    _selectedCharSlots[i].SetCharacterVisibility(true);
                }

                CharacterClassType classType = CustomCharacter.GetClass(charID);
                Color bgColor = _classReference.GetColor(classType);
                Color bgDeepColor = _classReference.GetDeepColor(classType);
                Sprite classIcon = _classReference.GetCornerIcon(classType);

                _selectedCharSlots[i].SetInfo(selectedCharacters[i], info.GalleryHeadImage,bgColor, bgDeepColor, info.Name, _classReference.GetNameIcon(classType), classIcon);

                if (gradientOverride)
                {
                    Sprite bgImage;
                    // If there already is a sprite generated for this classtype, use the existing one
                    if (_classBgSprites.TryGetValue(classType, out var sprite))
                    {
                        bgImage = sprite;
                    }
                    // If there is no sprite for this classtype, create a new one
                    else
                    {
                        Color bgAltColor = _classReference.GetAlternativeColor(classType);
                        bgImage = ThreeColorGradient.GenerateGradient(bgColor, bgAltColor, bgDeepColor);

                        // Store the sprite to the dictionary
                        _classBgSprites.Add(classType, bgImage);
                    }
                    _selectedCharSlots[i].GradientOverride(bgImage);
                }
            }

        }

    }
}
