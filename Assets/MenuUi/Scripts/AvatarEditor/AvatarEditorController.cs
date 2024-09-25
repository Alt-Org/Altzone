using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField]private AvatarEditorMode _defaultMode = AvatarEditorMode.FeaturePicker;
        private FeatureSlot _currentlySelectedCategory;
        void Start(){
            _switchModeButtons[0].onClick.AddListener(LoadNextMode);
            _switchModeButtons[1].onClick.AddListener(LoadPreviousMode);
            
        }
        void OnEnable(){
            foreach(GameObject mode in _modeList){
                mode.SetActive(false);
            }
            _currentMode = _defaultMode;
            GoIntoMode();
        }
        private void GoIntoMode(){
            
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                _modeList[0].GetComponent<FeaturePicker>().SetCharacterClassID(_characterLoader.GetCharacterClassID());
            }
            if (_currentMode == AvatarEditorMode.ColorPicker){
                _modeList[1].GetComponent<ColorPicker>().SelectFeature(_currentlySelectedCategory);
                _modeList[1].GetComponent<ColorPicker>().SetCharacterClassID(_characterLoader.GetCharacterClassID());
            }
            _modeList[(int)_currentMode].SetActive(true);
        }
        
        private void LoadNextMode(){
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                _currentlySelectedCategory = _modeList[0].GetComponent<FeaturePicker>().GetCurrentlySelectedCategory();
            }
            _modeList[(int)_currentMode].SetActive(false);
            _currentMode++;
            if ((int)_currentMode >= Enum.GetNames(typeof(AvatarEditorMode)).Length){
                _currentMode = 0;
            }
            GoIntoMode();
        }
        private void LoadPreviousMode(){
            if(_currentMode == AvatarEditorMode.FeaturePicker){
                _currentlySelectedCategory = _modeList[0].GetComponent<FeaturePicker>().GetCurrentlySelectedCategory();
            }
            _modeList[(int)_currentMode].SetActive(false);
            _currentMode--;
            if((int)_currentMode < 0){
                _currentMode = (AvatarEditorMode)Enum.GetNames(typeof(AvatarEditorMode)).Length-1;
            }
            GoIntoMode();
        }
    }
    enum AvatarEditorMode{
        FeaturePicker = 0,
        ColorPicker = 1,
        AvatarScaler = 2,
    }
}