using Altzone.Scripts.Config;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    /// <summary>
    /// Display a grid overlay in Battle scene.
    /// </summary>
    internal class BattleUiGridHandler : MonoBehaviour
    {
        [SerializeField] private LineRenderer myLineRenderer;

        private Camera myMainCamera;

        private bool _isGridDisabled;
        private int _gridWidth;
        private int _gridHeight;
        private float _gridLineWidth;

        private Color _gridColor;

        private void Awake()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Features;

            _isGridDisabled = variables._isDisableBattleUiGrid;
            _gridWidth = variables._battleUiGridWidth;
            _gridHeight = variables._battleUiGridHeight;
            _gridLineWidth = variables._battleUiGridLineWidth;
            _gridColor = variables._battleUiGridColor;
        }

        private void Start()
        {
            if (_isGridDisabled) { return; }

            myMainCamera = Camera.main;

            myLineRenderer.startColor = _gridColor;
            myLineRenderer.endColor = _gridColor;
            myLineRenderer.startWidth = _gridLineWidth;
            myLineRenderer.endWidth = _gridLineWidth;
            myLineRenderer.positionCount = (_gridHeight + _gridWidth) * 2 + 4;

            Vector3 _lineStart;
            Vector3 _lineEnd;
            Vector3 _startPoint;
            Vector3 _endPoint;

            int _remainder;
            float _startXValue;
            float _startYValue;
            float _endXValue;
            float _endYValue;

            for (int i = 0; i <= _gridWidth + _gridHeight + 1; i++)
            {
                _remainder = i % 2;

                if (i <= _gridWidth && _gridWidth % 2 != 0)
                {
                    _startXValue = (float)(i) / _gridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 1 - _remainder;
                }
                else if (i < _gridWidth && _gridWidth % 2 == 0)
                {
                    _startXValue = (float)(i) / _gridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 1 - _remainder;
                }
                else if (_gridWidth % 2 == 0 && i > _gridWidth)
                {
                    _startXValue = _remainder;
                    _startYValue = 1f - (float)(i - _gridWidth - 1) / _gridHeight;
                    _endXValue = 1 - _remainder;
                    _endYValue = _startYValue;
                }
                else if (_gridWidth % 2 != 0)
                {
                    _startXValue = _remainder;
                    _startYValue = (float)(i - _gridWidth - 1) / _gridHeight;
                    _endXValue = 1 - _remainder;
                    _endYValue = _startYValue;
                }
                else
                {
                    _startXValue = (float)(i) / _gridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 0.5f - _remainder;
                }
                _startPoint = new Vector3(_startXValue, _startYValue, myMainCamera.nearClipPlane);
                _endPoint = new Vector3(_endXValue, _endYValue, myMainCamera.nearClipPlane);

                _lineStart = myMainCamera.ViewportToWorldPoint(_startPoint);
                _lineEnd = myMainCamera.ViewportToWorldPoint(_endPoint);

                myLineRenderer.SetPosition(2 * i, _lineStart);
                myLineRenderer.SetPosition(2 * i + 1, _lineEnd);
            }
        }
    }
}
