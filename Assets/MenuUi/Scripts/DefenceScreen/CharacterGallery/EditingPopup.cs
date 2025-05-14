using UnityEngine;

using MenuUi.Scripts.Signals;

using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Handles the functionality for character gallery editing popup.
    /// Note: it's important that the prefab is active at first so Awake triggers to connect SignalBus event.
    /// </summary>
    public class EditingPopup : MonoBehaviour
    {
        [SerializeField] private GalleryView _galleryView;

        // Array of character slots in selected grid
        [SerializeField] private SelectedCharacterEditingSlot[] _selectedCharacterSlots;

        private void Awake()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);

            _galleryView.OnGalleryCharactersSet += SetCharacters;
            SignalBus.OnDefenceGalleryEditPanelRequested += OpenPopup;

            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                _selectedCharacterSlots[i].SlotIndex = i;
                _selectedCharacterSlots[i].OnSlotPressed += HandleSlotPressed;
            }
        }


        private void OnDestroy()
        {
            _galleryView.OnGalleryCharactersSet -= SetCharacters;
            SignalBus.OnDefenceGalleryEditPanelRequested -= OpenPopup;

            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                _selectedCharacterSlots[i].SlotIndex = i;
                _selectedCharacterSlots[i].OnSlotPressed -= HandleSlotPressed;
            }
        }


        /// <summary>
        /// Open selected defence characters editing popup.
        /// </summary>
        public void OpenPopup()
        {
            gameObject.SetActive(true);
        }


        /// <summary>
        /// Close selected defence characters editing popup.
        /// </summary>
        public void ClosePopup()
        {
            gameObject.SetActive(false);
            SignalBus.OnReloadCharacterGalleryRequestedSignal();
        }


        private void SetCharacters(int[] selectedCharacterIds)
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
                    if (selectedCharacterIds[i] == (int)slot.Id)
                    {
                        _selectedCharacterSlots[i].SelectedCharacter = slot.Character;

                        slot.Character.transform.SetParent(_selectedCharacterSlots[i].transform, false);
                        slot.Character.SetSelectedVisuals();
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
                SignalBus.OnSelectedDefenceCharacterChangedSignal(CharacterID.None, selectedCharacterSlot.SlotIndex);
            }
            else
            {
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
                SignalBus.OnSelectedDefenceCharacterChangedSignal(characterSlot.Character.Id, selectedCharacterSlot.SlotIndex);
            }
        }
    }
}
