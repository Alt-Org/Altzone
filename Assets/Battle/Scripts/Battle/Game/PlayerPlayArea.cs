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

        [Header("Arena Border")]
        [SerializeField] private GameObject _arenaBorderTop;
        [SerializeField] private GameObject _arenaBorderBottom;
        [SerializeField] private GameObject _arenaBorderRight;
        [SerializeField] private GameObject _arenaBorderLeft;

        [Header("Soul Walls")]
        [SerializeField] private GameObject _soulWallTeamAlpha;
        [SerializeField] private GameObject _soulWallTeamBeta;
        public int soulWallSegmentHeahlt;

        [Header("Player Start Positions")]
        [SerializeField] private GridPos _startPositionAlpha1;
        [SerializeField] private GridPos _startPositionAlpha2;
        [SerializeField] private GridPos _startPositionBeta1;
        [SerializeField] private GridPos _startPositionBeta2;

        [SerializeField] private GameObject[] _soulWallTeamAlphaSegments;
        [SerializeField] private GameObject[] _soulWallTeamBateSegments;

        private BoxCollider2D _arenaBorderBottomCollider;
        private BoxCollider2D _arenaBorderTopCollider;
        private BoxCollider2D _arenaBorderRightCollider;
        private BoxCollider2D _arenaBorderLeftCollider;

        private const float SoulWallSegmentSpriteWidth = 2.35f;
        private const int SegmentsPerSoulWall = 5;

        private Rect _playStartAreaAlpha;
        private Rect _playStartAreaBeta;

        private void Awake()
        {
            SetupArenaBorders();
            SetupSoulWalls();

            float middleAreaHeight = _middleAreaHeight * _arenaHeight / _gridHeight;
            float squareHeight = _arenaHeight / _gridHeight;

            _playStartAreaAlpha = new Rect(-_arenaWidth / 2, -_arenaHeight / 2 + squareHeight * _brickHeight, _arenaWidth, _arenaHeight / 2 - middleAreaHeight / 2 - squareHeight * _brickHeight);
            _playStartAreaBeta = new Rect(-_arenaWidth / 2, middleAreaHeight / 2, _arenaWidth, _arenaHeight / 2 - middleAreaHeight / 2 - squareHeight * _brickHeight);
        }

        private void SetupArenaBorders()
        {
            _arenaBorderTopCollider = _arenaBorderTop.GetComponent<BoxCollider2D>();
            _arenaBorderBottomCollider = _arenaBorderBottom.GetComponent<BoxCollider2D>();
            _arenaBorderRightCollider = _arenaBorderRight.GetComponent<BoxCollider2D>();
            _arenaBorderLeftCollider = _arenaBorderLeft.GetComponent<BoxCollider2D>();

            _arenaBorderTopCollider.size = new Vector2(_arenaWidth, _arenaHeight);
            _arenaBorderBottomCollider.size = new Vector2(_arenaWidth, _arenaHeight);
            _arenaBorderRightCollider.size = new Vector2(_arenaWidth, _arenaHeight * 2);
            _arenaBorderLeftCollider.size = new Vector2(_arenaWidth, _arenaHeight * 2);

            _arenaBorderTop.transform.position = new Vector2(0, _arenaHeight);
            _arenaBorderBottom.transform.position = new Vector2(0, -_arenaHeight);
            _arenaBorderRightCollider.transform.position = new Vector2(_arenaWidth, 0);
            _arenaBorderLeftCollider.transform.position = new Vector2(-_arenaWidth, 0);
        }

        private void SetupSoulWalls()
        {
            _soulWallTeamAlphaSegments = new GameObject[SegmentsPerSoulWall];
            _soulWallTeamBateSegments = new GameObject[SegmentsPerSoulWall];

            for (int i = 0; i < SegmentsPerSoulWall; i++)
            {
                _soulWallTeamAlphaSegments[i] = _soulWallTeamAlpha.transform.GetChild(i).gameObject;
                _soulWallTeamBateSegments[i] = _soulWallTeamBeta.transform.GetChild(i).gameObject;
                _soulWallTeamAlphaSegments[i].GetComponent<SpriteRenderer>().size = new Vector2(SoulWallSegmentSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _soulWallTeamBateSegments[i].GetComponent<SpriteRenderer>().size = new Vector2(SoulWallSegmentSpriteWidth, _brickHeight * _arenaHeight / _gridHeight);
                _soulWallTeamAlphaSegments[i].transform.position = new Vector2(_soulWallTeamAlphaSegments[i].transform.position.x, -_arenaHeight / 2 + (_arenaHeight / _gridHeight));
                _soulWallTeamBateSegments[i].transform.position = new Vector2(_soulWallTeamBateSegments[i].transform.position.x, _arenaHeight / 2 - (_arenaHeight / _gridHeight));
                _soulWallTeamAlphaSegments[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / SegmentsPerSoulWall, _brickHeight * _arenaHeight / _gridHeight);
                _soulWallTeamBateSegments[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / SegmentsPerSoulWall, _brickHeight * _arenaHeight / _gridHeight);
            }
        }

        internal float ArenaWidth => _arenaWidth;
        internal float ArenaHeight => _arenaHeight;
        internal float ArenaScaleFactor => _arenaHeight / _gridHeight;
        internal int GridWidth => _gridWidth;
        internal int GridHeight => _gridHeight;
        internal int MiddleAreaHeight => _middleAreaHeight;

        internal Rect GetPlayerPlayArea(BattleTeamNumber teamNumber)
        {
            return teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => _playStartAreaAlpha,
                BattleTeamNumber.TeamBeta => _playStartAreaBeta,
                _ => throw new UnityException($"Invalid Team Number {teamNumber}"),
            };
        }

        internal GridPos GetPlayerStartPosition(int playerPos)
        {
            GridPos startPosition = playerPos switch
            {
                PhotonBattle.PlayerPosition1 => _startPositionAlpha1,
                PhotonBattle.PlayerPosition2 => _startPositionAlpha2,
                PhotonBattle.PlayerPosition3 => _startPositionBeta1,
                PhotonBattle.PlayerPosition4 => _startPositionBeta2,
                _ => throw new UnityException($"Invalid player position {playerPos}"),
            };
            return startPosition;
        }
    }
}
