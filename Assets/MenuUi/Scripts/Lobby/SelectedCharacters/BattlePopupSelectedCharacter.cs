using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    [RequireComponent(typeof(Button))]
    public class BattlePopupSelectedCharacter : AltMonoBehaviour
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _backgroundDetailsImage;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] private GameObject _selectionDropdown;
        [SerializeField] private Transform _selectionDropdownContent;
        [SerializeField] private ClassColorReference _classColorReference;

        private CharacterID _characterId;
        private Button _button;
        private bool _isOwnCharacter;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_isOwnCharacter)
            {
                _button.onClick.AddListener(ToggleSelectionDropdown);
            }
        }


        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }


        public void SetInfo(Sprite galleryImage, CharacterID charID, bool isOwnCharacter)
        {
            _spriteImage.sprite = galleryImage;

            CharacterClassID charClassID = CustomCharacter.GetClassID(charID);
            _backgroundImage.color = _classColorReference.GetColor(charClassID);

            _characterId = charID;
            _piechartPreview.UpdateChart(_characterId);
            _isOwnCharacter = isOwnCharacter;
        }


        private void ToggleSelectionDropdown()
        {
            if (_selectionDropdown.activeSelf) CloseSelectionDropdown(); else OpenSelectionDropdown();
        }


        private void OpenSelectionDropdown()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                var characters = playerData.CustomCharacters.ToList();

                foreach (CustomCharacter character in characters)
                {
                    bool isValid = true;

                    for (int i = 0; i < playerData.SelectedCharacterIds.Length; i++)
                    {
                        if (character.ServerID == playerData.SelectedCharacterIds[i])
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        CreateDropdownButton(character);
                    }
                }
                _selectionDropdown.SetActive(true);
            }));
        }


        private void CloseSelectionDropdown()
        {
            _selectionDropdown.SetActive(false);
            for (int i = 0; i < _selectionDropdownContent.childCount; i++)
            {
                Destroy(_selectionDropdownContent.GetChild(i).gameObject);
            }
        }


        private void CreateDropdownButton(CustomCharacter character)
        {
            PlayerCharacterPrototype charInfo = PlayerCharacterPrototypes.GetCharacter(((int)character.Id).ToString());
            CharacterClassID charClassID = CustomCharacter.GetClassID(character.Id);

            GameObject dropdownButton = new();

            // Background image
            Image backgroundImage = dropdownButton.AddComponent<Image>();
            backgroundImage.sprite = _backgroundImage.sprite;
            backgroundImage.color = _classColorReference.GetColor(charClassID);

            // Button component
            Button button = dropdownButton.AddComponent<Button>();

            // Details image
            GameObject details = new();
            Image detailsImage = details.AddComponent<Image>();
            detailsImage.sprite = _backgroundDetailsImage.sprite;
            details.transform.SetParent(dropdownButton.transform, false);

            RectTransform detailsRect = details.GetComponent<RectTransform>();
            detailsRect.sizeDelta = Vector2.zero;
            detailsRect.anchorMin = Vector2.zero;
            detailsRect.anchorMax = Vector2.one;

            // Sprite image
            GameObject gallerySprite = new();
            Image spriteImage = gallerySprite.AddComponent<Image>();
            spriteImage.preserveAspect = true;
            spriteImage.sprite = charInfo.GalleryImage;
            gallerySprite.transform.SetParent(dropdownButton.transform, false);

            RectTransform spriteRect = gallerySprite.GetComponent<RectTransform>();
            spriteRect.sizeDelta = Vector2.zero;
            spriteRect.anchorMin = Vector2.zero;
            spriteRect.anchorMax = Vector2.one;


            dropdownButton.transform.SetParent(_selectionDropdownContent, false);
        }
    }
}
