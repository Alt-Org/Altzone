using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor{
    public class AvatarScaler : MonoBehaviour
    {
        [SerializeField]private Transform _characterImageParent;
        private Transform _characterImage;
        [SerializeField] private List<Button> _buttons;
        [SerializeField] private Image _bookBackground;
        [SerializeField] private float _amountToScale;

        void Start(){
            _buttons[0].onClick.AddListener(ScaleDownHorizontally);
            _buttons[1].onClick.AddListener(ScaleUpVertically);
            _buttons[2].onClick.AddListener(ScaleUpHorizontally);
            _buttons[3].onClick.AddListener(ScaleDownVertically);
            _characterImage = _characterImageParent.GetChild(0);
        }
        void OnEnable(){
            _bookBackground.enabled = false;
        }
        void OnDisable(){
            _bookBackground.enabled = true;
        }
        private void ScaleUpVertically(){
            if(_characterImage.localScale.y < 2){
                _characterImage.localScale += new Vector3(0, _amountToScale, 0);
            }
            
        }
        private void ScaleDownVertically(){
            if(_characterImage.localScale.y > 1){
                _characterImage.localScale += new Vector3(0, -_amountToScale, 0);
            }
        }
        private void ScaleUpHorizontally(){
            if(_characterImage.localScale.x < 2){
                _characterImage.localScale += new Vector3(_amountToScale, 0, 0);
            }
        }
        private void ScaleDownHorizontally(){
            if(_characterImage.localScale.x > 1){
                _characterImage.localScale += new Vector3(-_amountToScale, 0, 0);
            }
        }
    }
    

}