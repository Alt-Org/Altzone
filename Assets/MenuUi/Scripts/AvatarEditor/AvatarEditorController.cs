using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace MenuUi.scripts.AvatarEditor
{
    public class AvatarEditorController : AltMonoBehaviour
    {
        //Planning to add this to the original AvatarEditorController, but find it easier to do in it's own script while making it
        [SerializeField] private ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] private ScrollBarFeatureLoader _featureLoader;
        [SerializeField] private ColorGridLoader _colorLoader;
        [SerializeField] private CharacterLoader _characterLoader;
        [SerializeField] private float _timeoutSeconds = 10f;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
        [SerializeField] private FeatureSetter _featureSetter;
        [SerializeField] private AvatarVisualDataScriptableObject _visualDataScriptableObject;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _revertButton;
        [SerializeField] private Button _defaultButton;
        [SerializeField] private TextHandler _textHandler;

        private PlayerData _currentPlayerData;
        private PlayerAvatar _playerAvatar;

        // Start is called before the first frame update
        void Start()
        {
            _categoryLoader.SetCategoryCells((categoryId) => _featureLoader.RefreshFeatureListItems(categoryId));
            _categoryLoader.UpdateCellSize();
            _colorLoader.SetColorCells();
            _colorLoader.UpdateCellSize();
            _featureLoader.UpdateCellSize();
            _colorLoader.gameObject.SetActive(false);

            _saveButton.onClick.AddListener(() => StartCoroutine(SaveAvatarData()));
            _defaultButton.onClick.AddListener(() => SetDefaultAvatar());
            _revertButton.onClick.AddListener(() => RevertAvatarChanges());   

            StartCoroutine(ClickMiddleCategoryCellOnNextFrame());

        }

        private void OnEnable()
        {
            _characterLoader.RefreshPlayerCurrentCharacter();
            StartCoroutine(LoadAvatarData());
            _textHandler.SetRandomSpeechBubbleText();
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

            _featureSetter.SetCharacterClassID(_characterLoader.GetCharacterClass());
            _featureSetter.SetLoadedFeatures(_playerAvatar);
        }


        private void SetDefaultAvatar()
        {
            _playerAvatar = new(_avatarDefaultReference.GetByCharacterId(_currentPlayerData.SelectedCharacterId)[0]);

            _featureSetter.SetCharacterClassID(_characterLoader.GetCharacterClass());
            _featureSetter.SetLoadedFeatures(_playerAvatar);
        }

        private void RevertAvatarChanges()
        {
            SetAllAvatarFeatures();
        }

        private IEnumerator SaveAvatarData()
        {
            bool? timeout = null;
            PlayerData playerData = null;
            PlayerData savePlayerData = _currentPlayerData;

            savePlayerData.AvatarData = new(_playerAvatar.Name,
                null,
                "FFFFFF",
                new Vector2(1, 1));

            var features = Enum.GetValues(typeof(FeatureSlot));
            foreach (FeatureSlot feature in features)
            {
                AssignPartToPlayerData(savePlayerData.AvatarData, feature, _playerAvatar.GetPartId(feature));
            }
            StartCoroutine(SavePlayerData(savePlayerData, p => playerData = p));
            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;

            List<AvatarPiece> pieceIDs = Enum.GetValues(typeof(AvatarPiece)).Cast<AvatarPiece>().ToList();
            foreach (AvatarPiece piece in pieceIDs)
            {
                _visualDataScriptableObject.SetAvatarPiece(piece, _featureSetter.GetCurrentlySelectedFeatureSprite(piece));
            }

            AvatarDesignLoader.Instance.InvokeOnAvatarDesignUpdate();

            GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        private void AssignPartToPlayerData(AvatarData playerAvatarData, FeatureSlot feature, string id)
        {
            int convertedID;
            if (int.TryParse(id, out int intID))
            {
                convertedID = intID;
            }
            else { convertedID = 0; }

            switch (feature)
            {
                case FeatureSlot.Hair:
                    playerAvatarData.Hair = convertedID;
                    break;
                case FeatureSlot.Eyes:
                    playerAvatarData.Eyes = convertedID;
                    break;
                case FeatureSlot.Nose:
                    playerAvatarData.Nose = convertedID;
                    break;
                case FeatureSlot.Mouth:
                    playerAvatarData.Mouth = convertedID;
                    break;
                case FeatureSlot.Body:
                    playerAvatarData.Clothes = convertedID;
                    break;
                case FeatureSlot.Hands:
                    playerAvatarData.Hands = convertedID;
                    break;
                case FeatureSlot.Feet:
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
