using Battle.Scripts.Battle;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    /// <summary>
    /// Display a grid overlay in Battle scene.
    /// </summary>
    internal class UiGridHandler : MonoBehaviour
    {
        [SerializeField] private LineRenderer myLineRenderer;
        [SerializeField] private Color _gridColor1;
        [SerializeField] private Color _gridColor2;
        [SerializeField] private Color _lineColor;
        [SerializeField] private GameObject _shieldGridTile;
        [SerializeField] private float _gridLineWidth = 0.03f;
        [SerializeField] private int _spriteSortingLayer;

        private Camera _camera;
        private IBattlePlayArea _battlePlayArea;
        private int _movementGridWidth;
        private int _movementGridHeight;
        private int _shieldGridWidth;
        private int _shieldGridHeight;
        private float _arenaWidth;
        private float _arenaHeight;
        private GameObject[,] _shieldGridSquares;
        private SpriteRenderer[,] _shieldSpriteSquares;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea();
            _movementGridWidth = _battlePlayArea.MovementGridWidth;
            _movementGridHeight = _battlePlayArea.MovementGridHeight;
            _shieldGridWidth = _battlePlayArea.ShieldGridWidth;
            _shieldGridHeight = _battlePlayArea.ShieldGridHeight;
            _arenaWidth = _battlePlayArea.ArenaWidth;
            _arenaHeight = _battlePlayArea.ArenaHeight;
            _camera = Context.GetBattleCamera.Camera;
        }

        private void Start()
        {
            myLineRenderer.startColor = _lineColor;
            myLineRenderer.endColor = _lineColor;
            myLineRenderer.startWidth = _gridLineWidth;
            myLineRenderer.endWidth = _gridLineWidth;
            myLineRenderer.positionCount = (_movementGridHeight + _movementGridWidth) * 2 + 4;

            Vector3 _lineStart;
            Vector3 _lineEnd;
            Vector3 _startPoint;
            Vector3 _endPoint;

            int _remainder;
            float _startXValue;
            float _startYValue;
            float _endXValue;
            float _endYValue;

            for (int i = 0; i <= _movementGridWidth + _movementGridHeight + 1; i++)
            {
                _remainder = i % 2;

                if (i <= _movementGridWidth && _movementGridWidth % 2 != 0)
                {
                    _startXValue = (float)(i) / _movementGridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 1 - _remainder;
                }
                else if (i < _movementGridWidth && _movementGridWidth % 2 == 0)
                {
                    _startXValue = (float)(i) / _movementGridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 1 - _remainder;
                }
                else if (_movementGridWidth % 2 == 0 && i > _movementGridWidth)
                {
                    _startXValue = _remainder;
                    _startYValue = 1f - (float)(i - _movementGridWidth - 1) / _movementGridHeight;
                    _endXValue = 1 - _remainder;
                    _endYValue = _startYValue;
                }
                else if (_movementGridWidth % 2 != 0)
                {
                    _startXValue = _remainder;
                    _startYValue = (float)(i - _movementGridWidth - 1) / _movementGridHeight;
                    _endXValue = 1 - _remainder;
                    _endYValue = _startYValue;
                }
                else
                {
                    _startXValue = (float)(i) / _movementGridWidth;
                    _startYValue = _remainder;
                    _endXValue = _startXValue;
                    _endYValue = 0.5f - _remainder;
                }
                _startPoint = new Vector3(_startXValue, _startYValue, _camera.nearClipPlane);
                _endPoint = new Vector3(_endXValue, _endYValue, _camera.nearClipPlane);

                _lineStart = _camera.ViewportToWorldPoint(_startPoint);
                _lineEnd = _camera.ViewportToWorldPoint(_endPoint);

                myLineRenderer.SetPosition(2 * i, _lineStart);
                myLineRenderer.SetPosition(2 * i + 1, _lineEnd);
            }
            _shieldGridSquares = new GameObject[_shieldGridHeight, _shieldGridWidth];
            _shieldSpriteSquares = new SpriteRenderer[_shieldGridHeight, _shieldGridWidth];
            Texture2D texture = new Texture2D(1, 1);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0, 0));

            var squareSize = new Vector2(_arenaWidth / _shieldGridWidth, _arenaHeight / _shieldGridHeight);
            var xPos = -_arenaWidth / 2;
            var yPos = -_arenaHeight / 2;
            for (int col = 0; col < _shieldGridWidth; col++)
            {
                for (int row = 0; row < _shieldGridHeight; row++)
                {
                    if (col % 2 == row % 2)
                    {
                        _shieldGridSquares[row, col] = Instantiate(_shieldGridTile, new Vector3(xPos, yPos, 0), Quaternion.identity, this.transform);
                        _shieldSpriteSquares[row, col] = _shieldGridSquares[row, col].GetComponentInChildren<SpriteRenderer>();
                        _shieldSpriteSquares[row, col].sprite = sprite;
                        _shieldSpriteSquares[row, col].color = _gridColor1;
                        _shieldSpriteSquares[row, col].size = squareSize;
                        _shieldSpriteSquares[row, col].sortingOrder = _spriteSortingLayer;
                        yPos += squareSize.y;
                    }
                    else
                    {
                        _shieldGridSquares[row, col] = Instantiate(_shieldGridTile, new Vector3(xPos, yPos, 0), Quaternion.identity, this.transform);
                        _shieldSpriteSquares[row, col] = _shieldGridSquares[row, col].GetComponentInChildren<SpriteRenderer>();
                        _shieldSpriteSquares[row, col].sprite = sprite;
                        _shieldSpriteSquares[row, col].color = _gridColor2;
                        _shieldSpriteSquares[row, col].size = squareSize;
                        _shieldSpriteSquares[row, col].sortingOrder = _spriteSortingLayer;
                        yPos += squareSize.y;
                    }
                }
                yPos = -_arenaHeight / 2;
                xPos += squareSize.x;
            }
        }
    }
}
