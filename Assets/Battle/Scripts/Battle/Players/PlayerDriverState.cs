using System.Collections;
using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour
    {
        [SerializeField] private bool _autoRotate = true;

        private const int shieldEffectDistSquares = 6;
        private const float PhotonWaitTime = 2f;

        private float _shieldEffectSqr;
        private PlayerPlayArea _battlePlayArea;
        private PlayerActor _playerActor;
        private GridManager _gridManager;
        private Transform _myActorTransform;
        private bool _isWaitingToMove;
        private float _defaultRotation;
        private int _teamNumber;

        private Transform[] _otherActorTransforms;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(PhotonWaitTime);
            _battlePlayArea = Context.GetBattlePlayArea;
            var shieldEffectDist = shieldEffectDistSquares * _battlePlayArea.ArenaWidth / _battlePlayArea.GridWidth;
            _shieldEffectSqr = shieldEffectDist * shieldEffectDist + 0.001f;
            // After UNITY 2021.3.18 you can use faster FindObjectsByType API call:
            //var allActors = FindObjectsByType<PlayerActor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var allActors = FindObjectsOfType<PlayerActor>();
            _myActorTransform = _playerActor.transform;
            _otherActorTransforms = new Transform[allActors.Length - 1];
            var i = 0;
            foreach (var actor in allActors)
            {
                if (actor == _playerActor)
                {
                    continue;
                }
                _otherActorTransforms[i] = actor.GetComponent<Transform>();
                i++;
            }
        }

        /*
        private void Update()
        {
            var actorRotation = _defaultRotation;
            if (_otherActorTransforms == null)
            {
                return;
            }
            foreach (var actor in _otherActorTransforms)
            {
                var distVector = actor.position - _myActorTransform.position;
                if (distVector.sqrMagnitude < _shieldEffectSqr)
                {
                    actorRotation = Vector2.SignedAngle(_myActorTransform.up, new Vector2(distVector.x, distVector.y));
                }
            }
            if (_autoRotate)
            {
                _playerActor.SetRotation(actorRotation);
            }
        }
        */

        /*
        old
        private IEnumerator DelayTime(GridPos gridPos, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
            IsWaitingToMove(false);
        }
        */

        internal bool CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        internal void ResetState(PlayerActor playerActor, int teamNumber)
        {
            _playerActor = playerActor;
            _teamNumber = teamNumber;
            if (_teamNumber == PhotonBattle.TeamAlphaValue)
            {
                _defaultRotation = 0f;
            }
            if (_teamNumber == PhotonBattle.TeamBetaValue)
            {
                _defaultRotation = 180f;
            }
            _gridManager = Context.GetGridManager;
        }

        /* 
        old
        internal void DelayedMove(GridPos gridPos, float moveExecuteDelay)
        {
            StartCoroutine(DelayTime(gridPos, moveExecuteDelay));
        }
        */

        internal void Move(GridPos gridPos, int teleportUpdateNumber)
        {
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition, teleportUpdateNumber);
            IsWaitingToMove(false);
        }

        internal void IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }
    }
}
