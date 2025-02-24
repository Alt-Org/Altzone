using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Base class for CharacterInventorySlot and SelectedCharacterInventorySlot. Has methods related to setting slot selectable.
    /// </summary>
    public class SlotBase : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _selectButton;

        [HideInInspector] public bool IsLocked = false;

        public delegate void CharacterSelectedHandler(SlotBase slot);
        public CharacterSelectedHandler OnCharacterSelected;


        /// <summary>
        /// Set this slot as selectable or not selectable.
        /// </summary>
        /// <param name="selectable">If slot should be selectable or not.</param>
        public void SetSelectable(bool selectable)
        {
            _selectButton.SetActive(selectable);

            GalleryCharacter galleryCharacter = GetComponentInChildren<GalleryCharacter>();

            if (galleryCharacter != null && selectable)
            {
                if (!IsLocked) PlaySelectableAnimation(); // only play selectable animation if slot isn't locked
                galleryCharacter.DisableNaviButton();
            }
            else if (galleryCharacter != null && !selectable)
            {
                galleryCharacter.EnableNaviButton();
            }
        }


        private void PlaySelectableAnimation()
        {
            _animator.Play("SelectableAnimation", -1, 0f);
        }


        public void SelectButtonPressed()
        {
            OnCharacterSelected?.Invoke(this);
        }
    }
}







