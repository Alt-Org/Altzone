using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public void OnEnable(){
            // _colorChangeTarget = _characterImageParent.GetChild(0).GetChild(2).GetComponent<Image>();
            InstantiateColorButtons();
        }
        public void OnDisable(){
            DestroyColorButtons();
        }


        public void SelectFeature(FeatureSlot feature){

            _colorChangeTarget = _characterImageParent.GetChild(0).GetChild((int)feature + 2).GetComponent<Image>();
        }

        private void InstantiateColorButtons(){
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
        private void DestroyColorButtons(){
            foreach(Transform pos in _colorButtonPositions){
                    if(pos.childCount > 0){
                        foreach(Transform child in pos){
                            Destroy(child.gameObject);
                        }
                    }
                }
        }
        private void SetDefaultColor(){

        }
        private void SetColor(Color color){
            if(_colorChangeTarget != null){
                _colorChangeTarget.color = color;
            }
        }


    }
}