using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Sets the arena, player's starting positions and team play areas.
    /// </summary>
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

        [Tooltip("Set top and bottom wall colliders as triggers"), SerializeField] private bool _isBackWallsTriggers;

        [SerializeField] private GameObject _alphaTeamBrickWall;
        [SerializeField] private GameObject _betaTeamBrickWall;

        [Header("Player Start Positions"), SerializeField] private GridPos _startPositionAlpha1;
        [SerializeField] private GridPos _startPositionAlpha2;
        [SerializeField] private GridPos _startPositionBeta1;
        [SerializeField] private GridPos _startPositionBeta2;

        private GameObject[] _alphaTeamBricks;
        private GameObject[] _betaTeamBricks;

        private BoxCollider2D _rightWallCollider;
        private BoxCollider2D _leftWallCollider;
        private BoxCollider2D _bottomWallCollider;
        private BoxCollider2D _topWallCollider;

        private int _movementGridHeight;
        private int _movementGridWidth;
        private const float BrickSpriteWidth = 2.35f;
        private const int BricksPerWall = 5;

        private Rect _playStartAreaAlpha;
        private Rect _playStartAreaBeta;

        private void Awake()
        {
            SetupArenaBorders();
            SetupBrickWalls();

            var middleAreaHeight = _middleAreaHeight * _arenaHeigth / _shieldGridHeight;
            var shieldSquareHeight = _arenaHeigth / _shieldGridHeight;
            _playStartAreaAlpha = new Rect(-_arenaWidth / 2, -_arenaHeigth / 2 + shieldSquareHeight, _arenaWidth, _arenaHeigth / 2 - middleAreaHeight / 2 - shieldSquareHeight);
            _playStartAreaBeta = new Rect(-_arenaWidth / 2, middleAreaHeight / 2, _arenaWidth, _arenaHeigth / 2 - middleAreaHeight / 2 - shieldSquareHeight);
            _movementGridHeight = _movementGridMultiplier * _shieldGridHeight;
            _movementGridWidth = _movementGridMultiplier * _shieldGridWidth;
        }

        private void SetupArenaBorders()
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
        }

        private void SetupBrickWalls()
        {
            _alphaTeamBricks = new GameObject[BricksPerWall];
            _betaTeamBricks = new GameObject[BricksPerWall];
            for (int i = 0; i < BricksPerWall; i++)
            {
                _alphaTeamBricks[i] = _alphaTeamBrickWall.transform.GetChild(i).gameObject;
                _betaTeamBricks[i] = _betaTeamBrickWall.transform.GetChild(i).gameObject;
                _alphaTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _arenaHeigth / _shieldGridHeight);
                _betaTeamBricks[i].GetComponent<SpriteRenderer>().size = new Vector2(BrickSpriteWidth, _arenaHeigth / _shieldGridHeight);
                _alphaTeamBricks[i].transform.position = new Vector2(_alphaTeamBricks[i].transform.position.x, -_arenaHeigth / 2 + _arenaHeigth / (2 * _shieldGridHeight));
                _betaTeamBricks[i].transform.position = new Vector2(_betaTeamBricks[i].transform.position.x, _arenaHeigth / 2 - _arenaHeigth / (2 * _shieldGridHeight));
                _alphaTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _arenaHeigth / _shieldGridHeight);
                _betaTeamBricks[i].GetComponent<BoxCollider2D>().size = new Vector2(_arenaWidth / BricksPerWall, _arenaHeigth / _shieldGridHeight);
            }
        }

        #region IBattlePlayArea

        float IBattlePlayArea.ArenaWidth => _arenaWidth;
        float IBattlePlayArea.ArenaHeight => _arenaHeigth;
        int IBattlePlayArea.ShieldGridWidth => _shieldGridWidth;
        int IBattlePlayArea.ShieldGridHeight => _shieldGridHeight;
        int IBattlePlayArea.MovementGridWidth => _movementGridWidth;
        int IBattlePlayArea.MovementGridHeight => _movementGridHeight;

        Rect IBattlePlayArea.GetPlayerPlayArea(int teamNumber)
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

        GridPos IBattlePlayArea.GetPlayerStartPosition(int playerPos)
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
        #endregion
    }
}
