using System;
using System.Collections;
using System.Diagnostics;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using TMPro;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote player (avatar+shield) instances.
    /// </summary>
    /// <remarks>
    /// This class manages local visual representation of the player actor.
    /// </remarks>
    internal class PlayerActor2 : PlayerActorBase, IPlayerActor
    {
        [Serializable]
        internal class DebugSettings
        {
            public bool _isLogEvents;
            public bool _isLogMoveTo;
            public bool _isShowPlayerText;
            public TextMeshPro _playerText;
            public char _playerModeOrBuff = '?';
        }

        [Header("Player Settings"), SerializeField] private PlayerSettings2 _settings;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private IPlayerDriver _playerDriver;
        private bool _hasPlayer;
        private Transform _transform;
        private int _actorNumber;
        private int _playerPos;

        private bool _hasTarget;
        private Vector3 _targetPosition;
        private Vector3 _tempPosition;

        private BattlePlayMode _playMode;
        private bool _isPendingPlayMode;
        private BattlePlayMode _pendingPlayMode;
        private int _poseIndex;
        private bool _isStunned;
        private Coroutine _stunnedCoroutine;

        private IPoseManager _avatarPose;
        private IPoseManager _shieldPose;
        private int _lastShieldCollisionFrame;
        private int _maxPoseIndex;
        private int _disconnectedPoseIndex;

        private float _speed;
        private float _playerMoveSpeedMultiplier;
        private int _resistance;

        private bool CanMove => _hasTarget && !_isStunned && _playMode.CanMove();

        private bool CanAcceptMove => !_isStunned && _playMode.CanMove();

        private bool IsBuffedOrDeBuffed => _isStunned;

        private void Awake()
        {
            Debug.Log($"{name}");
            if (_debug._playerText == null)
            {
                _debug._isShowPlayerText = false;
            }
            else if (!_debug._isShowPlayerText)
            {
                _debug._playerText.gameObject.SetActive(false);
            }
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _playerMoveSpeedMultiplier = variables._playerMoveSpeedMultiplier;
            // Wait until PlayerDriver is assigned.
            enabled = false;
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
            try
            {
                if (_playerDriver != null && _playerDriver.IsValid)
                {
                    _playerDriver.PlayerActorDestroyed();
                }
            }
            catch (MissingReferenceException)
            {
                // Scene is unloading and destroying all related object-graph without errors was too hard to accomplish for now :-(
                Debug.LogWarning($"<color=white>MissingReferenceException</color> {name}");
            }
        }

        protected override void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            // Now we are good to go.
            _playerDriver = playerDriver;
            _hasPlayer = true;
            _actorNumber = playerDriver.ActorNumber;
            _playerPos = playerDriver.PlayerPos;
            Debug.Log($"{name} pos {_playerPos} actor {_actorNumber}");
            _transform = GetComponent<Transform>();
            if (_debug._isShowPlayerText && !playerDriver.IsLocal)
            {
                // Pinkish text color.
                _debug._playerText.faceColor = new Color32(255, 204, 255, 255);
            }
            enabled = true;
        }

        private void OnEnable()
        {
            var model = _playerDriver.CharacterModel;
            Debug.Log($"{name} {model.Name} {model.MainDefence}");
            _avatarPose = _settings.GetAvatarPoseManager;
            var avatarHeadTag = _playerDriver.TeamNumber == PhotonBattle.TeamBlueValue ? Tags.BlueTeam : Tags.RedTeam;
            _avatarPose.Reset(0, BattlePlayMode.Normal, true, avatarHeadTag);
            _shieldPose = _settings.GetShieldPoseManager;
            _shieldPose.Reset(0, BattlePlayMode.Normal, true, null);
            var collisionDriver = _playerDriver.PlayerActorCollision;
            _settings.SetAvatarsColliderCallback(collision => { collisionDriver.OnHeadCollision(collision); });
            _settings.SetShieldsColliderCallback(OnShieldCollisionFilter);
            UpdatePlayerText();
            _lastShieldCollisionFrame = -1;
            // Avatar should have one "extra" pose for disconnected state.
            _maxPoseIndex = _avatarPose.MaxPoseIndex;
            if (_maxPoseIndex > _shieldPose.MaxPoseIndex)
            {
                _disconnectedPoseIndex = _maxPoseIndex;
                _maxPoseIndex -= 1;
            }
            else
            {
                _disconnectedPoseIndex = -1;
            }

            void OnShieldCollisionFilter(Collision2D collision)
            {
                if (Time.frameCount == _lastShieldCollisionFrame)
                {
                    return;
                }
                _lastShieldCollisionFrame = Time.frameCount;
                collisionDriver.OnShieldCollision(collision);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _settings.SetAvatarsColliderCallback(null);
            _settings.SetShieldsColliderCallback(null);
        }

        private void Update()
        {
            if (!CanMove)
            {
                return;
            }
            var maxDistanceDelta = _speed * _playerMoveSpeedMultiplier * Time.deltaTime;
            _tempPosition = Vector3.MoveTowards(_transform.position, _targetPosition, maxDistanceDelta);
            _transform.position = _tempPosition;
            _hasTarget = !(Mathf.Approximately(_tempPosition.x, _targetPosition.x) && Mathf.Approximately(_tempPosition.y, _targetPosition.y));
        }

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdatePlayerText()
        {
            if (!_debug._isShowPlayerText)
            {
                return;
            }
            if (!_hasPlayer)
            {
                _debug._playerText.text = $"{PlayerPosChars[_playerPos]}--{PlayModes[^1]}";
                return;
            }
            if (!IsBuffedOrDeBuffed)
            {
                _debug._playerModeOrBuff = PlayModes[(int)_playMode];
            }
            _debug._playerText.text = $"{PlayerPosChars[_playerPos]}{_maxPoseIndex - _poseIndex}{_resistance}{_debug._playerModeOrBuff}";
        }

        [Conditional("UNITY_EDITOR")]
        private void SetThrottledDebugLogMessage(string debugLogMessage)
        {
            if (!_playerDriver.IsLocal)
            {
                return;
            }
            _debugLogMessage = debugLogMessage;
        }

        private string _debugLogMessage;

        /// <summary>
        /// Throttles debug log messages printing them only on every N seconds after first message is detected.
        /// </summary>
        private IEnumerator ThrottledDebugLogger()
        {
            const float samplingInterval = 1.0f;
            Assert.IsTrue(Application.isEditor);
            var nextDebugLogTime = 0f;
            _debugLogMessage = null;
            for (; enabled;)
            {
                if (_debugLogMessage != null && Time.time > nextDebugLogTime)
                {
                    Debug.Log(_debugLogMessage);
                    _debugLogMessage = null;
                    nextDebugLogTime = Time.time + samplingInterval;
                }
                yield return null;
            }
        }

        #endregion

        #region IPlayerActor Interface

        GameObject IPlayerActor.GameObject => gameObject;

        Transform IPlayerActor.Transform => _transform;

        BattlePlayMode IPlayerActor.BattlePlayMode => _playMode;

        int IPlayerActor.MaxPoseIndex => _maxPoseIndex;

        float IPlayerActor.Speed
        {
            get => _speed;
            set => _speed = value;
        }

        int IPlayerActor.CurrentResistance
        {
            get => _resistance;
            set
            {
                _resistance = value;
                UpdatePlayerText();
            }
        }

        void IPlayerActor.Rotate(bool isUpsideDown)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} isUpsideDown {isUpsideDown}");
            }
            _settings.Rotate(isUpsideDown);
            UpdatePlayerText();
        }

        void IPlayerActor.FixCameraRotation(Camera gameCamera)
        {
            // Check that camera and everything that needs to have same orientation is oriented in the same way.
            // - That is if camera is upside down, we must change e.g. texts to be upside down so they have correct orientation.
            if (_debug._isShowPlayerText)
            {
                _debug._playerText.GetComponent<Transform>().rotation = gameCamera.GetComponent<Transform>().rotation;
            }
        }

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            var canDo = CanAcceptMove;
            if (_debug._isLogMoveTo)
            {
                SetThrottledDebugLogMessage($"{name} MoveTo {(Vector2)_targetPosition} <- {targetPosition} Speed {_speed} canDo {canDo}");
            }
            if (!canDo)
            {
                return;
            }
            _hasTarget = true;
            _targetPosition = targetPosition;
        }

        void IPlayerActor.SetCharacterPose(int poseIndex)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} max {_maxPoseIndex} : {_poseIndex} <- {poseIndex}");
            }
            _poseIndex = poseIndex;
            StartCoroutine(SetPoseOnNextFrame(_poseIndex));
            UpdatePlayerText();
        }

        private IEnumerator SetPoseOnNextFrame(int poseIndex)
        {
            // We have a problem with colliders when character pose is changed during a collision!
            yield return null;
            _shieldPose.SetPose(poseIndex);
            _avatarPose.SetPose(poseIndex);
        }

        void IPlayerActor.SetPlayMode(BattlePlayMode playMode)
        {
            Assert.IsTrue(playMode < BattlePlayMode.Disconnected, "playMode < BattlePlayMode.Disconnected");
            var canTransition = _playMode.CanTransition(playMode);
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} {_playMode} <- {playMode}{(canTransition ? string.Empty : " CAN NOT TRANSITION")}");
            }
            if (!canTransition)
            {
                if (_playMode == BattlePlayMode.RaidGhosted)
                {
                    if (_debug._isLogEvents)
                    {
                        Debug.Log($"{name} {playMode} SAVE PENDING");
                    }
                    _isPendingPlayMode = true;
                    _pendingPlayMode = playMode;
                }
                else
                {
                    _isPendingPlayMode = false;
                }
                return;
            }
            if (playMode == BattlePlayMode.RaidReturn && _isPendingPlayMode)
            {
                _isPendingPlayMode = false;
                playMode = _pendingPlayMode;
                if (_debug._isLogEvents)
                {
                    Debug.Log($"{name} {_playMode} <- {playMode} FROM PENDING");
                }
            }
            _playMode = playMode;
            _shieldPose.SetPlayMode(playMode);
            _avatarPose.SetPlayMode(playMode);
            UpdatePlayerText();
        }

        void IPlayerActor.SetShieldVisibility(bool state)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} {_shieldPose.IsVisible} <- {state}");
            }
            _shieldPose.SetVisible(state);
            UpdatePlayerText();
        }

        void IPlayerActor.SetBuff(PlayerBuff buff, float duration)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} start {buff} {duration:0.0}s");
            }
            switch (buff)
            {
                case PlayerBuff.Stunned:
                    StartStunnedBuff(duration);
                    break;
                default:
                    throw new UnityException($"Unknown buff {buff}");
            }
        }

        private void StartStunnedBuff(float duration)
        {
            _isStunned = true;
            _debug._playerModeOrBuff = 'X';
            if (_stunnedCoroutine != null)
            {
                StopCoroutine(_stunnedCoroutine);
            }
            _stunnedCoroutine = StartCoroutine(StunnedBuff(duration));
            UpdatePlayerText();
        }

        private IEnumerator StunnedBuff(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isStunned = false;
            _stunnedCoroutine = null;
            UpdatePlayerText();
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} expired");
            }
        }

        void IPlayerActor.ResetPlayerDriver()
        {
            // We have lost our original driver
            Debug.Log($"{name}");
            _playerDriver = null;
            _hasPlayer = false;
            UpdatePlayerText();
            if (_disconnectedPoseIndex != -1)
            {
                _avatarPose.SetPose(_disconnectedPoseIndex);
            }
            else
            {
                _avatarPose.SetVisible(false);
            }
            _shieldPose.SetVisible(false);

            // Brute force way to disable everything (that is still active)!
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var childCollider in colliders)
            {
                childCollider.enabled = false;
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{name}";
        }
    }
}