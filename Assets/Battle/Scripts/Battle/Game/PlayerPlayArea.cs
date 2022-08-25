using Altzone.Scripts.Battle;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class PlayerPlayArea : MonoBehaviour, IBattlePlayArea
    {
        [Header("PlayArea"), SerializeField] private Vector2 _playAreaMiddlePosition;

        [Header("Player Areas"), SerializeField] private Rect _playAreaP1;
        [SerializeField] private Rect _playAreaP2;
        [SerializeField] private Rect _playAreaP3;
        [SerializeField] private Rect _playAreaP4;
        [SerializeField] private Vector2 _deflateLeftBottomArea;
        [SerializeField] private Vector2 _deflateRightTopArea;

        [Header("Player Start Positions"), SerializeField] private Vector2 _startPositionP1;
        [SerializeField] private Vector2 _startPositionP2;
        [SerializeField] private Vector2 _startPositionP3;
        [SerializeField] private Vector2 _startPositionP4;

        [Header("Team Positions"), SerializeField] private Transform _blueTeam;
        [SerializeField] private Transform _redTeam;
        [SerializeField] private Collider2D _teamBlueCollider;
        [SerializeField] private Collider2D _teamRedCollider;

        public Transform BlueTeamTransform => _blueTeam;
        public Transform RedTeamTransform => _redTeam;

        public Collider2D BlueTeamCollider => _teamBlueCollider;
        public Collider2D RedTeamCollider => _teamRedCollider;

        public Vector2 GetPlayAreaMiddlePosition => _playAreaMiddlePosition;
        
        public Rect GetPlayerPlayArea(int playerPos)
        {
            Rect playArea;
            switch (playerPos)
            {
                case PhotonBattle.PlayerPosition1:
                    playArea = _playAreaP1;
                    break;
                case PhotonBattle.PlayerPosition2:
                    playArea = _playAreaP2;
                    break;
                case PhotonBattle.PlayerPosition3:
                    playArea = _playAreaP3;
                    break;
                case PhotonBattle.PlayerPosition4:
                    playArea = _playAreaP4;
                    break;
                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            playArea.x += _playAreaMiddlePosition.x;
            playArea.y += _playAreaMiddlePosition.y;

            if (PhotonBattle.GetTeamNumber(playerPos) == PhotonBattle.TeamBlueValue)
            {
                return InflateBlueSide(playArea, -_deflateLeftBottomArea, -_deflateRightTopArea);
            }
            return InflateRedSide(playArea, -_deflateLeftBottomArea, -_deflateRightTopArea);
        }

        public Vector2 GetPlayerStartPosition(int playerPos)
        {
            Vector2 startPosition;
            switch (playerPos)
            {
                case PhotonBattle.PlayerPosition1:
                    startPosition = _startPositionP1;
                    break;
                case PhotonBattle.PlayerPosition2:
                    startPosition = _startPositionP2;
                    break;
                case PhotonBattle.PlayerPosition3:
                    startPosition = _startPositionP3;
                    break;
                case PhotonBattle.PlayerPosition4:
                    startPosition = _startPositionP4;
                    break;
                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            startPosition += _playAreaMiddlePosition;
            var playArea = GetPlayerPlayArea(playerPos);
            if (!playArea.Contains(startPosition))
            {
                // Note that Rect.Contains tests if a point is "inside this rectangle" - not "on the border line"
                const float smallOffset = 0.001f;
                playArea = playArea.Inflate(smallOffset, smallOffset, smallOffset, smallOffset);
                if (!playArea.Contains(startPosition))
                {
                    Debug.LogWarning($"playerPos {playerPos} startPosition {startPosition} outside playArea {playArea}");
                    startPosition = playArea.center;
                }
            }
            return startPosition;
        }

        private static Rect InflateBlueSide(Rect rect, Vector2 leftBottom, Vector2 rightTop)
        {
            return new Rect
            {
                xMin = rect.xMin - leftBottom.x,
                yMin = rect.yMin - leftBottom.y,
                xMax = rect.xMax + rightTop.x,
                yMax = rect.yMax + rightTop.y
            };
        }

        private static Rect InflateRedSide(Rect rect, Vector2 leftBottom, Vector2 rightTop)
        {
            return new Rect
            {
                xMin = rect.xMin - rightTop.x,
                yMin = rect.yMin - rightTop.y,
                xMax = rect.xMax + leftBottom.x,
                yMax = rect.yMax + leftBottom.y
            };
        }
    }
}