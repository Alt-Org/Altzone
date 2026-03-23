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
        [Header("Buttons")]
        [SerializeField] private AvatarEditorController _controller;
        [SerializeField] private Button _noseButton;
        [SerializeField] private Button _mouthButton;
        [SerializeField] private Button _handsButton;
        [SerializeField] private Button _bodyButton;
        [SerializeField] private Button _hairButton;
        [SerializeField] private Button _eyesButton;
        [SerializeField] private Button _clothesButton;
        [SerializeField] private Button _shoesButton;

        [Header("Slot Images")]
        [SerializeField] private Image _hairImage;
        [SerializeField] private Image _eyesImage;
        [SerializeField] private Image _noseImage;
        [SerializeField] private Image _mouthImage;
        [SerializeField] private Image _bodyImage;
        [SerializeField] private Image _clothesImage;
        [SerializeField] private Image _handsImage;
        [SerializeField] private Image _shoesImage;

        [SerializeField] private TextMeshProUGUI _categoryText;

        private Dictionary<Button, string> _buttonToCategoryId;

        // This is needed before awake even runs so need to do this
        private Dictionary<Button, string> ButtonToCategoryId
        {
            get
            {
                if (_buttonToCategoryId == null || _buttonToCategoryId.Count == 0)
                {
                    _buttonToCategoryId = new Dictionary<Button, string>
                    {
                        { _hairButton, "10" },
                        { _eyesButton, "21" },
                        { _noseButton, "22" },
                        { _mouthButton, "23" },
                        { _bodyButton, "" },
                        { _clothesButton, "31" },
                        { _handsButton, "32" },
                        { _shoesButton, "33" }
                    };
                }
                return _buttonToCategoryId;
            }
        }
        private Dictionary<AvatarPiece, Image> _pieceToImage;
        private Dictionary<AvatarPiece, Image> PieceToImage
        {
            get
            {
                if (_pieceToImage == null)
                {
                    _pieceToImage = new Dictionary<AvatarPiece, Image>
                    {
                        { AvatarPiece.Hair, _hairImage },
                        { AvatarPiece.Eyes, _eyesImage },
                        { AvatarPiece.Nose, _noseImage },
                        { AvatarPiece.Mouth, _mouthImage },
                        { AvatarPiece.Clothes, _clothesImage },
                        { AvatarPiece.Hands, _handsImage },
                        { AvatarPiece.Feet, _shoesImage }
                    };
                }
                return _pieceToImage;
            }
        }
        private string _currentlySelectedCategory = "10";
        public string CurrentlySelectedCategory => _currentlySelectedCategory;
        private TextMeshProUGUI _currentCategoryTMP;

        public void ClickHairButton()
        {
            _hairButton.onClick.Invoke();
        }

        public void UpdateSlotImage(AvatarPiece slot, AvatarPartInfo partInfo)
        {
            if (PieceToImage.TryGetValue(slot, out Image image))
            {
                image.sprite = partInfo.IconImage;
            }
        }

        public void UpdateSlotImages()
        {
            foreach (AvatarPiece piece in Enum.GetValues(typeof (AvatarPiece)))
            {
                AvatarPartInfo partInfo = AvatarPartsReference.Instance.GetAvatarPartById(_controller.PlayerAvatar.GetPartId(piece));
                UpdateSlotImage(piece, partInfo);
            }

            ColorUtility.TryParseHtmlString(_controller.PlayerAvatar.SkinColor, out Color skinColor);

            if (skinColor != null)
            {
                _bodyImage.color = skinColor;
            }
        }

        public void SetCategoryButtons(Action<string> buttonFunction)
        {
            foreach (KeyValuePair<Button, string> entry in ButtonToCategoryId)
            {
                Button button = entry.Key;
                string id = entry.Value;

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

                button.onClick.RemoveAllListeners();

                button.onClick.AddListener(() =>
                {
                    _currentlySelectedCategory = id;
                    _currentCategoryTMP = buttonText;

                    _categoryText.text = buttonText.text;
                    buttonFunction.Invoke(id);
                });
            }
        }
    }
}
