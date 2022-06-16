using System.Collections;
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
    }

    /// <summary>
    /// Tracks ball around gameplay area.
    /// </summary>
    [RequireComponent(typeof(BallManager))]
    internal class BallTracker : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private LayerMask _teamAreaMask;
        [SerializeField] private bool _isSendEvents;
        [SerializeField] private bool _isSetBallState;

        [Header("Live Data"), SerializeField] private bool _isOnBlueTeamArea;
        [SerializeField] private bool _isOnRedTeamArea;
        [SerializeField] private string _lastTeamTag;

        private IBallManager _ballManager;
        private int _teamAreaMaskValue;

        private void Awake()
        {
            _ballManager = Context.BallManager;
            _teamAreaMaskValue = _teamAreaMask.value;
            Assert.IsTrue(_teamAreaMaskValue > 0, "_teamAreaMaskValue > 0");
        }

        private void OnEnable()
        {
            var playArea = Context.GetBattlePlayArea;
            Assert.IsTrue(playArea.BlueTeamCollider.isTrigger, "playArea.BlueTeamCollider.isTrigger");
            Assert.IsTrue(playArea.RedTeamCollider.isTrigger, "playArea.RedTeamCollider.isTrigger");
        }

        #region Collisions

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
            Debug.Log($"enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            _lastTeamTag = otherGameObject.tag;
            if (_isSetBallState)
            {
                if (otherGameObject.CompareTag(Tags.BlueTeam))
                {
                    _isOnBlueTeamArea = true;
                    _ballManager.SetBallState(_isOnRedTeamArea ? BallState.NoTeam : BallState.BlueTeam);
                }
                else if (otherGameObject.CompareTag(Tags.RedTeam))
                {
                    _isOnRedTeamArea = true;
                    _ballManager.SetBallState(_isOnBlueTeamArea ? BallState.NoTeam : BallState.RedTeam);
                }
            }
            if (_isSendEvents)
            {
                this.Publish(new BallMoved(_isOnBlueTeamArea, _isOnRedTeamArea));
            }
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
            Debug.Log($"exit {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            if (_isSetBallState)
            {
                // We have to wait one frame because "overlapping" ball state changes can not happen on same frame.
                StartCoroutine(HandleTriggerExit2D(otherGameObject));
            }
            if (_isSendEvents)
            {
                this.Publish(new BallMoved(_isOnBlueTeamArea, _isOnRedTeamArea));
            }
        }

        private IEnumerator HandleTriggerExit2D(GameObject otherGameObject)
        {
            yield return null;
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _isOnBlueTeamArea = false;
                _ballManager.SetBallState(_isOnRedTeamArea ? BallState.RedTeam : BallState.NoTeam);
            }
            else if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _isOnRedTeamArea = false;
                _ballManager.SetBallState(_isOnBlueTeamArea ? BallState.BlueTeam : BallState.NoTeam);
            }
        }

        #endregion
    }
}