using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorController : AltMonoBehaviour
    {
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] private ScrollBarFeatureLoader _featureLoader;
        [SerializeField] private ColorGridLoader _colorLoader;
        [SerializeField] private float _timeoutSeconds = 10f;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
        [SerializeField] private FeatureSetter _featureSetter;
        [SerializeField] private AvatarVisualDataScriptableObject _visualDataScriptableObject;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _revertButton;
        [SerializeField] private Button _defaultButton;
        [SerializeField] private TextHandler _textHandler;
        [SerializeField] private PopUpHandler _popUpHandler;
        [SerializeField] private DivanImageHandler _divanImageHandler;
        [SerializeField] private AvatarLoader _avatarLoader;

        private PlayerData _currentPlayerData;
        private PlayerAvatar _playerAvatar;

        void Start()
        {
            _categoryLoader.SetCategoryCells((categoryId) => _featureLoader.RefreshFeatureListItems(categoryId));
            _colorLoader.SetColorCells();
            _colorLoader.gameObject.SetActive(false);

            UpdateCellSizes();

            _saveButton.onClick.AddListener(() => _popUpHandler.ShowPopUp());

            _popUpHandler.AddConfirmButtonListener(() =>
            {
                StartCoroutine(SaveAvatarData());
            });

            _defaultButton.onClick.AddListener(() =>
            {
                SetDefaultAvatar();
                _featureLoader.RefreshFeatureListItems(_categoryLoader.CurrentlySelectedCategory);
            });

            _revertButton.onClick.AddListener(() =>
            {
                RevertAvatarChanges();
                _featureLoader.RefreshFeatureListItems(_categoryLoader.CurrentlySelectedCategory);
            });

            StartCoroutine(ClickMiddleCategoryCellOnNextFrame());
        }

        private void OnEnable()
        {
            _divanImageHandler.UpdateDivanImage(_currentPlayerData);
            StartCoroutine(LoadAvatarData());
            _textHandler.SetRandomSpeechBubbleText();

            AspectRatioChangeDetector.OnAspectRatioChange += UpdateCellSizes;
        }

        private void OnDisable()
        {
            AspectRatioChangeDetector.OnAspectRatioChange -= UpdateCellSizes;
        }

        private void UpdateCellSizes()
        {
            _categoryLoader.UpdateCellSize();
            _colorLoader.UpdateCellSize();
            _featureLoader.UpdateCellSize();
        }

        private IEnumerator LoadAvatarData()
        {
            bool? timeout = null;
            PlayerData playerData = null;

            StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => playerData = data));
            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;
            SetAllAvatarFeatures();
            _divanImageHandler.UpdateDivanImage(playerData);
            _avatarLoader.UpdateVisuals(AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData.AvatarData));
        }

        private IEnumerator SaveAvatarData()
        {
            bool? timeout = null;
            PlayerData playerData = null;
            PlayerData savePlayerData = _currentPlayerData;

            savePlayerData.AvatarData = new(_playerAvatar.Name,
                null,
                _playerAvatar.SkinColor,
                null,
                new Vector2(1, 1));

            var features = Enum.GetValues(typeof(AvatarPiece));
            foreach (AvatarPiece feature in features)
            {
                AssignPartToPlayerData(savePlayerData.AvatarData, feature, _playerAvatar.GetPartId(feature));
                AssignColorToPlayerData(savePlayerData.AvatarData, feature, _playerAvatar.GetPartColor(feature));
            }
            StartCoroutine(SavePlayerData(savePlayerData, p => playerData = p));
            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;

            List<AvatarPiece> pieceIDs = Enum.GetValues(typeof(AvatarPiece)).Cast<AvatarPiece>().ToList();
            foreach (AvatarPiece piece in pieceIDs)
            {
                _visualDataScriptableObject.SetAvatarPiece(piece, _featureSetter.GetCurrentlySelectedFeaturePartInfo(piece));
                ColorUtility.TryParseHtmlString(_playerAvatar.GetPartColor(piece), out Color pieceColor);
                _visualDataScriptableObject.SetColor(piece, pieceColor);
            }

            ColorUtility.TryParseHtmlString(savePlayerData.AvatarData.Color, out Color skinColor);
            _visualDataScriptableObject.SkinColor = skinColor;

            AvatarDesignLoader.Instance.InvokeOnAvatarDesignUpdate();      

            GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        // If this isn't done, function will be called too early and will not work
        private IEnumerator ClickMiddleCategoryCellOnNextFrame()
        {
            yield return null;
            _categoryLoader.ClickMiddleCategoryCell();
        }

        private void SetAllAvatarFeatures()
        {
            if (_currentPlayerData.AvatarData == null || !_currentPlayerData.AvatarData.Validate())
            {
                Debug.LogError("AvatarData is null! Using default data.");
                _playerAvatar = new(_avatarDefaultReference.GetByCharacterId(_currentPlayerData.SelectedCharacterId)[0]);
            }
            else
            {
                _playerAvatar = new(_currentPlayerData.AvatarData);
            }

            _featureSetter.SetLoadedFeatures(_playerAvatar);
        }


        private void SetDefaultAvatar()
        {
            _playerAvatar = new(_avatarDefaultReference.GetByCharacterId(_currentPlayerData.SelectedCharacterId)[0]);

            _featureSetter.SetLoadedFeatures(_playerAvatar);
        }

        private void RevertAvatarChanges()
        {
            SetAllAvatarFeatures();
        }

        private void AssignColorToPlayerData(AvatarData playerAvatarData, AvatarPiece feature, string color)
        {
            switch(feature)
            {
                case AvatarPiece.Hair:
                    playerAvatarData.HairColor = color;
                    break;
                case AvatarPiece.Eyes:
                    playerAvatarData.EyesColor = color;
                    break;
                case AvatarPiece.Nose:
                    playerAvatarData.NoseColor = color;
                    break;
                case AvatarPiece.Mouth:
                    playerAvatarData.MouthColor = color;
                    break;
                case AvatarPiece.Clothes:
                    playerAvatarData.ClothesColor = color;
                    break;
                case AvatarPiece.Hands:
                    playerAvatarData.HandsColor = color;
                    break;
                case AvatarPiece.Feet:
                    playerAvatarData.FeetColor = color;
                    break;
                default:
                    Debug.LogWarning($"Couldn't find {feature} in AvatarPiece");
                    break;
            }
        }

        private void AssignPartToPlayerData(AvatarData playerAvatarData, AvatarPiece feature, string id)
        {
            int convertedID;
            if (int.TryParse(id, out int intID))
            {
                convertedID = intID;
            }
            else { convertedID = 0; }

            switch (feature)
            {
                case AvatarPiece.Hair:
                    playerAvatarData.Hair = convertedID;
                    break;
                case AvatarPiece.Eyes:
                    playerAvatarData.Eyes = convertedID;
                    break;
                case AvatarPiece.Nose:
                    playerAvatarData.Nose = convertedID;
                    break;
                case AvatarPiece.Mouth:
                    playerAvatarData.Mouth = convertedID;
                    break;
                case AvatarPiece.Clothes:
                    playerAvatarData.Clothes = convertedID;
                    break;
                case AvatarPiece.Hands:
                    playerAvatarData.Hands = convertedID;
                    break;
                case AvatarPiece.Feet:
                    playerAvatarData.Feet = convertedID;
                    break;
                default:
                    Debug.LogWarning($"Couldn't find {feature} in FeatureSlots");
                    break;
            }
        }

        public PlayerAvatar PlayerAvatar
        {
            get { return _playerAvatar; }
        }
    }
}
