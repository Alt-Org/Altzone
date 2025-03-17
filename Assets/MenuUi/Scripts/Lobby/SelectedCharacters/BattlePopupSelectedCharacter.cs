using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.ModelV2;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.Signals;
using System;

namespace MenuUi.Scripts.Lobby.SelectedCharacters
{
    /// <summary>
    /// Added to the individual selected character slots in Battle Popup SelectedCharacters prefab.
    /// Handles setting the visuals for the slot, and has functionality for the dropdown from which you can select a new defence character.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BattlePopupSelectedCharacter : AltMonoBehaviour
    {
        [Header("Character slot references")]
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private PieChartPreview _piechartPreview;

        [Header("Dropdown references")]
        [SerializeField] private GameObject _selectionDropdown;
        [SerializeField] private Transform _selectionDropdownContent;
        [SerializeField] private BaseScrollRect _dropdownScrollRect;

        [Header("Reference sheet")]
        [SerializeField] private ClassColorReference _classColorReference;

        private CharacterID _characterId;
        private int _slotIdx;
        private Button _button;

        public Button ButtonComponent { get { return _button; } }

        public Action SelectedCharactersChanged;


        private void OnDisable()
        {
            CloseSelectionDropdown();
        }


        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(ToggleSelectionDropdown);
        }


        /// <summary>
        /// Set character info to the battle popup selected character slot.
        /// </summary>
        /// <param name="galleryImage">Character's gallery sprite.</param>
        /// <param name="charID">Character id.</param>
        /// <param name="isEditable">If the character is editable for the local player.</param>
        /// <param name="slotIdx">The slot index for this selected character slot.</param>
        /// <param name="stats">The stats for the character in an int array. Order: Hp, Speed, CharacterSize, Attack, Defence.</param>
        public void SetInfo(Sprite galleryImage, CharacterID charID, bool isEditable, int slotIdx, int[] stats = null)
        {
            _spriteImage.sprite = galleryImage;
            _spriteImage.enabled = true;

            CharacterClassID charClassID = CustomCharacter.GetClassID(charID);
            _backgroundImage.color = _classColorReference.GetColor(charClassID);

            _characterId = charID;

            _slotIdx = slotIdx;

            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            
            if (!isEditable)
            {
                _button.enabled = false;
            }
            else
            {
                _button.enabled = true;
                _button.onClick.RemoveListener(ToggleSelectionDropdown);
                _button.onClick.AddListener(ToggleSelectionDropdown);
            }

            _piechartPreview.gameObject.SetActive(true);
            if (stats != null)
            {
                _piechartPreview.UpdateChart(stats[3], stats[0], stats[4], stats[2], stats[1]);
            }
            else
            {
                _piechartPreview.UpdateChart(_characterId);
            }
        }


        /// <summary>
        /// Set character slot showing as empty.
        /// </summary>
        /// <param name="isEditable">If slot is editable for the local player or not.</param>
        /// <param name="slotIdx">Slot's index.</param>
        public void SetEmpty(bool isEditable, int slotIdx)
        {
            _spriteImage.enabled = false;
            _backgroundImage.color = Color.white;
            _piechartPreview.gameObject.SetActive(false);
            _characterId =CharacterID.None;
            _slotIdx = slotIdx;

            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (isEditable)
            {
                _button.enabled = true;
                _button.onClick.RemoveListener(ToggleSelectionDropdown);
                _button.onClick.AddListener(ToggleSelectionDropdown);
            }
            else
            {
                _button.enabled = false;
            }
        }


        private void ToggleSelectionDropdown()
        {
            if (_selectionDropdown.activeSelf) CloseSelectionDropdown(); else OpenSelectionDropdown();
        }


        private void OpenSelectionDropdown()
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                var characters = playerData.CustomCharacters.GroupBy(x => x.Id).Select(x => x.First()).ToList(); // ensuring no duplicate characters are shown

                foreach (CustomCharacter character in characters)
                {
                    if (character.Id != _characterId)
                    {
                        CreateDropdownButton(character);
                    }
                }

                if (_selectionDropdownContent.childCount > 0) // only opening dropdown if there are dropdown buttons
                {
                    _selectionDropdown.SetActive(true);
                }
            }));
        }


        public void CloseSelectionDropdown()
        {
            if (_selectionDropdown.activeSelf)
            {
                _selectionDropdown.SetActive(false);
                for (int i = 0; i < _selectionDropdownContent.childCount; i++)
                {
                    Transform dropdownButton = _selectionDropdownContent.GetChild(i);
                    Button buttonComponent = dropdownButton.GetComponent<Button>();
                    buttonComponent.onClick.RemoveAllListeners();

                    Destroy(dropdownButton.gameObject);
                }
                _dropdownScrollRect.VerticalNormalizedPosition = 1;
            }
        }


        private void HandleNewCharacterSelected(CustomCharacter newCharacter)
        {
            StartCoroutine(GetPlayerData(playerData =>
            {
                // Checking if we have to swap characters. The dropdown menu holds all characters except the current slot's.
                for (int i = 0; i < playerData.SelectedCharacterIds.Length; i++)
                {
                    if (playerData.SelectedCharacterIds[i] == newCharacter.ServerID)
                    {
                        SignalBus.OnSelectedDefenceCharacterChangedSignal(_characterId, i); // swapping this slot's character to the other character's slot
                    }
                }

                SignalBus.OnSelectedDefenceCharacterChangedSignal(newCharacter.Id, _slotIdx); // setting new character to this slot
                SignalBus.OnReloadCharacterGalleryRequestedSignal();
                SelectedCharactersChanged.Invoke();
            }));
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
            button.onClick.AddListener(() =>
            {
                CloseSelectionDropdown();
                HandleNewCharacterSelected(character);
            });

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

            // Adding dropdown button to dropdown contents
            dropdownButton.transform.SetParent(_selectionDropdownContent, false);
        }
    }
}
