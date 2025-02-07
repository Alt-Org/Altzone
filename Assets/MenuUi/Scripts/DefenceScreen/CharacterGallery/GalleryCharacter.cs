using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MenuUi.Scripts.SwipeNavigation;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using System;
using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.CharacterGallery
{
    public class GalleryCharacter : MonoBehaviour, IGalleryCharacterData
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _contentsImage;
        [SerializeField] private Image _contentsDetailsImage;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] private GameObject _removeButton;

        private CharacterSlot _originalSlot;

        private CharacterID _id;
        public CharacterID Id { get => _id; }

        public Action OnReturnedToOriginalSlot;

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


        public void SetInfo(Sprite sprite, Color bgColor, Color bgAltColor, string name, CharacterID id, CharacterSlot originalSlot)
        {
            _spriteImage.sprite = sprite;
            _characterNameText.text = name;
            _id = id;
            _backgroundImage.color = bgColor;
            _contentsImage.color = bgAltColor;
            _originalSlot = originalSlot;
        }


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
        }


        public void SetUnselectedVisuals()
        {
            _aspectRatioFitter.aspectRatio = 0.6f;
            _characterNameText.gameObject.SetActive(true);

            _spriteImage.rectTransform.anchorMax = new Vector2(0.9f, 0.75f);
            _spriteImage.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);

            _piechartPreview.gameObject.SetActive(false);

            _contentsImage.gameObject.SetActive(true);
            _contentsDetailsImage.gameObject.SetActive(true);
        }


        /// <summary>
        /// Reparent this character to its original slot.
        /// </summary>
        public void ReturnToOriginalSlot()
        {
            transform.SetParent(_originalSlot.transform, false);
            SetUnselectedVisuals();
            HideRemoveButton();
            OnReturnedToOriginalSlot?.Invoke();
        }


        public void ShowRemoveButton()
        {
            _removeButton.SetActive(true);
        }

        public void HideRemoveButton()
        {
            _removeButton.SetActive(false);
        }
    }
}
