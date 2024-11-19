using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using System;

namespace MenuUi.Scripts.AvatarEditor
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField]private GameObject _colorButtonPrefab;
        [SerializeField]private GameObject _defaultColorButtonPrefab;
        [SerializeField]private List<Color> _colors;
        [SerializeField]private List<Transform> _colorButtonPositions;
        [SerializeField]private Transform _characterImageParent;
        private Image _colorChangeTarget;
        private CharacterClassID _characterClassID;
        private List<FeatureColor> _currentColors = new(){
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
        private FeatureSlot _currentlySelectedCategory;
        public void OnEnable()
        {
            // _colorChangeTarget = _characterImageParent.GetChild(0).GetChild(2).GetComponent<Image>();
            InstantiateColorButtons();
        }
        public void OnDisable()
        {
            DestroyColorButtons();
        }


        public void SelectFeature(FeatureSlot feature)
        {
            _currentlySelectedCategory = feature;
            _colorChangeTarget = _characterImageParent.GetChild(0).GetChild((int)feature).GetComponent<Image>();
        }

        private void InstantiateColorButtons()
        {
            for (int i = 0; i < _colors.Count; i++){
                int j = i;
                if(i == 0){
                    Button button = Instantiate(_defaultColorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                    button.onClick.AddListener(delegate{SetColor(_colors[j]);});
                }
                else{
                    Button button = Instantiate(_colorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                    button.GetComponent<Image>().color = _colors[i];
                    button.onClick.AddListener(delegate{SetColor(_colors[j]);});
                }
            }
        }
        private void DestroyColorButtons()
        {
            foreach(Transform pos in _colorButtonPositions){
                    if(pos.childCount > 0){
                        foreach(Transform child in pos){
                            Destroy(child.gameObject);
                        }
                    }
                }
        }
        private void SetColor(Color color)
        {
            _currentColors[(int)_currentlySelectedCategory] = (FeatureColor)_colors.IndexOf(color);
            if(_colorChangeTarget != null){
                _colorChangeTarget.color = color;
                if(_characterClassID == CharacterClassID.Confluent){
                    _colorChangeTarget.transform.GetChild(0).GetComponent<Image>().color = color;
                }
            }
        }
        private void SetDefaultColor()
        {

        }
        private void SetTransparentColor(){
            if(_colorChangeTarget != null){
                _colorChangeTarget.color = new Color(255,255,255,0);
                if(_characterClassID == CharacterClassID.Confluent){
                    _colorChangeTarget.transform.GetChild(0).GetComponent<Image>().color = new Color(255,255,255,0);
                }
            }
        }
        public void SetCharacterClassID(CharacterClassID id)
        {
            _characterClassID = id;
        }
        public List<FeatureColor> GetCurrentColors()
        {
            return _currentColors;
        }
        public void SetLoadedColors(List<FeatureColor> colors, List<FeatureID> features)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                SelectFeature((FeatureSlot)i);
                if(features[i] == FeatureID.None){
                    SetTransparentColor();
                }
                else if (features[i] == FeatureID.Default){
                    Debug.Log("feature was default when cioloring!");
                }
                else{
                    SetColor(_colors[(int)colors[i]]);
                }
                
            }
        }
        public void RestoreDefaultColor(FeatureSlot slot){
            _currentColors[(int)slot] = FeatureColor.White;
        }

        // internal void SetCurrentCategoryAndColors(FeatureSlot category, List<FeatureColor> colors) {
        //     _currentlySelectedCategory = category;
        //     _currentColors = colors;
        // }
    }
}