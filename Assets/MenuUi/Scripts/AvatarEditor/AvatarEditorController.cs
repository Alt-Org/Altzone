using System;
using System.Collections;
using System.Collections.Generic;
//using ExitGames.Client.Photon.StructWrapping;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorController : MonoBehaviour
    {
        [SerializeField]private CharacterLoader _characterLoader;
        private AvatarEditorMode _currentMode = 0;
        [SerializeField]private List<GameObject> _modeList;
        [SerializeField]private List<Button> _switchModeButtons;
        [SerializeField]private Button _saveButton;
        [SerializeField]private TMP_InputField _nameInput;
        [SerializeField]private AvatarEditorMode _defaultMode = AvatarEditorMode.FeaturePicker;
        private FeatureSlot _currentlySelectedCategory;
        
        
        private Vector2 _selectedScale;
        private PlayerAvatar _playerAvatar;
        private FeaturePicker _featurePicker;
        private ColorPicker _colorPicker;
        private AvatarScaler _avatarScaler;
        void Awake(){
            _featurePicker = _modeList[0].GetComponent<FeaturePicker>();
            _colorPicker = _modeList[1].GetComponent<ColorPicker>(); 
            _avatarScaler = _modeList[2].GetComponent<AvatarScaler>();
        }
        void Start(){
            _saveButton.onClick.AddListener(SaveAvatarData);
            _switchModeButtons[0].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.FeaturePicker);});
            _switchModeButtons[1].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.ColorPicker);});
            _switchModeButtons[2].onClick.AddListener(delegate{GoIntoMode(AvatarEditorMode.AvatarScaler);});
        }
        void OnEnable(){
            // _colorPicker.SetCurrentCategoryAndColors(_currentlySelectedCategory, _selectedColors);
            _characterLoader.RefreshPlayerCurrentCharacter(CharacterLoaded);
            // ResetAvatarDataToDefaults();
            foreach(GameObject mode in _modeList){
                mode.SetActive(false);
            }
            _currentMode = _defaultMode;

        }
        private void CharacterLoaded()
        {
            LoadAvatarData();
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
        private void RestoreDefaultColorToFeature(){
            Debug.Log("restoring default color to slot: " + _currentlySelectedCategory);
            _colorPicker.RestoreDefaultColor(_featurePicker.GetCurrentlySelectedCategory());
            // _selectedColors[(int)_currentlySelectedCategory] = FeatureColor.White;
            // SetSaveableData();
        }
        
        // private void LoadNextMode(){
        //     SetSaveableData();
        //     _modeList[(int)_currentMode].SetActive(false);
        //     _currentMode++;
        //     if ((int)_currentMode >= Enum.GetNames(typeof(AvatarEditorMode)).Length){
        //         _currentMode = 0;
        //     }
        //     GoIntoMode();
        // }
        // private void LoadPreviousMode(){
        //     SetSaveableData();
        //     _modeList[(int)_currentMode].SetActive(false);
        //     _currentMode--;
        //     if((int)_currentMode < 0){
        //         _currentMode = (AvatarEditorMode)Enum.GetNames(typeof(AvatarEditorMode)).Length-1;
        //     }
        //     GoIntoMode();
        // }
        #endregion
        #region Loading Data



        private void LoadAvatarData()
        {
            _playerAvatar = new PlayerAvatar(PlayerPrefs.GetString("CharacterName"), LoadFeaturesFromPrefs(), LoadColorsFromPrefs(), LoadScaleFromPrefs());
            _nameInput.text = _playerAvatar.Name;

            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _featurePicker.SetLoadedFeatures(_playerAvatar.Features);

            _colorPicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _colorPicker.SetLoadedColors(_playerAvatar.Colors, _playerAvatar.Features);

            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);

            
        }
        private List<FeatureID> LoadFeaturesFromPrefs()
        {
            List<FeatureID> features = new();
            for(int i = 0; i < 8; i++){
                features.Add((FeatureID)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Feature"));
                // Debug.Log("Loaded Feature: " + features[i].ToString() + " from a key of: " + ((FeatureSlot)i).ToString());
            }
            return features;
        }
        private List<FeatureColor> LoadColorsFromPrefs()
        {
            List<FeatureColor> colors = new();
            for (int i = 0; i < 8; i++){
                colors.Add((FeatureColor)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Color"));
            }
            return colors;
        }
        private Vector2 LoadScaleFromPrefs()
        {
            return new Vector2(PlayerPrefs.GetFloat("ScaleX"), PlayerPrefs.GetFloat("ScaleY"));
        }
        // private void ResetAvatarDataToDefaults(){
        //     for(int i = 0; i < _playerAvatar.Features.Count; i++){
        //         PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Feature", 0);
        //         Debug.Log("Reset slot " + ((FeatureSlot)i).ToString() + " , now it is: + " + PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Feature"));
        //     }
        //     for(int i = 0; i < _playerAvatar.Colors.Count; i++){
        //         PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Color", 0);
        //     }
        // }
        #endregion
        #region Saving Data
        private void SetSaveableData(){
            _currentlySelectedCategory = _featurePicker.GetCurrentlySelectedCategory();
            _selectedScale = _avatarScaler.GetCurrentScale();
            // foreach(Feature feature in _selectedFeatures)
            //     Debug.Log("Data saved: " + feature.ToString());
            // foreach(FeatureColors color in _selectedColors)
            //     Debug.Log("Color saved: " + color.ToString());
        }

        private void SaveAvatarData()
        {
            SetSaveableData();
            SaveName();
            SaveFeaturesToPrefs();
            SaveColorsToPrefs();
            SaveScaleToPrefs();
            PlayerPrefs.Save();
        }
        private void SaveName()
        {
            string characterName = _nameInput.text;
            // Debug.Log("Character name saved: " + characterName);

            _playerAvatar.Name = characterName;
            PlayerPrefs.SetString("CharacterName", characterName);
        }
        private void SaveFeaturesToPrefs()
        {
            List<FeatureID> features = _featurePicker.GetCurrentlySelectedFeatures();
            for(int i = 0; i < features.Count; i++){
                // Debug.Log("Saving feature: " +_selectedFeatures[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Feature", (int)features[i]);
            }
        }
        private void SaveColorsToPrefs()
        {
            List<FeatureColor> colors = _colorPicker.GetCurrentColors();
            for(int i = 0; i < _playerAvatar.Colors.Count; i++){
                Debug.Log("Saving color: " +colors[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Color", (int)colors[i]);
            }
        }
        private void SaveScaleToPrefs()
        {
            PlayerPrefs.SetFloat("ScaleX", _selectedScale.x);
            PlayerPrefs.SetFloat("ScaleY", _selectedScale.y);
        }

        #endregion
    }
    enum AvatarEditorMode
    {
        FeaturePicker = 0,
        ColorPicker = 1,
        AvatarScaler = 2,
    }
}
