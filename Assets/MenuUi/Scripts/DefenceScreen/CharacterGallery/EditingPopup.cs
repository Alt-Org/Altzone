using UnityEngine;

using MenuUi.Scripts.Signals;

using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.UIScaling;
using MenuUi.Scripts.SwipeNavigation;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Handles the functionality for character gallery editing popup.
    /// Note: it's important that the prefab is active at first so Awake triggers to connect SignalBus event.
    /// </summary>
    public class EditingPopup : MonoBehaviour
    {
        [SerializeField] private GalleryView _galleryView;
        private SwipeUI _swipe;
        private RectTransform _rectTransform;
        private bool _charactersUpdated = false;

        // Array of character slots in selected grid
        [SerializeField] private SelectedCharacterEditingSlot[] _selectedCharacterSlots;

        private bool _openedFromLoadout = false;
        private int _currentLoadoutIndex = -1;

        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            _swipe.OnCurrentPageChanged += ClosePopup;

            if (gameObject.activeSelf) gameObject.SetActive(false);

            _galleryView.OnGalleryCharactersSet += SetCharacters;
            _galleryView.OnFilterChanged += HandleFilterChanged;
            SignalBus.OnDefenceGalleryEditPanelRequested += OpenPopupFromSelected;
            SignalBus.OnDefenceGalleryEditPanelRequestedForLoadout += OpenPopupFromLoadout;

            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                _selectedCharacterSlots[i].SlotIndex = i;
                _selectedCharacterSlots[i].OnSlotPressed += HandleSlotPressed;
            }
        }

        private void OnEnable()
        {
            if (_rectTransform == null)_rectTransform = GetComponent<RectTransform>();

            _rectTransform.anchorMin = new Vector2(0, PanelScaler.CalculateBottomPanelHeight());
            _rectTransform.anchorMax = new Vector2(1, 1 - (PanelScaler.CalculateTopPanelHeight() + PanelScaler.CalculateUnsafeAreaHeight()));
        }

        private void OnDestroy()
        {
            _galleryView.OnGalleryCharactersSet -= SetCharacters;
            _galleryView.OnFilterChanged -= HandleFilterChanged;
            SignalBus.OnDefenceGalleryEditPanelRequested -= OpenPopupFromSelected;
            SignalBus.OnDefenceGalleryEditPanelRequestedForLoadout += OpenPopupFromLoadout;


            foreach (SelectedCharacterEditingSlot slot in _selectedCharacterSlots)
            {
                slot.OnSlotPressed -= HandleSlotPressed;
            }

            _swipe.OnCurrentPageChanged -= ClosePopup;
        }

        private void OnDisable()
        {
            ClosePopup();
        }

        /// <summary>
        /// Open selected defence characters editing popup.
        /// </summary>
        public void OpenPopup()
        {
            _charactersUpdated = false;
            gameObject.SetActive(true);
        }


        /// <summary>
        /// Close selected defence characters editing popup.
        /// </summary>
        public void ClosePopup()
        {
            gameObject.SetActive(false);
            _openedFromLoadout = false;
            _currentLoadoutIndex = -1;
            if (_charactersUpdated) SignalBus.OnReloadCharacterGalleryRequestedSignal();
        }


        private void SetCharacters(CustomCharacter[] selectedCharacters)
        {
            // Going through every character slot in the gallery to see which ones are selected
            foreach (CharacterSlot slot in _galleryView.CharacterSlots)
            {
                // Setting slot as editable and adding listeners
                slot.SetEditable(true);
                slot.OnSlotPressed -= HandleSlotPressed;
                slot.OnSlotPressed += HandleSlotPressed;

                // Changing parent and setting selected visuals for the selected characters
                for (int i = 0; i < _selectedCharacterSlots.Length; i++)
                {
                    if (selectedCharacters[i] == null) continue;
                    if (selectedCharacters[i].Id == slot.Id)
                    {
                        _selectedCharacterSlots[i].SelectedCharacter = slot.Character;

                        slot.Character.transform.SetParent(_selectedCharacterSlots[i].transform, false);
                        slot.Character.SetSelectedVisuals();
                        slot.gameObject.SetActive(false);
                    }
                }
            }
        }


        private void HandleSlotPressed(SlotBase pressedSlot)
        {
            // Checking if player pressed selected character slot
            SelectedCharacterEditingSlot selectedCharacterSlot = pressedSlot as SelectedCharacterEditingSlot;
            if (selectedCharacterSlot != null)
            {
                // Checking if the slot is empty or has a selected character in it
                if (selectedCharacterSlot.SelectedCharacter == null) return;

                // Returning selected character to original slot
                selectedCharacterSlot.SelectedCharacter.ReturnToOriginalSlot();
                selectedCharacterSlot.SelectedCharacter = null;

                if (_openedFromLoadout)
                {
                    SignalBus.OnLoadoutDefenceCharacterChangedSignal(CharacterID.None, selectedCharacterSlot.SlotIndex, _currentLoadoutIndex);
                }
                else
                {
                    SignalBus.OnSelectedDefenceCharacterChangedSignal(CharacterID.None, selectedCharacterSlot.SlotIndex);
                }
                   
                _charactersUpdated = true;
                return;
            }
            
                // Checking if the pressed slot is a CharacterSlot
                CharacterSlot characterSlot = pressedSlot as CharacterSlot;
                if (characterSlot == null) return;

                // Finding free selected character slot for the pressed character
                foreach (SelectedCharacterEditingSlot slot in _selectedCharacterSlots)
                {
                    if (slot.SelectedCharacter != null) continue;
                    selectedCharacterSlot = slot;
                    break;
                }

                // If no free slots we don't need to do anything
                if (selectedCharacterSlot == null) return;

                // Setting the gallery character to the free slot
                characterSlot.Character.transform.SetParent(selectedCharacterSlot.transform, false);
                characterSlot.Character.SetSelectedVisuals();
                selectedCharacterSlot.SelectedCharacter = characterSlot.Character;
                characterSlot.gameObject.SetActive(false);
             if (_openedFromLoadout)
            {
                SignalBus.OnLoadoutDefenceCharacterChangedSignal(characterSlot.Character.Id, selectedCharacterSlot.SlotIndex, _currentLoadoutIndex);
            }
            else
            {
                SignalBus.OnSelectedDefenceCharacterChangedSignal(characterSlot.Character.Id, selectedCharacterSlot.SlotIndex);
            }

                _charactersUpdated = true;
            
        }


        private void HandleFilterChanged() // Ensuring the selected character's slots are still hidden even though filter changed
        {
            foreach (SelectedCharacterEditingSlot slot in _selectedCharacterSlots)
            {
                if (slot.SelectedCharacter == null) continue;

                if (slot.SelectedCharacter.OriginalSlot.gameObject.activeSelf)
                {
                    slot.SelectedCharacter.OriginalSlot.gameObject.SetActive(false);
                }
            }
        }
        private void OpenPopupFromSelected()
        {
            _openedFromLoadout = false;
            _currentLoadoutIndex = -1;
            OpenPopup();
        }

        private void OpenPopupFromLoadout(int loadoutIndex)
        {
            _openedFromLoadout = true;
            _currentLoadoutIndex = loadoutIndex;
            OpenPopup();
        }
    }
}
