using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Sets the arena, player's starting positions and team play areas.
    /// </summary>
    internal class PlayerPlayArea : MonoBehaviour
    {
        [Tooltip("Arena width in world coordinates"), SerializeField] private float _arenaWidth;
        [Tooltip("Arena height in world coordinates"), SerializeField] private float _arenaHeight;
        [Tooltip("How many squares is the grid height"), SerializeField] private int _gridHeight;
        [Tooltip("How many squares is the grid width"), SerializeField] private int _gridWidth;
        [Tooltip("Middle area height in grid squares"), SerializeField] private int _middleAreaHeight;
        [Tooltip("How many squares is the brick wall height"), SerializeField] private int _brickHeight;

        [Header("Wall Colliders"), SerializeField] private GameObject _leftTopWall;
        [SerializeField] private GameObject _rightTopWall;
        [SerializeField] private GameObject _leftBottomWall;
        [SerializeField] private GameObject _rightBottomWall;
        [SerializeField] private GameObject _leftMidWall;
        [SerializeField] private GameObject _rightMidWall;
        [SerializeField] private GameObject _bottomWall;
        [SerializeField] private GameObject _topWall;

        [SerializeField] private GameObject _alphaTeamBrickWall;
        [SerializeField] private GameObject _betaTeamBrickWall;
        [SerializeField] private GameObject _alphaTeamLeftBricks;
        [SerializeField] private GameObject _betaTeamLeftBricks;
        [SerializeField] private GameObject _betaTeamRightBricks;
        [SerializeField] private GameObject _alphaTeamRightBricks;

        public int BrickHealth;

        [Header("Player Start Positions"), SerializeField] private GridPos _startPositionAlpha1;
        [SerializeField] private GridPos _startPositionAlpha2;
        [SerializeField] private GridPos _startPositionBeta1;
        [SerializeField] private GridPos _startPositionBeta2;

        [SerializeField] private GameObject[] _alphaTeamBricks;
        [SerializeField] private GameObject[] _betaTeamBricks;
        [SerializeField] private GameObject[] _alphaTeamBricksLeft;
        [SerializeField] private GameObject[] _alphaTeamBricksRight;
        [SerializeField] private GameObject[] _betaTeamBricksLeft;
        [SerializeField] private GameObject[] _betaTeamBricksRight;


        private BoxCollider2D _rightTopWallCollider;
        private BoxCollider2D _leftTopWallCollider;
        private BoxCollider2D _rightBottomWallCollider;
        private BoxCollider2D _leftBottomWallCollider;
        private BoxCollider2D _rightMidWallCollider;
        private BoxCollider2D _leftMidWallCollider;

        private BoxCollider2D _bottomWallCollider;
        private BoxCollider2D _topWallCollider;

        private const float BrickSpriteWidth = 2.35f;
        private const int BricksPerWall = 5;

        private Rect _playStartAreaAlpha;
        private Rect _playStartAreaBeta;

        private void Awake()
        {
            SetupArenaBorders();
            SetupBottomBrickWalls();
            SetupSideBrickWalls();

            var middleAreaHeight = _middleAreaHeight * _arenaHeight / _gridHeight;
            var squareHeight = _arenaHeight / _gridHeight;
            _playStartAreaAlpha = new Rect(-_arenaWidth / 2, -_arenaHeight / 2 + squareHeight * _brickHeight, _arenaWidth, _arenaHeight / 2 - middleAreaHeight / 2 - squareHeight * _brickHeight);
            _playStartAreaBeta = new Rect(-_arenaWidth / 2, middleAreaHeight / 2, _arenaWidth, _arenaHeight / 2 - middleAreaHeight / 2 - squareHeight * _brickHeight);
        }

        private void SetupArenaBorders()
        {

            _rightTopWallCollider = _rightTopWall.GetComponent<BoxCollider2D>();
            _leftTopWallCollider = _leftTopWall.GetComponent<BoxCollider2D>();
            _rightBottomWallCollider = _rightBottomWall.GetComponent<BoxCollider2D>();
            _leftBottomWallCollider = _leftBottomWall.GetComponent<BoxCollider2D>();
            _bottomWallCollider = _bottomWall.GetComponent<BoxCollider2D>();
            _topWallCollider = _topWall.GetComponent<BoxCollider2D>();
            _rightMidWallCollider = _rightMidWall.GetComponent<BoxCollider2D>();
            _leftMidWallCollider = _leftMidWall.GetComponent<BoxCollider2D>();

            var arenaSize = new Vector2(_arenaWidth, _arenaHeight);
            _rightTopWallCollider.size = arenaSize;
            _leftTopWallCollider.size = arenaSize;
            _rightBottomWallCollider.size = arenaSize;
            _leftBottomWallCollider.size = arenaSize;
            _bottomWallCollider.size = arenaSize;
            _topWallCollider.size = arenaSize;
            _rightMidWallCollider.size = new Vector2(_arenaWidth, 1.6f);
            _leftMidWallCollider.size = new Vector2(_arenaWidth, 1.6f);

            _leftMidWall.transform.position = new Vector2(-_arenaWidth, 0);
            _rightMidWall.transform.position = new Vector2(_arenaWidth, 0);
            _leftTopWall.transform.position = new Vector2(-_arenaWidth, .7f + _arenaHeight / 2);
            _rightTopWall.transform.position = new Vector2(_arenaWidth, .7f + _arenaHeight / 2);
            _leftBottomWall.transform.position = new Vector2(-_arenaWidth, -.7f + -_arenaHeight / 2);
            _rightBottomWall.transform.position = new Vector2(_arenaWidth, -.7f + -_arenaHeight / 2);
            _topWall.transform.position = new Vector2(0, _arenaHeight);
            _bottomWall.transform.position = new Vector2(0, -_arenaHeight);
        }

        private void SetupBottomBrickWalls()
        {
            _alphaTeamBricks = new GameObject[BricksPerWall];
            _betaTeamBricks = new GameObject[BricksPerWall];
            for (int i = 0; i < BricksPerWall; i++)
            {
                _alphaTeamBricks[i] = _alphaTeamBrickWall.transform.GetChild(i).gameObject;
                _betaTeamBricks[i] = _betaTeamBrickWall.transform.GetChild(i).gameObject;
                _alphaTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _alphaTeamBricks[i].transform.position = new Vector2(_alphaTeamBricks[i].transform.position.x, -_arenaHeight / 2 + _arenaHeight / (_gridHeight));
                _betaTeamBricks[i].transform.position = new Vector2(_betaTeamBricks[i].transform.position.x, _arenaHeight / 2 - _arenaHeight / (_gridHeight));
                _alphaTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
            }
        }

        private void SetupSideBrickWalls()
        {
            _alphaTeamBricksLeft = new GameObject[BricksPerWall];
            _alphaTeamBricksRight = new GameObject[BricksPerWall];
            _alphaTeamBricksLeft = new GameObject[BricksPerWall];
            _alphaTeamBricksRight = new GameObject[BricksPerWall];
            _betaTeamBricksLeft = new GameObject[BricksPerWall];
            _betaTeamBricksRight = new GameObject[BricksPerWall];
            for (int i = 0; i < BricksPerWall; i++)
            {
                _alphaTeamBricksLeft[i] = _alphaTeamLeftBricks.transform.GetChild(i).gameObject;
                _alphaTeamBricksRight[i] = _betaTeamRightBricks.transform.GetChild(i).gameObject;
                _betaTeamBricksLeft[i] = _betaTeamLeftBricks.transform.GetChild(i).gameObject;
                _betaTeamBricksRight[i] = _alphaTeamRightBricks.transform.GetChild(i).gameObject;

                _alphaTeamBricksLeft[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _alphaTeamBricksRight[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricksLeft[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricksRight[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);

                _alphaTeamBricksLeft[i].transform.position = new Vector2(-_arenaHeight / 3.3f + _arenaHeight / (_gridHeight), _alphaTeamBricksLeft[i].transform.position.y);
                _alphaTeamBricksRight[i].transform.position = new Vector2( -_arenaHeight / 3.3f + _arenaHeight / (_gridHeight), _alphaTeamBricksRight[i].transform.position.y);
                _betaTeamBricksLeft[i].transform.position = new Vector2(_arenaHeight / 3.3f - _arenaHeight / (_gridHeight), _betaTeamBricksLeft[i].transform.position.y);
                _betaTeamBricksRight[i].transform.position = new Vector2(_arenaHeight / 3.3f - _arenaHeight / (_gridHeight), _betaTeamBricksRight[i].transform.position.y);

                _alphaTeamBricksLeft[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
                _alphaTeamBricksRight[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricksLeft[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
                _betaTeamBricksRight[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _brickHeight * _arenaHeight / _gridHeight);
            }
        }

        internal float ArenaWidth => _arenaWidth;
        internal float ArenaHeight => _arenaHeight;
        internal float ArenaScaleFactor => _arenaHeight / _gridHeight;
        internal int GridWidth => _gridWidth;
        internal int GridHeight => _gridHeight;
        internal int MiddleAreaHeight => _middleAreaHeight;

        internal Rect GetPlayerPlayArea(int teamNumber)
        {
            Rect playArea;
            switch (teamNumber)
            {
                case PhotonBattle.TeamAlphaValue:
                    playArea = _playStartAreaAlpha;
                    break;
                case PhotonBattle.TeamBetaValue:
                    playArea = _playStartAreaBeta;
                    break;
                default:
                    throw new UnityException($"Invalid Team Number {teamNumber}");
            }
            return playArea;
        }

        internal GridPos GetPlayerStartPosition(int playerPos)
        {
            GridPos startPosition;
            switch (playerPos)
            {
                case PhotonBattle.PlayerPosition1:
                    startPosition = _startPositionAlpha1;
                    break;
                case PhotonBattle.PlayerPosition2:
                    startPosition = _startPositionAlpha2;
                    break;
                case PhotonBattle.PlayerPosition3:
                    startPosition = _startPositionBeta1;
                    break;
                case PhotonBattle.PlayerPosition4:
                    startPosition = _startPositionBeta2;
                    break;
                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            return startPosition;
        }
    }
}
