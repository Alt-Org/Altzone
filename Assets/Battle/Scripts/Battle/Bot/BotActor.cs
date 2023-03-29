using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using Battle.Scripts.Test;
using UnityEngine;

namespace Battle.Scripts.Battle.Bot
{
    internal class BotActor : PlayerActorBase, IPlayerActor
    {
        [SerializeField] Transform ballTransform;

        private Transform _geometryRoot;
        
        private Vector2 targetPosition;
        private Vector2 taretPosition;

        private IPlayerDriver _playerDriver;
        private IShieldPoseManager _shieldPoseManager;
        private GridManager _gridManager;
        //private IPlayerDriverState _state;

        private float _movementSpeed = 6;
        private float _shieldDeformDelay;
        private float _shieldHitDelay;
        private float _angleLimit;

        private bool _allowShieldHit;
        private bool _hasTarget;

        private int _maxPoseIndex;
        private int _currentPoseIndex;
        private int _shieldResistance;
        private int _shieldHitPoints;
        private int _teamNumber;


        public static string PlayerName = "bot0";

        private void Awake()
        {
            _teamNumber = 1;
            _gridManager = Context.GetGridManager;
            _geometryRoot = transform.GetChild(0).transform;
            _allowShieldHit = true;
            _shieldResistance = 1;
            _shieldHitPoints = _shieldResistance;
            _shieldDeformDelay = 1;
            _shieldHitDelay = 1;
            _angleLimit = 1;
            _shieldPoseManager = GetComponentInChildren<ShieldPoseManager>();
            if (_shieldPoseManager != null)
            {
                StartCoroutine(ResetPose());
            }
        }

        private void FindMovePosition()
        {
            if (_hasTarget == true)
            {
                return;
            }
            var _Y = 0f;
            var _X = 0f;
            if (ballTransform.position.y > transform.position.y)
            {
                _X = Random.Range(-5.0f,5.0f);
                _Y = Random.Range(-1.0f,2.0f);
            }
            else if (ballTransform.position.y < transform.position.y)
            {
                _X = ballTransform.position.x;
                
                _Y = Random.Range(-2.0f,1.0f);
            }
            targetPosition = new Vector2(_X, transform.position.y + _Y);
            if (targetPosition.x < -Screen.width || targetPosition.y < .5 ||
            targetPosition.x > Screen.width || targetPosition.y > Screen.height)
            {
                return;
            }
            StartCoroutine(MoveCoroutine(targetPosition));
            return;
        }

        private void Update()
        {
            FindMovePosition();
        }

        private IEnumerator ShieldDeformDelay(int poseIndex)
        {
            yield return new WaitForSeconds(_shieldDeformDelay);
            _shieldPoseManager.SetPose(poseIndex);
        }

        private IEnumerator ShieldHitDelay(int damage)
        {
            yield return new WaitForSeconds(_shieldHitDelay);
            _shieldHitPoints -= damage;
            if (_shieldHitPoints <= 0)
            {
                _currentPoseIndex++;
                _playerDriver.SetCharacterPose(_currentPoseIndex);
                _shieldHitPoints = _shieldResistance;
            }
            _allowShieldHit = true;
        }

        private IEnumerator MoveCoroutine(Vector2 targetPosition)
        {
            var position = _gridManager.GridPositionToWorldPoint(_gridManager.WorldPointToGridPosition(targetPosition));
            _hasTarget = true;

            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, position, maxDistanceDelta);
                _hasTarget = !(Mathf.Approximately(transform.position.x, position.x) && Mathf.Approximately(transform.position.y, position.y));
            }
            //_state.IsWaitingToMove(true);
        }

        private IEnumerator ResetPose()
        {
            yield return new WaitUntil(() => _shieldPoseManager.MaxPoseIndex > 0);
            _currentPoseIndex = 0;
            _shieldPoseManager.SetPose(_currentPoseIndex);
            _maxPoseIndex = _shieldPoseManager.MaxPoseIndex;
        }

        #region IPlayerActor

        bool IPlayerActor.IsBusy => _hasTarget;

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        void IPlayerActor.SetPlayerDriver(IPlayerDriver playerDriver)
        {
            _playerDriver = playerDriver;
        }

        void IPlayerActor.SetRotation(float angle)
        {
            var multiplier = Mathf.Round (angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
        }

        void IPlayerActor.ShieldHit(int damage)
        {
            if (!_allowShieldHit)
            {
                return;
            }
            _allowShieldHit = false;
            if (_shieldPoseManager == null)
            {
                return;
            }
            if (_currentPoseIndex < _maxPoseIndex)
            {
                StartCoroutine(ShieldHitDelay(damage));
            }
        }

        void IPlayerActor.SetCharacterPose(int poseIndex)
        {
            StartCoroutine(ShieldDeformDelay(poseIndex));
        }

        #endregion


        public static IPlayerActor InstantiatePrefabFor(IPlayerDriver playerDriver, int playerPos, PlayerActorBase playerPrefab, string gameObjectName, float scale)
        {
            PlayerName = gameObjectName;
            Debug.Log($"heoooo{gameObjectName}");            
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            var playerActorBase = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            if (playerActorBase != null)
            {
                playerActorBase.name = playerActorBase.name.Replace("Clone", gameObjectName);
                switch (playerPos)
                {
                    case PhotonBattle.PlayerPosition1:
                        playerActorBase.gameObject.layer = 18;
                        break;
                    case PhotonBattle.PlayerPosition2:
                        playerActorBase.gameObject.layer = 19;
                        break;
                    case PhotonBattle.PlayerPosition3:
                        playerActorBase.gameObject.layer = 20;
                        break;
                    case PhotonBattle.PlayerPosition4:
                        playerActorBase.gameObject.layer = 21;
                        break;
                    default:
                        throw new UnityException($"Invalid player position {playerPos}");
                }
            }            
            playerActorBase.transform.localScale = Vector3.one * scale;
            var playerActor = (IPlayerActor)playerActorBase;
            playerActor.SetPlayerDriver(playerDriver);
            return playerActor;
        }
    }
}