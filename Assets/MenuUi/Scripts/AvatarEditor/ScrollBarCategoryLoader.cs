using System;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using Assets.Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ScrollBarCategoryLoader : MonoBehaviour
    {
        [SerializeField] private AvatarEditorController _controller;
        [SerializeField] private ColorGridLoader _colorLoader;

        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private Sprite _selectedSlotSprite;
        [SerializeField] private Sprite _slotsprite;
        [SerializeField] private Image _lastSelectedSlotImage;
        [SerializeField] private GameObject _introText;
        [SerializeField] private GameObject _bottomMenu;

        [SerializeField] private Image _bodyImage;
        [SerializeField] private Image _bodyBackgroundImage;
        [SerializeField] private TextMeshProUGUI _bodyTMP;
        [SerializeField] private Button _bodyButton;

        [SerializeField] private List<CategoryButton> _categoryButtons;
        [SerializeField] private Dictionary<AvatarPiece, CategoryButton> _slotToCategoryButton = new();

        private string _currentlySelectedCategory = "10";
        public string CurrentlySelectedCategory => _currentlySelectedCategory;

        public void UpdateSlotImage(AvatarPiece slot, AvatarPartInfo partInfo)
        {
            if (_slotToCategoryButton.TryGetValue(slot, out CategoryButton button))
            {
                button.FeatureImage.preserveAspect = true;
                button.FeatureImage.sprite = partInfo.IconImage;
            }
        }

        public void UpdateSlotImages()
        {
            _slotToCategoryButton?.Clear();

            foreach (CategoryButton button in _categoryButtons)
            {
                _slotToCategoryButton.Add(button.Slot, button);

                AvatarPartInfo partInfo = AvatarPartsReference.Instance.GetAvatarPartById(_controller.PlayerAvatar.GetPartId(button.Slot));
                UpdateSlotImage(button.Slot, partInfo);
            }

            ColorUtility.TryParseHtmlString(_controller.PlayerAvatar.SkinColor, out Color skinColor);

            if (skinColor != null)
            {
                _bodyImage.color = skinColor;
            }
        }

        public void SetCategoryButtons(Action<string> buttonFunction)
        {
            foreach (CategoryButton buttonRef in _categoryButtons)
            {
                buttonRef.Button.onClick.RemoveAllListeners();
                buttonRef.Button.onClick.AddListener(() =>
                {
                    _introText.SetActive(false);
                    _bottomMenu.SetActive(true);
                    _lastSelectedSlotImage.sprite = _slotsprite;
                    _currentlySelectedCategory = buttonRef.CategoryId;
                    _categoryText.text = buttonRef.TMP.text;
                    buttonRef.BackgroundImage.sprite = _selectedSlotSprite;
                    _lastSelectedSlotImage = buttonRef.BackgroundImage;
                    buttonFunction.Invoke(buttonRef.CategoryId);

                    _colorLoader.UpdateHighlight(_controller.PlayerAvatar.GetPartColor(buttonRef.Slot));
                });
            }

            _bodyButton.onClick.RemoveAllListeners();
            _bodyButton.onClick.AddListener(() =>
            {
                _introText.SetActive(false);
                _bottomMenu.SetActive(true);
                _lastSelectedSlotImage.sprite = _slotsprite;
                _currentlySelectedCategory = "";
                _categoryText.text = _bodyTMP.text;
                _bodyBackgroundImage.sprite = _selectedSlotSprite;
                _lastSelectedSlotImage = _bodyBackgroundImage;
                buttonFunction.Invoke("");

                _colorLoader.UpdateHighlight(_controller.PlayerAvatar.SkinColor);
            });
        }
    }
}
