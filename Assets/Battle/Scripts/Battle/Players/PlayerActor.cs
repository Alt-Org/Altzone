using System;
using System.Collections;
using System.Diagnostics;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Prg.Scripts.Test;
using TMPro;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// This class manages local visual representation of player actor.
    /// </remarks>
    internal class PlayerActor : PlayerActorBase, IPlayerActor
    {
        [Serializable]
        internal class PlayerSettings
        {
            public Transform _geometryRoot;
            public Avatar _avatar;
            public Shield _shield;
        }

        [Serializable]
        internal class Avatar
        {
            public GameObject _avatar1;
            public GameObject _avatar2;
            public GameObject _avatar3;
            public GameObject _avatar4;
            public GameObject _avatar5;
            public GameObject _avatar6;

            // Last pose is reserved for disconnected pose
            public GameObject[] Avatars => new[] { _avatar1, _avatar2, _avatar3, _avatar4, _avatar5, _avatar6 };
        }

        [Serializable]
        internal class Shield
        {
            public GameObject _shield1;
            public GameObject _shield2;
            public GameObject _shield3;
            public GameObject _shield4;
            public GameObject _shield5;

            public GameObject[] Shields => new[] { _shield1, _shield2, _shield3, _shield4, _shield5 };
        }

        [Serializable]
        internal class DebugSettings
        {
            public bool _isLogEvents;
            public bool _isLogMoveTo;
            public bool _isShowPlayerText;
            public TextMeshPro _playerText;
            public char _playerModeOrBuff = '?';
        }

        [Serializable]
        internal class ColorSettings
        {
            public Color _colorForDes;
            public Color _colorForDef;
            public Color _colorForInt;
            public Color _colorForPro;
            public Color _colorForRet;
            public Color _colorForEgo;
            public Color _colorForCon;
        }

        [Header("Player Settings"), SerializeField] private PlayerSettings _settings;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        [Header("Color Settings"), SerializeField] private ColorSettings _colors;

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
        private ThrottledDebugLogger _moveLogger;

        private Color[] _skillColors;
        private IPoseManager _avatarPose;
        private IPoseManager _shieldPose;
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
            _skillColors = new[]
            {
                Color.black, _colors._colorForDes, _colors._colorForDef, _colors._colorForInt,
                _colors._colorForPro, _colors._colorForRet, _colors._colorForEgo, _colors._colorForCon
            };
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
            Debug.Log($"{name} {model.MainDefence}");
            var skillColor = _skillColors[(int)model.MainDefence];
            var avatarColorCount = _settings._avatar.Avatars.Length - 1;
            _avatarPose = new TestPoseManager(_settings._avatar.Avatars, skillColor, avatarColorCount);
            // Last pose is reserved for disconnected pose
            _maxPoseIndex = _settings._avatar.Avatars.Length - 2;
            _disconnectedPoseIndex = _maxPoseIndex + 1;
            var avatarHeadTag = _playerDriver.TeamNumber == PhotonBattle.TeamBlueValue ? Tags.BlueTeam : Tags.RedTeam;
            _avatarPose.Reset(0, BattlePlayMode.Normal, true, avatarHeadTag);
            _shieldPose = new TestPoseManager(_settings._shield.Shields, skillColor);
            _shieldPose.Reset(0, BattlePlayMode.Normal, true, null);
            UpdatePlayerText();
            // We have to add ColliderTracker for every collider there is
            // - if we add it to upper level in hierarchy (which would be nice) it does not receive collision events :-(
            var collisionDriver = _playerDriver.PlayerActorCollision;
            foreach (var avatar in _settings._avatar.Avatars)
            {
                var tracker = avatar.AddComponent<ColliderTracker>();
                tracker.Callback = collision => { collisionDriver.OnHeadCollision(collision); };
            }
            foreach (var shield in _settings._shield.Shields)
            {
                var tracker = shield.AddComponent<ColliderTracker>();
                tracker.Callback = collision => { collisionDriver.OnShieldCollision(collision); };
            }
            if (_debug._isLogMoveTo)
            {
                if (Application.isEditor)
                {
                    _moveLogger = new ThrottledDebugLogger(this);
                }
                else
                {
                    _debug._isLogMoveTo = false;
                }
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
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

        #endregion

        #region IPlayerActor Interface

        GameObject IPlayerActor.GameObject => gameObject;

        Transform IPlayerActor.Transform => _transform;

        BattlePlayMode IPlayerActor.BattlePlayMode => _playMode;

        int IPlayerActor.MaxPoseIndex => _maxPoseIndex;

        float IPlayerActor.Speed => _speed;

        int IPlayerActor.ShieldResistance => _resistance;

        void IPlayerActor.Setup(float speed, int resistance)
        {
            _speed = speed;
            _resistance = resistance;
            UpdatePlayerText();
        }
        
        bool IPlayerActor.IsBusy  => _hasTarget;

        void IPlayerActor.Rotate(bool isUpsideDown)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} isUpsideDown {isUpsideDown}");
            }
            _settings._geometryRoot.Rotate(isUpsideDown);
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
            if (_playerDriver.IsLocal && _debug._isLogMoveTo)
            {
                ThrottledDebugLogger.Log(_moveLogger, $"{name} MoveTo {(Vector2)_targetPosition} <- {targetPosition} Speed {_speed} canDo {canDo}");
            }
            if (!canDo)
            {
                return;
            }
            _hasTarget = true;
            _targetPosition = targetPosition;
        }

        void IPlayerActor.SetShieldResistance(int resistance)
        {
            _resistance = resistance;
            UpdatePlayerText();
        }

        void IPlayerActor.SetCharacterPose(int poseIndex)
        {
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} {_poseIndex} <- {poseIndex}");
            }
            _poseIndex = poseIndex;
            StartCoroutine(SetPoseOnNextFrame());
            UpdatePlayerText();
        }

        private IEnumerator SetPoseOnNextFrame()
        {
            // We have a problem with colliders when character pose is changed during a collision!
            yield return null;
            _shieldPose.SetPose(_poseIndex);
            _avatarPose.SetPose(_poseIndex);
        }

        void IPlayerActor.SetPlayMode(BattlePlayMode playMode)
        {
            Assert.IsTrue(playMode < BattlePlayMode.Disconnected, "playMode < BattlePlayMode.Disconnected");
            var canTransition = _playMode.CanTransition(playMode);
            if (_debug._isLogEvents)
            {
                Debug.Log($"{name} {_playMode} <- {playMode} {(canTransition ? "OK" : "NO CAN TRANSITION")}");
            }
            if (!canTransition)
            {
                if (_playMode == BattlePlayMode.RaidGhosted)
                {
                    Debug.Log($"{name} {playMode} SAVE PENDING");
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
            Debug.Log($"{name} expired");
        }

        void IPlayerActor.ResetPlayerDriver()
        {
            // We have lost our original driver
            Debug.Log($"{name}");
            _playerDriver = null;
            _hasPlayer = false;
            UpdatePlayerText();
            _avatarPose.SetPose(_disconnectedPoseIndex);
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

    /// <summary>
    /// Helper class to manage <c>GameObject</c> hierarchy for <c>PlayerActor</c> (shields and avatars).
    /// </summary>
    internal class TestPoseManager : IPoseManager
    {
        private readonly PoseState _state;
        private readonly GameObject[] _avatars;
        private readonly Collider2D[] _colliders;

        public bool IsVisible => _state.IsVisible;
        public int MaxPoseIndex => throw new NotSupportedException();

        private GameObject _currentAvatar;
        private Collider2D _currentCollider;

        private Transform _parentTransform;
        private int _childCount;

        public TestPoseManager(GameObject[] avatars, Color avatarColor, int avatarColorCount = 0)
        {
            _state = new PoseState();
            _avatars = avatars;
            _colliders = new Collider2D[avatars.Length];
            var index = -1;
            foreach (var avatar in _avatars)
            {
                if (--avatarColorCount >= 0)
                {
                    var spriteRenderer = avatar.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = avatarColor;
                }
                _colliders[++index] = avatar.GetComponentsInChildren<Collider2D>(true)[0];
            }
        }

        public void SetVisible(bool isVisible)
        {
            _state.IsVisible = isVisible;
            _currentAvatar.SetActive(isVisible);
            _currentCollider.enabled = _state.PlayMode.CanCollide();
        }

        public void SetPlayMode(BattlePlayMode playMode)
        {
            _state.PlayMode = playMode;
            SetVisible(IsVisible);
        }

        public void SetPose(int poseIndex)
        {
            Assert.IsTrue(poseIndex >= 0 && poseIndex < _childCount);
            _currentAvatar.SetActive(false);
            _currentAvatar = _avatars[poseIndex];
            _currentCollider = _colliders[poseIndex];
            SetVisible(IsVisible);
        }

        public void Reset(int poseIndex, BattlePlayMode playMode, bool isVisible, string tag)
        {
            _currentAvatar = _avatars[0];
            _currentCollider = _colliders[0];

            _childCount = _avatars.Length;
            var firstPosition = _currentAvatar.transform.position;
            var isSetTag = !string.IsNullOrWhiteSpace(tag);
            for (var i = 0; i < _childCount; ++i)
            {
                var child = _avatars[i];
                if (isSetTag)
                {
                    child.tag = tag;
                }
                if (i > 0)
                {
                    child.transform.position = firstPosition;
                    child.gameObject.SetActive(false);
                }
            }

            _state.PlayMode = playMode;
            _state.IsVisible = isVisible;
            SetPose(poseIndex);
        }
    }

    internal class ColliderTracker : MonoBehaviour
    {
        public Action<Collision2D> Callback;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (collision.contactCount == 0)
            {
                return;
            }
            Callback(collision);
        }
    }
}
