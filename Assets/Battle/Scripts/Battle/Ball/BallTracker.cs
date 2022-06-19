using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Ball
{
    internal class BallMoved
    {
        public readonly bool IsOnBlueTeamArea;
        public readonly bool IsOnRedTeamArea;

        public BallMoved(bool isOnBlueTeamArea, bool isOnRedTeamArea)
        {
            IsOnBlueTeamArea = isOnBlueTeamArea;
            IsOnRedTeamArea = isOnRedTeamArea;
        }

        public override string ToString()
        {
            return $"IsOnBlueTeamArea: {IsOnBlueTeamArea}, IsOnRedTeamArea: {IsOnRedTeamArea}";
        }
    }

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
        [SerializeField] private bool _isSetBallState;

        [Header("Live Data"), SerializeField] private bool _isOnBlueTeamArea;
        [SerializeField] private bool _isOnRedTeamArea;
        [SerializeField] private string _lastTeamTag;

        [Header("Debug Settings"), SerializeField] private bool _useScoreFlashNet;
        
        private IGameScoreManager _scoreManager;
        private IBallManager _ballManager;
        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _wallMaskValue;

        private void Awake()
        {
            _scoreManager = Context.GetGameScoreManager;
            _ballManager = Context.BallManager;
            _teamAreaMaskValue = _teamAreaMask.value;
            _headMaskValue = _headMask.value;
            _wallMaskValue = _wallMask.value;
        }

        private void OnEnable()
        {
            var playArea = Context.GetBattlePlayArea;
            Assert.IsTrue(playArea.BlueTeamCollider.isTrigger, "playArea.BlueTeamCollider.isTrigger");
            Assert.IsTrue(playArea.RedTeamCollider.isTrigger, "playArea.RedTeamCollider.isTrigger");
            if (_useScoreFlashNet)
            {
                ScoreFlashNet.RegisterEventListener();
            }
        }

        #region Collisions

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Untagged))
            {
                return;
            }
            var layer = otherGameObject.layer;
            var colliderMask = 1 << layer;
            if (_headMaskValue == (_headMaskValue | colliderMask))
            {
                _scoreManager.OnHeadCollision(collision);
                return;
            }
            if (_wallMaskValue == (_wallMaskValue | colliderMask))
            {
                Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
                _scoreManager.OnWallCollision(collision);
                if (_useScoreFlashNet && PhotonNetwork.IsMasterClient)
                {
                    var contactPoint = collision.GetFirstContactPoint();
                    ScoreFlashNet.Push("WALL", contactPoint.point);
                }
                return;
            }
            Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
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
            _lastTeamTag = otherGameObject.tag;
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _isOnBlueTeamArea = true;
            }
            else if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _isOnRedTeamArea = true;
            }
            if (_isSetBallState)
            {
                _ballManager.SetBallLocalTeamColor(
                    _isOnBlueTeamArea && _isOnRedTeamArea ? TwoTeamValue :
                    _isOnBlueTeamArea ? PhotonBattle.TeamBlueValue :
                    PhotonBattle.TeamRedValue);
            }
            this.Publish(new BallMoved(_isOnBlueTeamArea, _isOnRedTeamArea));
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
            if (_isSetBallState)
            {
                _ballManager.SetBallLocalTeamColor(
                    _isOnBlueTeamArea ? PhotonBattle.TeamBlueValue :
                    _isOnRedTeamArea ? PhotonBattle.TeamRedValue :
                    PhotonBattle.NoTeamValue);
            }
            this.Publish(new BallMoved(_isOnBlueTeamArea, _isOnRedTeamArea));
        }

        #endregion
    }
}