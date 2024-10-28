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
        private List<FeatureID> _selectedFeatures = new(){
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
        };
        private List<FeatureColor> _selectedColors = new(){
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,
            FeatureColor.White,     
        };
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
            _switchModeButtons[0].onClick.AddListener(LoadNextMode);
            _switchModeButtons[1].onClick.AddListener(LoadPreviousMode);
            
        }
        void OnEnable(){
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
            GoIntoMode();
        }
        #region Mode selection
        private void GoIntoMode()
        {
            
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                _featurePicker.RestoreDefaultColorToFeature(RestoreDefaultColorToFeature);
                _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
                
            }
            if (_currentMode == AvatarEditorMode.ColorPicker){
                _modeList[1].GetComponent<ColorPicker>().SelectFeature(_currentlySelectedCategory);
                _modeList[1].GetComponent<ColorPicker>().SetCharacterClassID(_characterLoader.GetCharacterClassID());
            }
            
            _modeList[(int)_currentMode].SetActive(true);
        }
        private void RestoreDefaultColorToFeature(){
            // Debug.Log("restore default color");
            // _selectedColors[(int)_currentlySelectedCategory] = FeatureColor.White;
            // SetSaveableData();
        }
        
        private void LoadNextMode(){
            SetSaveableData();
            _modeList[(int)_currentMode].SetActive(false);
            _currentMode++;
            if ((int)_currentMode >= Enum.GetNames(typeof(AvatarEditorMode)).Length){
                _currentMode = 0;
            }
            GoIntoMode();
        }
        private void LoadPreviousMode(){
            SetSaveableData();
            _modeList[(int)_currentMode].SetActive(false);
            _currentMode--;
            if((int)_currentMode < 0){
                _currentMode = (AvatarEditorMode)Enum.GetNames(typeof(AvatarEditorMode)).Length-1;
            }
            GoIntoMode();
        }
        #endregion
        #region Saving and loading Data

        private void SetSaveableData(){
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                _currentlySelectedCategory = _featurePicker.GetCurrentlySelectedCategory();
                _selectedFeatures = _featurePicker.GetCurrentlySelectedFeature();
            }
            // else if(_currentMode == AvatarEditorMode.ColorPicker){
            //     _selectedColors[(int)_currentlySelectedCategory] = _modeList[1].GetComponent<ColorPicker>().GetCurrentColor();
            // }
            _selectedColors[(int)_currentlySelectedCategory] = _colorPicker.GetCurrentColor();
            _selectedScale = _avatarScaler.GetCurrentScale();
            // foreach(Feature feature in _selectedFeatures)
            //     Debug.Log("Data saved: " + feature.ToString());
            // foreach(FeatureColors color in _selectedColors)
            //     Debug.Log("Color saved: " + color.ToString());
        }








        private void LoadAvatarData()
        {
            _playerAvatar = new PlayerAvatar(PlayerPrefs.GetString("CharacterName"), LoadFeatures(), LoadColors(), LoadScale());
            _nameInput.text = _playerAvatar.Name;
            _featurePicker.gameObject.SetActive(true);
            _featurePicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _featurePicker.GetComponent<FeaturePicker>().SetLoadedFeatures(_playerAvatar.Features);
            _featurePicker.gameObject.SetActive(false);
            _colorPicker.gameObject.SetActive(true);
            _colorPicker.SetCharacterClassID(_characterLoader.GetCharacterClassID());
            _colorPicker.SetLoadedColors(_playerAvatar.Colors, _playerAvatar.Features);
            _colorPicker.gameObject.SetActive(false);
            _avatarScaler.gameObject.SetActive(true);
            _avatarScaler.SetLoadedScale(_playerAvatar.Scale);
            _avatarScaler.gameObject.SetActive(false);
            // _modeList[1].SetActive(true);
            // _modeList[1].GetComponent<ColorPicker>().SetLoadedColors(_playerAvatar.Colors);
            // _modeList[1].SetActive(false);
            // Debug.Log("Player chosen hair is: " +_playerAvatar.Features[0].ToString());
            
        }
        private List<FeatureID> LoadFeatures()
        {
            List<FeatureID> features = new();
            for(int i = 0; i < 8; i++){
                features.Add((FeatureID)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Feature"));
                // Debug.Log("Loaded Feature: " + features[i].ToString() + " from a key of: " + ((FeatureSlot)i).ToString());
            }
            return features;
        }
        private List<FeatureColor> LoadColors()
        {
            List<FeatureColor> colors = new();
            for (int i = 0; i < 8; i++){
                colors.Add((FeatureColor)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()+"Color"));
            }
            return colors;
        }
        private Vector2 LoadScale()
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

        private void SaveAvatarData()
        {
            SetSaveableData();
            SaveName();
            SaveFeatures();
            SaveColors();
            SaveScale();
            PlayerPrefs.Save();
        }
        private void SaveName()
        {
            string characterName = _nameInput.text;
            // Debug.Log("Character name saved: " + characterName);

            _playerAvatar.Name = characterName;
            PlayerPrefs.SetString("CharacterName", characterName);
        }
        private void SaveFeatures()
        {
            for(int i = 0; i < _selectedFeatures.Count; i++){
                // Debug.Log("Saving feature: " +_selectedFeatures[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Feature", (int)_selectedFeatures[i]);
            }
        }
        private void SaveColors()
        {
            for(int i = 0; i < _playerAvatar.Colors.Count; i++){
                // Debug.Log("Saving color: " +_selectedColors[i] + " to a string key named: " + ((FeatureSlot)i).ToString());
                PlayerPrefs.SetInt(((FeatureSlot)i).ToString()+"Color", (int)_selectedColors[i]);
            }
        }
        private void SaveScale()
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
