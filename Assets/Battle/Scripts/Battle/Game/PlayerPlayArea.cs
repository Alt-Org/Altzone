using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class PlayerPlayArea : MonoBehaviour, IBattlePlayArea
    {
        [Tooltip("Arena width in world coordinates"), SerializeField] private float _arenaWidth;
        [Tooltip("Arena height in world coordinates"), SerializeField] private float _arenaHeigth;
        [Tooltip("Middle area height in shield grid squares"), SerializeField] private float _middleAreaHeight;
        [Tooltip("How many squares is the shield grid height"), SerializeField] private int _shieldGridHeight;
        [Tooltip("How many squares is the shield grid width"), SerializeField] private int _shieldGridWidth;
        [Tooltip("How many movement grid squares in one shield grid square per side"), SerializeField] private int _movementGridMultiplier;

        [Header("Wall Colliders"), SerializeField] private GameObject _leftWall;
        [SerializeField] private GameObject _rightWall;
        [SerializeField] private GameObject _bottomWall;
        [SerializeField] private GameObject _topWall;
        [SerializeField] private GameObject _middleArea;
        [Tooltip("Set top and bottom wall colliders as triggers"), SerializeField] private bool _isBackWallsTriggers;

        [SerializeField] private GameObject _blueTeamBrickWall;
        [SerializeField] private GameObject _redTeamBrickWall;

        private GameObject[] _blueTeamBricks;
        private GameObject[] _redTeamBricks;

        private BoxCollider2D _rightWallCollider;
        private BoxCollider2D _leftWallCollider;
        private BoxCollider2D _bottomWallCollider;
        private BoxCollider2D _topWallCollider;

        private int _movementGridHeight;
        private int _movementGridWidth;
        private const float BrickSpriteWidth = 2.35f;
        private const int BricksPerWall = 5;

        private Rect _playAreaBlue;
        private Rect _playAreaRed;

        public float ArenaWidth => _arenaWidth;
        public float ArenaHeight => _arenaHeigth;

        public int ShieldGridWidth => _shieldGridWidth;
        public int ShieldGridHeight => _shieldGridHeight;
        public int MovementGridWidth => _movementGridWidth;
        public int MovementGridHeight => _movementGridHeight;

        private void Awake()
        {
            _rightWallCollider = _rightWall.GetComponent<BoxCollider2D>();
            _leftWallCollider = _leftWall.GetComponent<BoxCollider2D>();
            _bottomWallCollider = _bottomWall.GetComponent<BoxCollider2D>();
            _topWallCollider = _topWall.GetComponent<BoxCollider2D>();

            var arenaSize = new Vector2(_arenaWidth, _arenaHeigth);
            _rightWallCollider.size = arenaSize;
            _bottomWallCollider.size = arenaSize;
            _leftWallCollider.size = arenaSize;
            _topWallCollider.size = arenaSize;
            _bottomWallCollider.isTrigger = _isBackWallsTriggers;
            _topWallCollider.isTrigger = _isBackWallsTriggers;

            _leftWall.transform.position = new Vector2(-_arenaWidth, 0);
            _rightWall.transform.position = new Vector2(_arenaWidth, 0);
            _topWall.transform.position = new Vector2(0, _arenaHeigth);
            _bottomWall.transform.position = new Vector2(0, -_arenaHeigth);

            var middleAreaHeight = _middleAreaHeight * _arenaHeigth / _shieldGridHeight;
            _middleArea.transform.localScale = new Vector2(_arenaWidth, middleAreaHeight);

            _playAreaBlue = new Rect(-_arenaWidth / 2, -_arenaHeigth / 2, _arenaWidth, _arenaHeigth / 2 - middleAreaHeight / 2);
            _playAreaRed = new Rect(-_arenaWidth / 2, middleAreaHeight / 2, _arenaWidth, _arenaHeigth / 2 - middleAreaHeight / 2);
            _movementGridHeight = _movementGridMultiplier * _shieldGridHeight;
            _movementGridWidth = _movementGridMultiplier * _shieldGridWidth;

            _blueTeamBricks = new GameObject[BricksPerWall];
            _redTeamBricks = new GameObject[BricksPerWall];

            for (int i = 0; i < BricksPerWall; i++)
            {
                _blueTeamBricks[i] = _blueTeamBrickWall.transform.GetChild(i).gameObject;
                _redTeamBricks[i] = _redTeamBrickWall.transform.GetChild(i).gameObject;
                _blueTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _arenaHeigth / _shieldGridHeight);
                _redTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _arenaHeigth / _shieldGridHeight);
                _blueTeamBricks[i].transform.position = new Vector2(_blueTeamBricks[i].transform.position.x, -_arenaHeigth / 2 + _arenaHeigth / (2 * _shieldGridHeight));
                _redTeamBricks[i].transform.position = new Vector2(_redTeamBricks[i].transform.position.x, _arenaHeigth / 2 - _arenaHeigth / (2 * _shieldGridHeight));
                _blueTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _arenaHeigth / _shieldGridHeight);
                _redTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _arenaHeigth / _shieldGridHeight);
            }
        }

        public Rect GetPlayerPlayArea(int playerPos)
        {
            Rect playArea;
            switch (playerPos)
            {
                case 1:
                    playArea = _playAreaBlue;
                    break;

                case 2:
                    playArea = _playAreaBlue;
                    break;

                case 3:
                    playArea = _playAreaRed;
                    break;

                case 4:
                    playArea = _playAreaRed;
                    break;

                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            return playArea;
        }
    }
}
