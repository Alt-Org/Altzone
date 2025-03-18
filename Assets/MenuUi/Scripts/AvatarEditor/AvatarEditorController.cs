using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorController : AltMonoBehaviour
    {
        private PlayerData _currentPlayerData;

        [SerializeField] private float _timeoutSeconds = 10f;

        private enum AvatarEditorMode
        {
            FeaturePicker,
            ColorPicker,
            AvatarScaler,
        }
        private AvatarEditorMode _currentMode = AvatarEditorMode.FeaturePicker;

        [SerializeField] private CharacterLoader _characterLoader;
        [SerializeField] private List<GameObject> _modeList;
        [SerializeField] private List<Button> _switchModeButtons;
        [SerializeField] private Button _saveButton;
        [SerializeField] private AvatarEditorMode _defaultMode = AvatarEditorMode.FeaturePicker;
        [SerializeField] private AvatarVisualDataScriptableObject _visualDataScriptableObject;
        [SerializeField] private GameObject _avatarVisualsParent;
        private FeatureSlot _currentlySelectedCategory;
        
        private PlayerAvatar _playerAvatar;
        private FeaturePicker _featurePicker;
        private ColorPicker _colorPicker;
        private AvatarScaler _avatarScaler;

        void Awake()
        {
            _featurePicker = _modeList[0].GetComponent<FeaturePicker>();
            _colorPicker = _modeList[1].GetComponent<ColorPicker>(); 
            _avatarScaler = _modeList[2].GetComponent<AvatarScaler>();
        }

        void Start()
        {
            _saveButton.onClick.AddListener(() => StartCoroutine(SaveAvatarData()));
            _switchModeButtons[0].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.FeaturePicker);});
            _switchModeButtons[1].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.ColorPicker);});
            _switchModeButtons[2].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.AvatarScaler);});
        }

        void OnEnable()
        {
            _characterLoader.RefreshPlayerCurrentCharacter(CharacterLoaded);
            foreach(GameObject mode in _modeList){
                mode.SetActive(false);
            }
            _currentMode = _defaultMode;
        }

        private void CharacterLoaded()
        {
            StartCoroutine(LoadAvatarData());
            GoIntoMode(_defaultMode);
        }

        #region Mode selection

        private void GoIntoMode(AvatarEditorMode mode)
        {
            SetSaveableData();
            _modeList[(int)_currentMode].SetActive(false);
            _currentMode = mode;
            
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                //_featurePicker.RestoreDefaultColorToFeature(RestoreDefaultColorToFeature);
                _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
                
            }
            if (_currentMode == AvatarEditorMode.ColorPicker){
                _colorPicker.SelectFeature(_currentlySelectedCategory);
                _colorPicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            }
            
            _modeList[(int)_currentMode].SetActive(true);
        }

        private void RestoreDefaultColorToFeature()
        {
            _colorPicker.RestoreDefaultColor(_featurePicker.GetCurrentlySelectedCategory());
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

            if (_currentPlayerData.AvatarData == null)
            {
                
                Debug.Log("AvatarData is null. Using default data.");
                _playerAvatar = new(_featurePicker.GetCharacterDefaultFeature
                    (
                    $"{(_currentPlayerData.SelectedCharacterId.ToString())[0]}-{_currentPlayerData.SelectedCharacterId}")
                    );
            }
            else
                _playerAvatar = new(_currentPlayerData.AvatarData);

            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _featurePicker.SetLoadedFeatures(_playerAvatar.FeatureIds);

            _colorPicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _colorPicker.SetLoadedColors(_playerAvatar.Colors, _playerAvatar.FeatureIds);

            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);
        }

        #endregion

        #region Saving Data

        private void SetSaveableData()
        {
            _currentlySelectedCategory = _featurePicker.GetCurrentlySelectedCategory();
            //_playerAvatar.
            //_selectedScale = _avatarScaler.GetCurrentScale();
        }

        private IEnumerator SaveAvatarData()
        {
            bool? timeout = null;
            PlayerData playerData = null;
            PlayerData savePlayerData = _currentPlayerData;

            savePlayerData.AvatarData = new(_playerAvatar.Name,
                _featurePicker.GetCurrentlySelectedFeatures(),
                _colorPicker.GetCurrentColors(),
                _avatarScaler.GetCurrentScale());

            StartCoroutine(PlayerDataTransferer("save", savePlayerData, _timeoutSeconds, data => timeout = data, data => playerData = data));

            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;
        }

        #endregion
    }
}
