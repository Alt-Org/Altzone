using System;
using System.Collections;
using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour, IPlayerDriverState
    {
        [SerializeField] private bool _autoRotate = true;

        private const int shieldEffectDistSquares = 6;
        private const float waitTime = 2f;

        private float _shieldEffectSqr;
        private PlayerPlayArea _battlePlayArea;
        private IPlayerActor _playerActor;
        private GridManager _gridManager;
        private Transform _myActorTransform;
        private bool _isWaitingToMove;
        private float _defaultRotation;
        private int _teamNumber;

        private Transform[] _otherActorTransforms;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(waitTime);
            _battlePlayArea = Context.GetBattlePlayArea;
            var shieldEffectDist = shieldEffectDistSquares * _battlePlayArea.ArenaWidth / _battlePlayArea.GridWidth;
            _shieldEffectSqr = shieldEffectDist * shieldEffectDist + 0.001f;
            var allActors = FindObjectsOfType<PlayerActor>();
            var myActor = (PlayerActor)_playerActor;
            _myActorTransform = myActor.transform;
            _otherActorTransforms = new Transform[allActors.Length - 1];
            var i = 0;
            foreach (var actor in allActors)
            {
                if (actor == myActor)
                {
                    continue;
                }
                _otherActorTransforms[i] = actor.GetComponent<Transform>();
                i++;
            }
        }

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

        private IEnumerator DelayTime(GridPos gridPos, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
            ((IPlayerDriverState)this).IsWaitingToMove(false);
        }

        #region IPlayerDriverState

        bool IPlayerDriverState.CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        void IPlayerDriverState.ResetState(IPlayerActor playerActor, int teamNumber)
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

        void IPlayerDriverState.DelayedMove(GridPos gridPos, float moveExecuteDelay)
        {
            StartCoroutine(DelayTime(gridPos, moveExecuteDelay));
        }

        void IPlayerDriverState.IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }

        #endregion
    }
}
