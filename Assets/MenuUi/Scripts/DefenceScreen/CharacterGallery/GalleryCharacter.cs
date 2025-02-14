using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// Sets character gallery character's visual info to GalleryCharacter prefab.
    /// GalleryCharacter prefabs are parented under CharacterInventorySlot prefabs.
    /// Holds information about original inventory slot and has a method for returning this GalleryCharacter to its original slot
    /// </summary>
    public class GalleryCharacter : MonoBehaviour, IGalleryCharacterData
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _contentsImage;
        [SerializeField] private Image _contentsDetailsImage;
        [SerializeField] private Image _lockImage;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] private Material _grayScaleMaterial;

        private CharacterSlot _originalSlot;

        private CharacterID _id;
        public CharacterID Id { get => _id; }


        private void Awake()
        {
            _piechartPreview.gameObject.SetActive(false);
        }


        private void OnEnable()
        {
            if (_piechartPreview.gameObject.activeInHierarchy)
            {
                _piechartPreview.UpdateChart(Id);
            }
        }


        /// <summary>
        /// Initializes the visual and character slot info for this gallery character.
        /// </summary>
        /// <param name="sprite">The character sprite which to display.</param>
        /// <param name="bgColor">The main color to display in the GalleryCharacter background.</param>
        /// <param name="bgAltColor">The alternative color to display in the GalleryCharacter background.</param>
        /// <param name="name">Character's name which to display.</param>
        /// <param name="id">Character's ID.</param>
        /// <param name="originalSlot">The original inventory slot for the GalleryCharacter prefab.</param>
        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, CharacterID id, CharacterSlot originalSlot)
        {
            _spriteImage.sprite = sprite;
            _characterNameText.text = name;
            _id = id;
            _backgroundImage.color = bgColor;
            _contentsImage.color = bgAltColor;
            _originalSlot = originalSlot;
        }


        /// <summary>
        /// Set visual information and anchoring for when character is selected. (Placed to one of the top slots.)
        /// </summary>
        public void SetSelectedVisuals()
        {
            _aspectRatioFitter.aspectRatio = 1;
            _characterNameText.gameObject.SetActive(false);

            _spriteImage.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
            _spriteImage.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);

            _piechartPreview.gameObject.SetActive(true);
            _piechartPreview.UpdateChart(Id);

            _contentsImage.gameObject.SetActive(false);
            _contentsDetailsImage.gameObject.SetActive(false);

            _spriteImage.material = null;
            _contentsImage.material = null;
            _backgroundImage.material = null;

            _lockImage.gameObject.SetActive(false);
        }


        /// <summary>
        /// Set visual information and anchoring for when character is unselected. (Not in one of the top slots.)
        /// </summary>
        public void SetUnselectedVisuals()
        {
            _aspectRatioFitter.aspectRatio = 0.6f;
            _characterNameText.gameObject.SetActive(true);

            _spriteImage.rectTransform.anchorMax = new Vector2(0.9f, 0.75f);
            _spriteImage.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);

            _piechartPreview.gameObject.SetActive(false);

            _contentsImage.gameObject.SetActive(true);
            _contentsDetailsImage.gameObject.SetActive(true);

            _spriteImage.material = null;
            _contentsImage.material = null;
            _backgroundImage.material = null;

            _lockImage.gameObject.SetActive(false);
        }


        /// <summary>
        /// Set visual information for when character is locked. (Not owned.)
        /// </summary>
        public void SetLockedVisuals()
        {
            SetUnselectedVisuals();
            _spriteImage.material = _grayScaleMaterial;
            _contentsImage.material = _grayScaleMaterial;
            _contentsImage.material.SetColor("_Color", _contentsImage.color);
            _backgroundImage.material = _grayScaleMaterial;
            _backgroundImage.material.SetColor("_Color", _backgroundImage.color);
            _lockImage.gameObject.SetActive(true);
        }


        /// <summary>
        /// Reparent this character to its original slot.
        /// </summary>
        public void ReturnToOriginalSlot()
        {
            transform.SetParent(_originalSlot.transform, false);
            SetUnselectedVisuals();
        }


        /// <summary>
        /// Enable being able to press the navi button which can open character stats edit window.
        /// </summary>
        public void EnableNaviButton()
        {
            _backgroundImage.raycastTarget = true; // the button depends on background image being raycast target.
        }


        /// <summary>
        /// Disable being able to press the navi button which can open character stats edit window.
        /// </summary>
        public void DisableNaviButton()
        {
            _backgroundImage.raycastTarget = false;
        }
    }
}
