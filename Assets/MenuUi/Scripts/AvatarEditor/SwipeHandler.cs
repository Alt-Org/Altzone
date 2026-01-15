using UnityEngine;
using Prg.Scripts.Common;

namespace MenuUi.Scripts.AvatarEditor
{
    public enum SwipeDirection{
        None,
        Left,
        Right,
        Up,
        Down
    }
    public class SwipeHandler : MonoBehaviour
    {
        private Vector2 _startTouchPosition;
        private Vector2 _endTouchPosition;
        private Vector2 _positionDifferential;
        [SerializeField]private float _swipeThreshold = 100f;
        public delegate void Swipe(SwipeDirection direction , Vector2 swipeStartPoint, Vector2 swipeEndPoint);
        public static event Swipe OnSwipe;

        private void Update(){

            if(ClickStateHandler.GetClickState() is ClickState.Start)
            {
                _startTouchPosition = ClickStateHandler.GetClickPosition();
            }
            if(ClickStateHandler.GetClickState() is ClickState.End)
            {
                _endTouchPosition = ClickStateHandler.GetClickPosition();
                _positionDifferential = _endTouchPosition - _startTouchPosition;
                if(_positionDifferential.magnitude > _swipeThreshold)
                {
                    if (Mathf.Abs(_positionDifferential.x) > Mathf.Abs(_positionDifferential.y))
                    {
                        if(_endTouchPosition.x < _startTouchPosition.x)
                        {
                            OnSwipe?.Invoke(SwipeDirection.Left, _startTouchPosition, _endTouchPosition);
                        }
                        else if(_endTouchPosition.x > _startTouchPosition.x)
                        {
                            OnSwipe?.Invoke(SwipeDirection.Right, _startTouchPosition, _endTouchPosition);
                        }
                    }
                    else {
                        if(_endTouchPosition.y < _startTouchPosition.y)
                        {
                            OnSwipe?.Invoke(SwipeDirection.Down, _startTouchPosition, _endTouchPosition);
                        }
                        else if(_endTouchPosition.y > _startTouchPosition.y)
                        {
                            OnSwipe?.Invoke(SwipeDirection.Up, _startTouchPosition, _endTouchPosition);
                        }
                    }
                    
                }
            }
        }
    }
}


