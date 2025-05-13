using System.Collections.Generic;

using UnityEngine;

using Altzone.Scripts.Model.Poco.Game;

using MenuUi.Scripts.Signals;

namespace MenuUi.Scripts.CharacterGallery
{

    public class EditingPopup : MonoBehaviour
    {
        [SerializeField] private GalleryView _galleryView;
        // Array of character slots in selected grid
        [SerializeField] private SelectedCharacterEditingSlot[] _selectedCharacterSlots;

        private void Awake()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);

            SignalBus.OnDefenceGalleryEditPanelRequested += OpenPopup;

            for (int i = 0; i < _selectedCharacterSlots.Length; i++)
            {
                _selectedCharacterSlots[i].SlotIndex = i;
            }
        }


        private void OnDestroy()
        {
            SignalBus.OnDefenceGalleryEditPanelRequested -= OpenPopup;
        }


        public void OpenPopup()
        {
            gameObject.SetActive(true);
        }


        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
