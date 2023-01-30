using System;
using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour, IPlayerDriverState
    {
        private const int shieldEffectDist = 4;
        private const float waitTime = 2f;

        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private Transform _myActorTransform;
        private bool _isWaitingToMove;
        private float _defaultRotation;
        private int _teamNumber;

        private Transform[] _otherActorTransforms;
        private Vector3[] _otherPositions;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(waitTime);
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
            _otherPositions = new Vector3[_otherActorTransforms.Length];
        }

        private void Update()
        {
            var actorRotation = _defaultRotation;
            if (_otherActorTransforms == null)
            {
                return;
            }
            for (int i = 0; i < _otherActorTransforms.Length; i++)
            {
                _otherPositions[i] = _otherActorTransforms[i].position;
            }
            var myActorShieldGridPos = _gridManager.ShieldGridPosition(_myActorTransform.position);
            foreach (var actor in _otherActorTransforms)
            {
                var otherShieldGridPos = _gridManager.ShieldGridPosition(actor.position);
                if (myActorShieldGridPos.Row == otherShieldGridPos.Row)
                {
                    var horizontalGridDist = myActorShieldGridPos.Col - otherShieldGridPos.Col;
                    var turnRight = horizontalGridDist < 0;
                    if (Math.Abs(horizontalGridDist) == shieldEffectDist)
                    {
                        var corner = _gridManager.ShieldSquareCorner(myActorShieldGridPos, turnRight, _teamNumber);
                        var direction = corner - new Vector2(_myActorTransform.position.x, _myActorTransform.position.y);
                        actorRotation = Vector2.SignedAngle(_myActorTransform.up, direction);
                    }
                    if (Math.Abs(horizontalGridDist) < shieldEffectDist)
                    {
                        if (turnRight)
                        {
                            actorRotation = -90f;
                        }
                        else
                        {
                            actorRotation = 90f;
                        }
                    }
                }
            }
            _playerActor.SetRotation(actorRotation);
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
            if (_teamNumber == PhotonBattle.TeamBlueValue)
            {
                _defaultRotation = 0f;
            }
            if (_teamNumber == PhotonBattle.TeamRedValue)
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
