using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// Needs to derive from <c>PlayerActorBase</c> for type safe UNITY prefab instantiation.
    /// </remarks>
    internal class PlayerActor : PlayerActorBase
    {
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;

        private IPlayerDriver _playerDriver;
        private IShieldPoseManager _shieldPoseManager;
        private float _playerMoveSpeedMultiplier;
        private Transform _transform;
        private Vector3 _tempPosition;
        private bool _hasTarget;
        private int _shieldResistance;
        private int _shieldHitPoints;
        private float _shieldDeformDelay;
        private float _shieldHitDelay;
        private float _angleLimit;
        private int _maxPoseIndex;
        private int _currentPoseIndex;
        private bool _allowShieldHit;

        public static string PlayerName;

        private void Awake()
        {
            _allowShieldHit = true;
            _transform = GetComponent<Transform>();
            var variables = GameConfig.Get().Variables;
            _playerMoveSpeedMultiplier = variables._playerMoveSpeedMultiplier;
            _shieldResistance = variables._shieldResistance;
            _shieldHitPoints = _shieldResistance;
            _shieldDeformDelay = variables._shieldDeformDelay;
            _shieldHitDelay = variables._shieldHitDelay;
            _angleLimit = variables._angleLimit;
            _shieldPoseManager = GetComponentInChildren<ShieldPoseManager>();
            if (_shieldPoseManager != null)
            {
                StartCoroutine(ResetPose());
            }
        }

        private IEnumerator MoveCoroutine(Vector2 position)
        {
            Vector3 targetPosition = position;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                var maxDistanceDelta = _movementSpeed * _playerMoveSpeedMultiplier * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_transform.position, targetPosition, maxDistanceDelta);
                _transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
            }
        }

        private IEnumerator ResetPose()
        {
            yield return new WaitUntil(() => _shieldPoseManager.MaxPoseIndex > 0);
            _currentPoseIndex = 0;
            _shieldPoseManager.SetPose(_currentPoseIndex);
            _maxPoseIndex = _shieldPoseManager.MaxPoseIndex;
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

        public bool IsBusy => _hasTarget;

        public void MoveTo(Vector2 targetPosition)
        {
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            _playerDriver = playerDriver;
        }

        public void SetRotation(float angle)
        {
            var multiplier = Mathf.Round (angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
        }

        public void ShieldHit(int damage)
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

        public void SetCharacterPose(int poseIndex)
        {
            StartCoroutine(ShieldDeformDelay(poseIndex));
        }

        public static PlayerActor InstantiatePrefabFor(IPlayerDriver playerDriver, int playerPos, PlayerActor playerPrefab, string gameObjectName, float scale)
        {
            PlayerName = gameObjectName;
            Debug.Log($"heoooo{gameObjectName}");            
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            var instance = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            Assert.IsNotNull(instance, $"bad prefab: {playerPrefab.name}");
            instance.name = instance.name.Replace("Clone", gameObjectName);
            switch (playerPos)
            {
                case PhotonBattle.PlayerPosition1:
                    instance.gameObject.layer = Layers.Player1;
                    break;
                case PhotonBattle.PlayerPosition2:
                    instance.gameObject.layer = Layers.Player2;
                    break;
                case PhotonBattle.PlayerPosition3:
                    instance.gameObject.layer = Layers.Player3;
                    break;
                case PhotonBattle.PlayerPosition4:
                    instance.gameObject.layer = Layers.Player4;
                    break;
                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            instance.transform.localScale = Vector3.one * scale;
            instance.SetPlayerDriver(playerDriver);
            return instance;
        }
    }
}
