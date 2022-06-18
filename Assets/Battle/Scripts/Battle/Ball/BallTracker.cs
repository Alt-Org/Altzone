using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.PubSub;
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
        [Header("Settings"), SerializeField] private LayerMask _teamAreaMask;
        [SerializeField] private LayerMask _headMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private bool _isSetBallState;

        [Header("Live Data"), SerializeField] private bool _isOnBlueTeamArea;
        [SerializeField] private bool _isOnRedTeamArea;
        [SerializeField] private string _lastTeamTag;

        private IGameScoreManager _scoreManager;
        private IBallManager _ballManager;
        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _wallMaskValue;
        private readonly YieldInstruction _waitForFixedUpdate = new WaitForFixedUpdate();

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
            // We have to wait for fixed update because "overlapping" ball state changes should not happen inside physics engine callback.
            StartCoroutine(HandleCollisionEnter2D(collision));
        }

        private IEnumerator HandleCollisionEnter2D(Collision2D collision)
        {
            yield return _waitForFixedUpdate;
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            var colliderMask = 1 << layer;
            if (_headMaskValue == (_headMaskValue | colliderMask))
            {
                _scoreManager.OnHeadCollision(collision);
                yield break;
            }
            if (_wallMaskValue == (_wallMaskValue | colliderMask))
            {
                _scoreManager.OnWallCollision(collision);
                yield break;
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
                if (otherGameObject.CompareTag(Tags.BlueTeam))
                {
                    _ballManager.SetBallLocalTeamColor(_isOnRedTeamArea ? PhotonBattle.NoTeamValue : PhotonBattle.TeamBlueValue);
                }
                else if (otherGameObject.CompareTag(Tags.RedTeam))
                {
                    _ballManager.SetBallLocalTeamColor(_isOnBlueTeamArea ? PhotonBattle.NoTeamValue : PhotonBattle.TeamRedValue);
                }
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
                // We have to wait for fixed update because "overlapping" ball state changes should not happen inside physics engine callback.
                StartCoroutine(HandleTriggerExit2D(otherGameObject));
            }
            this.Publish(new BallMoved(_isOnBlueTeamArea, _isOnRedTeamArea));
        }

        private IEnumerator HandleTriggerExit2D(GameObject otherGameObject)
        {
            yield return _waitForFixedUpdate;
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _ballManager.SetBallLocalTeamColor(_isOnRedTeamArea ? PhotonBattle.TeamRedValue : PhotonBattle.NoTeamValue);
            }
            else if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _ballManager.SetBallLocalTeamColor(_isOnBlueTeamArea ? PhotonBattle.TeamBlueValue : PhotonBattle.NoTeamValue);
            }
        }

        #endregion
    }
}