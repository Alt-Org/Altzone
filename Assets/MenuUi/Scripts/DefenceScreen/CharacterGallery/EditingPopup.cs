using UnityEngine;

using MenuUi.Scripts.Signals;

using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.UIScaling;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine.UI;

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

        [SerializeField] private BlinkingFrame[] _blinkingFrames;

        [SerializeField] private Button _removeCharacterButton;


        // Which selected slot is currently active
        private int _activeSlotIndex = 0;

        private bool _openedFromLoadout = false;
        private int _currentLoadoutIndex = -1;

        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            if (_swipe) _swipe.OnCurrentPageChanged += ClosePopup;

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

            if (_removeCharacterButton != null)
            {
                _removeCharacterButton.onClick.AddListener(RemoveActiveSlotCharacter);
            }

        }

        private void OnEnable()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            _rectTransform.anchorMin = new Vector2(0, PanelScaler.CalculateBottomPanelHeight());
            _rectTransform.anchorMax = new Vector2(1, 1 - (PanelScaler.CalculateTopPanelHeight() + PanelScaler.CalculateUnsafeAreaHeight()));
        }

        private void OnDestroy()
        {
            _galleryView.OnGalleryCharactersSet -= SetCharacters;
            _galleryView.OnFilterChanged -= HandleFilterChanged;
            SignalBus.OnDefenceGalleryEditPanelRequested -= OpenPopupFromSelected;
            SignalBus.OnDefenceGalleryEditPanelRequestedForLoadout -= OpenPopupFromLoadout;


            foreach (SelectedCharacterEditingSlot slot in _selectedCharacterSlots)
            {
                slot.OnSlotPressed -= HandleSlotPressed;
            }

            if (_swipe) _swipe.OnCurrentPageChanged -= ClosePopup;

            if (_removeCharacterButton != null)
            {
                _removeCharacterButton.onClick.RemoveListener(RemoveActiveSlotCharacter);
            }

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

            SetActiveSlot(0);
        }


        /// <summary>
        /// Close selected defence characters editing popup.
        /// </summary>
        public void ClosePopup()
        {
            StopAllBlinking();

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
                SetActiveSlot(selectedCharacterSlot.SlotIndex);
                return;
            }


            // Checking if the pressed slot is a CharacterSlot
            CharacterSlot characterSlot = pressedSlot as CharacterSlot;
            if (characterSlot == null) return;

            SelectedCharacterEditingSlot targetSlot = _selectedCharacterSlots[_activeSlotIndex];
            if (targetSlot == null) return;

            if (targetSlot.SelectedCharacter != null)
            {
                targetSlot.SelectedCharacter.ReturnToOriginalSlot();

                if (targetSlot.SelectedCharacter.OriginalSlot != null)
                {
                    targetSlot.SelectedCharacter.OriginalSlot.gameObject.SetActive(true);
                }

                targetSlot.SelectedCharacter = null;

                if (_openedFromLoadout)
                {
                    SignalBus.OnLoadoutDefenceCharacterChangedSignal(CharacterID.None, targetSlot.SlotIndex, _currentLoadoutIndex);
                }
                else
                {
                    SignalBus.OnSelectedDefenceCharacterChangedSignal(CharacterID.None, targetSlot.SlotIndex);
                }

            }
            characterSlot.Character.transform.SetParent(targetSlot.transform, false);
            characterSlot.Character.SetSelectedVisuals();
            targetSlot.SelectedCharacter = characterSlot.Character;
            characterSlot.gameObject.SetActive(false);

            if (_openedFromLoadout)
            {
                SignalBus.OnLoadoutDefenceCharacterChangedSignal(characterSlot.Character.Id, targetSlot.SlotIndex, _currentLoadoutIndex);
            }
            else
            {
                SignalBus.OnSelectedDefenceCharacterChangedSignal(characterSlot.Character.Id, targetSlot.SlotIndex);
            }

            _charactersUpdated = true;

            SetActiveSlot((_activeSlotIndex + 1) % _selectedCharacterSlots.Length);
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

        private void SetActiveSlot(int index)
        {
            _activeSlotIndex = Mathf.Clamp(index, 0, _selectedCharacterSlots.Length - 1);

            if (_blinkingFrames == null || _blinkingFrames.Length == 0) return;

            // Assumption: one frame per slot, matching index
            for (int i = 0; i < _blinkingFrames.Length; i++)
            {
                if (_blinkingFrames[i] == null) continue;

                if (i == _activeSlotIndex)
                    _blinkingFrames[i].StartBlinking();
                else
                    _blinkingFrames[i].StopBlinking();
            }
        }

        private void StopAllBlinking()
        {
            if (_blinkingFrames == null) return;

            for (int i = 0; i < _blinkingFrames.Length; i++)
            {
                if (_blinkingFrames[i] != null)
                    _blinkingFrames[i].StopBlinking();
            }
        }

        private void RemoveCharacterFromSlot(SelectedCharacterEditingSlot slot)
        {
            if (slot == null) return;
            if (slot.SelectedCharacter == null) return;

            slot.SelectedCharacter.ReturnToOriginalSlot();
            slot.SelectedCharacter = null;

            if (_openedFromLoadout)
            {
                SignalBus.OnLoadoutDefenceCharacterChangedSignal(
                    CharacterID.None,
                    slot.SlotIndex,
                    _currentLoadoutIndex);
            }
            else
            {
                SignalBus.OnSelectedDefenceCharacterChangedSignal(
                    CharacterID.None,
                    slot.SlotIndex);
            }

            _charactersUpdated = true;
        }
        public void RemoveActiveSlotCharacter()
        {
            SelectedCharacterEditingSlot targetSlot = _selectedCharacterSlots[_activeSlotIndex];
            RemoveCharacterFromSlot(targetSlot);
        }
    }
}
