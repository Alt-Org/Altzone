using System;
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

        [SerializeField] private float _maxScaling = 8;
        [SerializeField] private float _minScaling = 0.1f;

        private Touch _touchOne;
        private Touch _touchTwo;
        private Vector3 _initialScale;
        private Vector2 _positionDifferential;
        private float _initialDistance;
        private RectTransform _touchArea;

        /*void Start(){
            _buttons[0].onClick.AddListener(ScaleDownHorizontally);
            _buttons[1].onClick.AddListener(ScaleUpVertically);
            _buttons[2].onClick.AddListener(ScaleUpHorizontally);
            _buttons[3].onClick.AddListener(ScaleDownVertically);
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                foreach(Button button in _buttons)
                {
                    button.gameObject.SetActive(false);
                }
            }           
        }*/
        /*void OnEnable()
        {
            _bookBackground.enabled = false;
            SetCharacterImage();
        }
        void OnDisable()
        {
            _bookBackground.enabled = true;
        }*/

        void Update()
        {
            //remember to change input to touch
            if (Input.touchCount >= 2)
            {
                _touchOne = Input.GetTouch(0);
                _touchTwo = Input.GetTouch(1);
                if(_touchOne.phase == TouchPhase.Ended || _touchOne.phase == TouchPhase.Canceled ||
                    _touchTwo.phase == TouchPhase.Ended || _touchTwo.phase == TouchPhase.Canceled)
                {
                    return;
                }
                if(_touchOne.phase == TouchPhase.Began || _touchTwo.phase==TouchPhase.Began)
                {
                    _initialScale = _characterImage.localScale;
                    _initialDistance = Vector2.Distance(_touchOne.position, _touchTwo.position);
                }
                else
                {
                    if(Mathf.Approximately(_initialDistance, 0))
                    {
                        return;
                    }
                    float currentDistance = Vector2.Distance(_touchOne.position, _touchTwo.position);
                    _positionDifferential = _touchOne.position - _touchTwo.position;
                    float factor = currentDistance / _initialDistance;
                    if(Mathf.Abs(_positionDifferential.x) > Mathf.Abs(_positionDifferential.y))
                    {
                        if(_initialScale.x * factor <= _maxScaling && _initialScale.x * factor >= _minScaling)
                        {
                            _characterImage.localScale = new Vector3(_initialScale.x * factor, _characterImage.localScale.y, _characterImage.localScale.z);
                        }
                        else{
                            Debug.Log("Max or Min x scaling achieved!");
                        }
                        
                    }
                    else if(Mathf.Abs(_positionDifferential.x) < Mathf.Abs(_positionDifferential.y))
                    {
                        if(_initialScale.y * factor <= _maxScaling && _initialScale.y * factor >= _minScaling)
                        {
                            _characterImage.localScale = new Vector3(_characterImage.localScale.x, _initialScale.y * factor, _characterImage.localScale.z);
                        }
                        else{
                            Debug.Log("Max or Min Y scaling achieved!");
                        }
                        
                    }
                    // _characterImage.localScale =  _initialScale * factor;
                }
            }
    }
        private void ScaleUpVertically()
        {
            if(_characterImage.localScale.y < _maxScaling){
                _characterImage.localScale += new Vector3(0, _amountToScale, 0);
            }
            
        }
        private void ScaleDownVertically()
        {
            if(_characterImage.localScale.y > _minScaling){
                _characterImage.localScale += new Vector3(0, -_amountToScale, 0);
            }
        }
        private void ScaleUpHorizontally()
        {
            if(_characterImage.localScale.x < _maxScaling){
                _characterImage.localScale += new Vector3(_amountToScale, 0, 0);
            }
        }
        private void ScaleDownHorizontally()
        {
            if(_characterImage.localScale.x > _minScaling){
                _characterImage.localScale += new Vector3(-_amountToScale, 0, 0);
            }
        }

        public Vector2 GetCurrentScale()
        {
            return _characterImage.localScale;
        }

        public void SetLoadedScale(Vector2 scale) 
        {
            SetCharacterImage();
            float x = scale.x;
            float y = scale.y;
            if(x< _minScaling)
            {
                x = _minScaling;
            }
            else if(x> _maxScaling)
            {
                x = _maxScaling;
            }
            if(y< _minScaling)
            {
                y = _minScaling;
            }
            else if (y> _maxScaling)
            {
                y = _maxScaling;
            }
            _characterImage.localScale = new Vector2(x, y);
        }
        private void SetCharacterImage()
        {
            _characterImage = _characterImageParent.GetChild(0);
        }
    }
    

}
