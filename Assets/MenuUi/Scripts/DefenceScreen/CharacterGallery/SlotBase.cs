using System.Runtime.InteropServices;
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

            if (GetComponentInChildren<GalleryCharacter>() != null && selectable) // play selectable animation if there is a gallery character child
            {
                PlaySelectableAnimation();
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







