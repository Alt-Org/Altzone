using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class PlayerPlayArea : MonoBehaviour, IBattlePlayArea
    {
        [Tooltip("Arena size in world coordinates"), SerializeField] private Vector2 _arenaSize;
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

        private BoxCollider2D _rightWallCollider;
        private BoxCollider2D _leftWallCollider;
        private BoxCollider2D _bottomWallCollider;
        private BoxCollider2D _topWallCollider;

        private int _movementGridHeight;
        private int _movementGridWidth;

        private Rect _playAreaBlue;
        private Rect _playAreaRed;

        public Vector2 ArenaSize => _arenaSize;

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

            _rightWallCollider.size = ArenaSize;
            _bottomWallCollider.size = ArenaSize;
            _leftWallCollider.size = ArenaSize;
            _topWallCollider.size = ArenaSize;
            _bottomWallCollider.isTrigger = _isBackWallsTriggers;
            _topWallCollider.isTrigger = _isBackWallsTriggers;

            _leftWall.transform.position = new Vector2(-ArenaSize.x, 0);
            _rightWall.transform.position = new Vector2(ArenaSize.x, 0);
            _topWall.transform.position = new Vector2(0, ArenaSize.y);
            _bottomWall.transform.position = new Vector2(0, -ArenaSize.y);

            var middleAreaHeight = _middleAreaHeight * _arenaSize.y / _shieldGridHeight;
            _middleArea.transform.localScale = new Vector2(_arenaSize.x, middleAreaHeight);

            _playAreaBlue = new Rect(-_arenaSize.x / 2, -_arenaSize.y / 2, _arenaSize.x, _arenaSize.y / 2 - middleAreaHeight / 2);
            _playAreaRed = new Rect(-_arenaSize.x / 2, middleAreaHeight / 2, _arenaSize.x, _arenaSize.y / 2 - middleAreaHeight / 2);
            _movementGridHeight = _movementGridMultiplier * _shieldGridHeight;
            _movementGridWidth = _movementGridMultiplier * _shieldGridWidth;
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
