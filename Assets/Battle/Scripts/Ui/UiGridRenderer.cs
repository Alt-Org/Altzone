using Battle.Scripts.Battle;
using UnityEngine;
using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Ui
{
    /// <summary>
    /// Display a grid overlay in Battle scene.
    /// </summary>
    internal class UiGridRenderer : MonoBehaviour
    {
        public CameraRotate CameraRotate;

        [SerializeField] private LineRenderer myLineRenderer;
        [SerializeField] private Color _gridColor1;
        [SerializeField] private Color _gridColor2;
        [SerializeField] private Color _gridColor3;
        [SerializeField] private Color _gridColor4;
        [SerializeField] private Color _lineColor;
        [SerializeField] private GameObject _gridTile;
        [SerializeField] private float _gridLineWidth = 0.03f;
        [SerializeField] private int _spriteSortingLayer;

        private Camera _camera;
        private IBattlePlayArea _battlePlayArea;
        private int _gridWidth;
        private int _gridHeight;
        private int _middleAreaHeight;
        private float _arenaWidth;
        private float _arenaHeight;
        private GameObject[,] _gridSquares;
        private SpriteRenderer[,] _gridSprites;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _gridWidth = _battlePlayArea.GridWidth;
            _gridHeight = _battlePlayArea.GridHeight;
            _arenaWidth = _battlePlayArea.ArenaWidth;
            _arenaHeight = _battlePlayArea.ArenaHeight;
            _middleAreaHeight = _battlePlayArea.MiddleAreaHeight;
            _camera = Context.GetBattleCamera.Camera;
        }

        private void Start()
        {
            //DrawLineGrid();
            var xPos = -_arenaWidth / 2;
            var yPos1 = -_arenaHeight / 2;
            var yPos2 = _arenaHeight / _gridHeight * _middleAreaHeight / 2;
            DrawChessGrid(xPos, yPos1, _gridColor1, _gridColor2);
            DrawChessGrid(xPos, yPos2, _gridColor3, _gridColor4);
        }

        private void DrawLineGrid()
        {
            myLineRenderer.startColor = _lineColor;
            myLineRenderer.endColor = _lineColor;
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
                _startPoint = new Vector3(_startXValue, _startYValue, _camera.nearClipPlane);
                _endPoint = new Vector3(_endXValue, _endYValue, _camera.nearClipPlane);

                _lineStart = _camera.ViewportToWorldPoint(_startPoint);
                _lineEnd = _camera.ViewportToWorldPoint(_endPoint);

                myLineRenderer.SetPosition(2 * i, _lineStart);
                myLineRenderer.SetPosition(2 * i + 1, _lineEnd);
            }
        }

        private void DrawChessGrid(float xPos, float yPos, Color gridColor1, Color gridColor2)
        {
            var teamAreaHeight = _gridHeight / 2 - _middleAreaHeight / 2;
            _gridSquares = new GameObject[teamAreaHeight, _gridWidth];
            _gridSprites = new SpriteRenderer[teamAreaHeight, _gridWidth];
            Texture2D texture = new Texture2D(1, 1);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0, 0));
            var yPosStart = yPos;
            var tileSize = new Vector2(_arenaWidth / _gridWidth, _arenaHeight / _gridHeight);
            for (int col = 0; col < _gridWidth; col++)
            {
                for (int row = 0; row < teamAreaHeight; row++)
                {
                    if (col % 2 == row % 2)
                    {
                        DrawGridTile(row, col, sprite, gridColor1, tileSize, xPos, yPos);
                        yPos += tileSize.y;
                    }
                    else
                    {
                        DrawGridTile(row, col, sprite, gridColor2, tileSize, xPos, yPos);
                        yPos += tileSize.y;
                    }
                }
                yPos = yPosStart;
                xPos += tileSize.x;
            }
            CameraRotate.enabled = true;
        }

        private void DrawGridTile(int row, int col, Sprite sprite, Color color, Vector2 tileSize, float xPos, float yPos)
        {
            _gridSquares[row, col] = Instantiate(_gridTile, new Vector3(xPos, yPos, 0), Quaternion.identity, this.transform);
            _gridSprites[row, col] = _gridSquares[row, col].GetComponentInChildren<SpriteRenderer>();
            _gridSprites[row, col].sprite = sprite;
            _gridSprites[row, col].color = color;
            _gridSprites[row, col].size = tileSize;
            _gridSprites[row, col].sortingOrder = _spriteSortingLayer;
        }
    }
}
