using Battle0.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts.Battle.Ball
{
    /// <summary>
    /// Tracks ball around gameplay area.
    /// </summary>
    [RequireComponent(typeof(BallManager))]
    internal class BallTracker : MonoBehaviour
    {
        private const int TwoTeamValue = 3;

        [Header("Settings"), SerializeField] private LayerMask _teamAreaMask;
        [SerializeField] private LayerMask _headMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private LayerMask _brickMask;
        [SerializeField] private LayerMask _ignoreMask;
        [SerializeField] private bool _isSetBallState;

        [Header("Live Data"), SerializeField] private bool _isOnBlueTeamArea;
        [SerializeField] private bool _isOnRedTeamArea;

        private IGameScoreManager _scoreManager;
        private IBallManager _ballManager;
        private IBallCollision _ballCollision;
        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _wallMaskValue;
        private int _brickMaskValue;
        private int _ignoreMaskValue;

        private void Awake()
        {
            _scoreManager = Context.GetGameScoreManager;
            _ballManager = Context.BallManager;
            _ballCollision = _ballManager.BallCollision;
            _teamAreaMaskValue = _teamAreaMask.value;
            _headMaskValue = _headMask.value;
            _wallMaskValue = _wallMask.value;
            _brickMaskValue = _brickMask.value;
            _ignoreMaskValue = _ignoreMask.value;
        }

        private void OnEnable()
        {
            var playArea = Context.GetBattlePlayArea;
            Assert.IsTrue(playArea.BlueTeamCollider.isTrigger, "playArea.BlueTeamCollider.isTrigger");
            Assert.IsTrue(playArea.RedTeamCollider.isTrigger, "playArea.RedTeamCollider.isTrigger");
        }

        #region Collisions

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            var colliderMask = 1 << layer;
            if (_wallMaskValue == (_wallMaskValue | colliderMask))
            {
                if (!otherGameObject.CompareTag(Tags.Untagged))
                {
                    _scoreManager.OnWallCollision(collision);
                }
                return;
            }
            if (_brickMaskValue == (_brickMaskValue | colliderMask))
            {
                _ballCollision.OnBrickCollision(collision);
                return;
            }
            if (_headMaskValue == (_headMaskValue | colliderMask))
            {
                if (!otherGameObject.CompareTag(Tags.Untagged))
                {
                    _scoreManager.OnHeadCollision(collision);
                    _ballCollision.OnHeadCollision(collision);
                }
                return;
            }
            if (_ignoreMaskValue == (_ignoreMaskValue | colliderMask))
            {
                // We ignore some collisions because they are handled by the others and we do not want to log them here.
                return;
            }
            Debug.Log($"UNHANDLED {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue != (_teamAreaMaskValue | colliderMask))
            {
                return;
            }
            // Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _isOnBlueTeamArea = true;
            }
            else if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _isOnRedTeamArea = true;
            }
            else
            {
                // No state change.
                return;
            }
            if (_isSetBallState)
            {
                _ballManager.SetBallLocalTeamColor(
                    _isOnBlueTeamArea && _isOnRedTeamArea ? TwoTeamValue :
                    _isOnBlueTeamArea ? PhotonBattle.TeamBlueValue :
                    PhotonBattle.TeamRedValue);
            }
            this.Publish(new UiEvents.TeamActivation(_isOnBlueTeamArea, _isOnRedTeamArea));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue != (_teamAreaMaskValue | colliderMask))
            {
                return;
            }
            // Debug.Log($"exit {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _isOnBlueTeamArea = false;
            }
            else if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _isOnRedTeamArea = false;
            }
            else
            {
                // No state change.
                return;
            }
            if (_isSetBallState)
            {
                _ballManager.SetBallLocalTeamColor(
                    _isOnBlueTeamArea ? PhotonBattle.TeamBlueValue :
                    _isOnRedTeamArea ? PhotonBattle.TeamRedValue :
                    PhotonBattle.NoTeamValue);
            }
            this.Publish(new UiEvents.TeamActivation(_isOnBlueTeamArea, _isOnRedTeamArea));
        }

        #endregion
    }
}