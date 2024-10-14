using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class AvatarEditorSwipe : MonoBehaviour
    {
        private Vector2 _startTouchPosition;
        private Vector2 _endTouchPosition;
        private Vector2 _positionDifferential;
        private RectTransform _touchArea;
        [SerializeField]private float _swipeThreshold = 100f;
        private Action _swipeRight;
        private Action _swipeLeft;
        private Action _swipeUp;
        private Action _swipeDown;

        private void Start(){
            _touchArea = GetComponent<RectTransform>();
        }

        private void Update(){
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _startTouchPosition = Input.GetTouch(0).position;
            }
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                _endTouchPosition = Input.GetTouch(0).position;
                _positionDifferential = _endTouchPosition - _startTouchPosition;
                if(RectTransformUtility.RectangleContainsScreenPoint(_touchArea, _startTouchPosition) && _positionDifferential.magnitude > _swipeThreshold)
                {
                    if (Mathf.Abs(_positionDifferential.x) > Mathf.Abs(_positionDifferential.y))
                    {
                        if(_endTouchPosition.x < _startTouchPosition.x)
                        {
                            _swipeLeft?.Invoke();
                        }
                        else if(_endTouchPosition.x > _startTouchPosition.x)
                        {
                            _swipeRight?.Invoke();
                        }
                    }
                    else {
                        if(_endTouchPosition.y < _startTouchPosition.y)
                        {
                            _swipeDown?.Invoke();
                        }
                        else if(_endTouchPosition.y > _startTouchPosition.y)
                        {
                            _swipeUp?.Invoke();
                        }
                    }
                    
                }
            }
        }
        public void SetSwipeActions(Action swipeLeft = null, Action swipeUp= null, Action swipeRight= null, Action swipeDown= null)
        {
            _swipeLeft = swipeLeft;
            _swipeUp = swipeUp;
            _swipeRight = swipeRight;
            _swipeDown = swipeDown;
            
        }
    }
}


