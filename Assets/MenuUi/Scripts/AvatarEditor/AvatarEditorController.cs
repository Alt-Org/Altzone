using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorController : MonoBehaviour
    {
        private AvatarEditorMode _currentMode = 0;
        [SerializeField]private List<GameObject> _modeList;
        [SerializeField]private List<Button> _switchModeButtons;
        private FeatureSlot _currentlySelectedCategory;
        void Start(){
            _switchModeButtons[0].onClick.AddListener(LoadNextMode);
            _switchModeButtons[1].onClick.AddListener(LoadPreviousMode);
            GoIntoMode();
        }
        private void GoIntoMode(){
            _modeList[(int)_currentMode].SetActive(true);
            if (_currentMode == AvatarEditorMode.ColorPicker){
                _modeList[1].GetComponent<ColorPicker>().SelectFeature(_currentlySelectedCategory);
            }
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