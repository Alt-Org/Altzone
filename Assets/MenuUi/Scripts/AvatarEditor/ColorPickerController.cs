using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerController : MonoBehaviour
{
    [SerializeField]private GameObject _colorButtonPrefab;
    [SerializeField]private GameObject _defaultColorButtonPrefab;
    [SerializeField]private List<Color> _colors;
    [SerializeField]private List<Transform> _colorButtonPositions;
    private Image _currentlySelectedFeature;
    public void OnEnable(){
        InstantiateColorButtons();
    }
    public void OnDisable(){
        DestroyColorButtons();
    }


    public void SelectFeature(Image feature){
        _currentlySelectedFeature = feature;
    }

    private void InstantiateColorButtons(){
        for (int i = 0; i < _colors.Count; i++){
            if(i == 0){
                Button button = Instantiate(_defaultColorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                button.onClick.AddListener(SetDefaultColor);
            }
            else{
                Button button = Instantiate(_colorButtonPrefab, _colorButtonPositions[i]).GetComponent<Button>();
                button.GetComponent<Image>().color = _colors[i];
                button.onClick.AddListener(delegate{SetColor(_colors[i]);});
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
        if(_currentlySelectedFeature != null){
            _currentlySelectedFeature.color = color;
        }
    }


}
