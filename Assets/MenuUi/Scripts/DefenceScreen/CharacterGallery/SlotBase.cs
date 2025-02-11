using UnityEngine;

namespace MenuUi.Scripts.CharacterGallery
{
    public class SlotBase : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _selectButton;

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
            if (galleryCharacter != null && selectable) // play selectable animation if there is a gallery character child
            {
                PlaySelectableAnimation();
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







