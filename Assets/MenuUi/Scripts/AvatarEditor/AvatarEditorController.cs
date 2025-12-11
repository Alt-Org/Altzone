using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorController : AltMonoBehaviour
    {
        private PlayerData _currentPlayerData;

        [SerializeField] private float _timeoutSeconds = 10f;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;

        private enum AvatarEditorMode
        {
            FeaturePicker,
            ColorPicker,
            AvatarScaler,
        }
        private AvatarEditorMode _currentMode = AvatarEditorMode.FeaturePicker;

        [SerializeField] private CharacterLoader _characterLoader;
        [Space]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _defaultButton;
        [SerializeField] private Button _revertButton;
        [SerializeField] private Button _featureModeButton;
        [SerializeField] private Button _colorModeButton;
        [SerializeField] private Button _scaleModeButton;
        [Space]
        [SerializeField] private AvatarEditorMode _defaultMode = AvatarEditorMode.FeaturePicker;
        [SerializeField] private AvatarVisualDataScriptableObject _visualDataScriptableObject;
        [SerializeField] private GameObject _avatarVisualsParent;
        [SerializeField] private GameObject _featureButtonsBase;
        [SerializeField] private FeaturePicker _featurePicker;
        [SerializeField] private ColorPicker _colorPicker;
        [SerializeField] private AvatarScaler _avatarScaler;

        private FeatureSlot _currentlySelectedCategory;
        private PlayerAvatar _playerAvatar;

        void Start()
        {
            _saveButton.onClick.AddListener(() => StartCoroutine(SaveAvatarData()));
            _defaultButton.onClick.AddListener(() => SetDefaultAvatar());
            //_revertButton.onClick.AddListener(() => RevertAvatarChanges());

            /*if (_featureModeButton != null && _featurePicker != null)
                _featureModeButton.onClick.AddListener(delegate { GoIntoMode(AvatarEditorMode.FeaturePicker); });
            if (_colorModeButton != null && _colorPicker != null)
                _colorModeButton.onClick.AddListener(delegate { GoIntoMode(AvatarEditorMode.ColorPicker); });
            if (_scaleModeButton != null && _avatarScaler != null)
                _scaleModeButton.onClick.AddListener(delegate { GoIntoMode(AvatarEditorMode.AvatarScaler); });*/
        }

        void OnEnable()
        {
            _characterLoader.RefreshPlayerCurrentCharacter();
            if (_featurePicker != null)
                _featurePicker.gameObject.SetActive(false);
            if (_colorPicker != null)
                _colorPicker.gameObject.SetActive(false);
            if (_avatarScaler != null)
                _avatarScaler.gameObject.SetActive(false);
            _currentMode = _defaultMode;
            CharacterLoaded();
        }

        private void CharacterLoaded()
        {
            StartCoroutine(LoadAvatarData());
            //GoIntoMode(_defaultMode);
        }

        #region Mode selection

        private void GoIntoMode(AvatarEditorMode mode)
        {
            SetSaveableData();
            _featurePicker.gameObject.SetActive(false);
            _colorPicker.gameObject.SetActive(false);
            _avatarScaler.gameObject.SetActive(false);
            _currentMode = mode;

            _featureButtonsBase.SetActive(mode != AvatarEditorMode.AvatarScaler);

            if (_currentMode == AvatarEditorMode.FeaturePicker)
            {
                _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClass());
                _featurePicker.gameObject.SetActive(true);
                return;
            }

            if (_currentMode == AvatarEditorMode.ColorPicker)
            {
                _colorPicker.SelectFeature(_currentlySelectedCategory);
                _colorPicker.gameObject.SetActive(true);
                return;
            }

            if (_currentMode == AvatarEditorMode.AvatarScaler)
            {
                _avatarScaler.gameObject.SetActive(true);
                return;
            }
        }

        #endregion

        #region Loading Data

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

        private void SetDefaultAvatar()
        {
            _playerAvatar = new(_avatarDefaultReference.GetByCharacterId(_currentPlayerData.SelectedCharacterId)[0]);

            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClass());
            _featurePicker.SetLoadedFeatures(_playerAvatar);

            _colorPicker.SetLoadedColors(_playerAvatar);
            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);
        }

        private void RevertAvatarChanges()
        {
            SetAllAvatarFeatures();
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

            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClass());
            _featurePicker.SetLoadedFeatures(_playerAvatar);

            _colorPicker.SetLoadedColors(_playerAvatar);
            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);
        }

        #endregion

        #region Saving Data

        private void SetSaveableData()
        {
            _currentlySelectedCategory = _featurePicker.GetCurrentlySelectedCategory();
        }

        private IEnumerator SaveAvatarData()
        {
            bool? timeout = null;
            PlayerData playerData = null;
            PlayerData savePlayerData = _currentPlayerData;

            savePlayerData.AvatarData = new(_playerAvatar.Name,
                null,
                _colorPicker.GetCurrentColors(),
                _avatarScaler.GetCurrentScale());

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
                _visualDataScriptableObject.SetAvatarPiece(piece, _featurePicker.GetCurrentlySelectedFeatureSprite(piece));
            }

            // Käytetään julkista Color-propertya, ei private-kenttää
            _visualDataScriptableObject.Color = _colorPicker.GetCurrentColorsAsColors();

            AvatarDesignLoader.Instance.InvokeOnAvatarDesignUpdate();

            GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        private void AssignPartToPlayerData(AvatarData playerAvatarData, FeatureSlot feature, string id)
        {
            int convertedID;
            if(int.TryParse(id, out int intID))
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
        #endregion
    }
}

