using UnityEngine;

namespace Battle.View.Game
{
    public class BattleCameraTest : MonoBehaviour
    {
        [SerializeField] private bool _active;
        [SerializeField] private float _scale;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private bool _rotate;

        private bool _activePrev;
        private float _scalePrev;
        private Vector2 _offsetPrev;
        private bool _rotatePrev;

        private void Update()
        {
            if (!_active)
            {
                if (_active == _activePrev) return;
                BattleCamera.UnsetView();
                _activePrev = _active;
                return;
            }

            if (
                _active == _activePrev &&
                _scale  == _scalePrev  &&
                _offset == _offsetPrev &&
                _rotate == _rotatePrev
            ) return;


            if (_scale <= 0.0f) _scale = _scalePrev;

            _offset = new Vector2(
                Mathf.Clamp01(_offset.x),
                Mathf.Clamp01(_offset.y)
            );

            BattleCamera.SetView(_scale, _offset, _rotate);

            _activePrev = _active;
            _scalePrev = _scale;
            _offsetPrev = _offset;
            _rotatePrev = _rotate;
        }
    }
}
