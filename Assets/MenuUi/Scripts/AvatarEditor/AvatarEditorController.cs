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
        
        //private Vector2 _selectedScale;
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
                _featurePicker.RestoreDefaultColorToFeature(RestoreDefaultColorToFeature);
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

            // _nameInput.text = _playerAvatar.Name;
            StartCoroutine(PlayerDataTransferer("get", null, data => timeout = data, data => playerData = data));

            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;

            if (_currentPlayerData.AvatarData == null)
            {
                Debug.Log("AvatarData is null. Using default data.");
                //_featurePicker.GetDefaultAvatar();
                _playerAvatar = new(new List<FeatureID>() { 0,0,0,0,0,0,0,0,0 } );
                //yield break;
            }
            else
                _playerAvatar = new(_currentPlayerData.AvatarData);

            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _featurePicker.SetLoadedFeatures(_playerAvatar.Features);

            _colorPicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _colorPicker.SetLoadedColors(_playerAvatar.Colors, _playerAvatar.Features);

            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);
        }

        //private List<FeatureID> LoadFeaturesFromPrefs()
        //{
        //    List<FeatureID> features = new();
        //    for(int i = 0; i < Enum.GetNames(typeof(FeatureSlot)).Length-1; i++){
        //        features.Add((FeatureID)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Feature"));
        //        Debug.Log("Loaded Feature: " + features[i].ToString() + " from a key of: " + ((FeatureSlot)i).ToString());
        //    }
        //    return features;
        //}

        //private List<FeatureColor> LoadColorsFromPrefs()
        //{
        //    List<FeatureColor> colors = new();
        //    for (int i = 0; i < Enum.GetNames(typeof(FeatureSlot)).Length-1; i++){
        //        colors.Add((FeatureColor)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Color"));
        //    }
        //    return colors;
        //}

        //private Vector2 LoadScaleFromPrefs()
        //{
        //    return new Vector2(PlayerPrefs.GetFloat("ScaleX"), PlayerPrefs.GetFloat("ScaleY"));
        //}

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
                _playerAvatar.ToFeaturesListInt(_featurePicker.GetCurrentlySelectedFeatures()),
                _playerAvatar.ToFeatureColorListInt(_colorPicker.GetCurrentColors()),
                _avatarScaler.GetCurrentScale());

            StartCoroutine(PlayerDataTransferer("save", savePlayerData, data => timeout = data, data => playerData = data));

            yield return new WaitUntil(() => ((timeout != null) || (playerData != null)));

            if (playerData == null)
                yield break;

            _currentPlayerData = playerData;
        }

        // private void SaveName()
        // {
        //     string characterName = _nameInput.text;
        //     // Debug.Log("Character name saved: " + characterName);

        //     _playerAvatar.Name = characterName;
        //     PlayerPrefs.SetString("CharacterName", characterName);
        // }

        //private void SaveFeaturesToPrefs()
        //{
        //    List<FeatureID> features = _featurePicker.GetCurrentlySelectedFeatures();
        //    for(int i = 0; i < features.Count; i++){
        //        // Debug.Log("Saving feature: " +_selectedFeatures[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
        //        PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Feature", (int)features[i]);
        //    }
        //}

        //private void SaveColorsToPrefs()
        //{
        //    List<FeatureColor> colors = _colorPicker.GetCurrentColors();
        //    for(int i = 0; i < _playerAvatar.Colors.Count; i++){
        //        Debug.Log("Saving color: " +colors[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
        //        PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Color", (int)colors[i]);
        //    }
        //}

        //private void SaveScaleToPrefs()
        //{
        //    PlayerPrefs.SetFloat("ScaleX", _selectedScale.x);
        //    PlayerPrefs.SetFloat("ScaleY", _selectedScale.y);
        //}

        //private void SaveDataToScriptableObject()
        //{
        //    _visualDataScriptableObject.sprites.Clear();
        //    _visualDataScriptableObject.colors.Clear();
        //    foreach(Image image in _avatarVisualsParent.GetComponentsInChildren<Image>())
        //    {
        //        _visualDataScriptableObject.sprites.Add(image.sprite);
        //        _visualDataScriptableObject.colors.Add(image.color);
        //    }
        //}

        private IEnumerator SavePlayerData(PlayerData playerData, System.Action<PlayerData> callback) //TODO: Remove when available in AltMonoBehaviour.
        {
            //Cant' save to server because server manager doesn't have functionality!
            //Storefront.Get().SavePlayerData(playerData, callback);

            //if (callback == null)
            //{
            //    StartCoroutine(ServerManager.Instance.UpdatePlayerToServer( playerData., content =>
            //    {
            //        if (content != null)
            //            callback(new(content));
            //        else
            //        {
            //            Debug.LogError("Could not connect to server and save player");
            //            return;
            //        }
            //    }));
            //}

            //yield return new WaitUntil(() => callback != null);

            //Testing code
            callback(playerData);

            yield return true;
        }

        #endregion

        /// <summary>
        /// Used to get and save player data to/from server.
        /// </summary>
        /// <param name="operationType">"get" or "save"</param>
        /// <param name="unsavedData">If saving: insert unsaved data.<br/> If getting: insert <c>null</c>.</param>
        /// <param name="timeoutCallback">Returns value if timeout with server.</param>
        /// <param name="dataCallback">Returns <c>PlayerData</c>.</param>
        private IEnumerator PlayerDataTransferer(string operationType, PlayerData unsavedData, System.Action<bool> timeoutCallback, System.Action<PlayerData> dataCallback)
        {
            PlayerData receivedData = null;
            bool? timeout = null;
            Coroutine playerCoroutine;

            switch (operationType.ToLower())
            {
                case "get":
                    {
                        //Get player data.
                        playerCoroutine = StartCoroutine(CoroutineWithTimeout(GetPlayerData, receivedData, _timeoutSeconds, timeoutCallBack => timeout = timeoutCallBack, data => receivedData = data));
                        break;
                    }
                case "save":
                    {
                        //Save player data.
                        playerCoroutine = StartCoroutine(CoroutineWithTimeout(SavePlayerData, unsavedData, receivedData, _timeoutSeconds, timeoutCallBack => timeout = timeoutCallBack, data => receivedData = data));
                        break;
                    }
                default: Debug.LogError($"Received: {operationType}, when expecting \"get\" or \"save\"."); yield break;
            }

            yield return new WaitUntil(() => (receivedData != null || timeout != null));

            if (receivedData == null)
            {
                timeoutCallback(true);
                Debug.LogError($"Player data operation: \"{operationType}\" timeout or null.");
                yield break;
            }

            dataCallback(receivedData);
        }
    }
}
