using System;
using System.Collections;
using System.Diagnostics;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// This class manages local visual representation of player actor.
    /// </remarks>
    internal class PlayerActor : MonoBehaviour, IPlayerActor
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

            public GameObject[] Avatars => new[] { _avatar1, _avatar2, _avatar3, _avatar4, _avatar5 };
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

        private bool _hasTarget;
        private Vector3 _targetPosition;
        private Vector3 _tempPosition;

        private BattlePlayMode _playMode;
        private int _poseIndex;
        private bool _isStunned;
        private Coroutine _stunnedCoroutine;

        private Color[] _skillColors;
        private PoseManager _avatarPose;
        private PoseManager _shieldPose;

        private bool CanMove => _hasTarget && !_isStunned && (_playMode == BattlePlayMode.Normal || _playMode == BattlePlayMode.Ghosted);

        private bool CanAcceptMove => !_isStunned && (_playMode == BattlePlayMode.Normal || _playMode == BattlePlayMode.Ghosted);

        private bool IsBuffedOrDeBuffed => _isStunned;

        public static PlayerActor Instantiate(IPlayerDriver playerDriver, PlayerActor playerPrefab)
        {
            Debug.Log($"prefab {playerPrefab.name}");

            var playerPos = playerDriver.PlayerPos;
            var instantiationPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);

            var playerActor = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            var playerTag = $"{playerPos}:{playerDriver.NickName}";
            playerActor.name = playerActor.name.Replace("Clone", playerTag);
            playerActor.SetPlayerDriver(playerDriver);
            return playerActor;
        }

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
            // Wait until PlayerDriver is assigned.
            enabled = false;
        }

        private void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            Debug.Log($"{name}");
            // Now we are good to go.
            _playerDriver = playerDriver;
            _hasPlayer = true;
            _actorNumber = playerDriver.ActorNumber;
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
            var skillColor = _skillColors[(int)model.MainDefence];
            _avatarPose = new PoseManager(_settings._avatar.Avatars);
            _avatarPose.Reset(BattlePlayMode.Normal, true, skillColor, true);
            _shieldPose = new PoseManager(_settings._shield.Shields);
            _shieldPose.Reset(BattlePlayMode.Normal, true, skillColor, false);
            UpdatePlayerText();
            StartCoroutine(ThrottledLogger());
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
            _tempPosition = Vector3.MoveTowards(_transform.position, _targetPosition, _speed * Time.deltaTime);
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
                _debug._playerText.text = $"---";
                return;
            }
            if (!IsBuffedOrDeBuffed)
            {
                _debug._playerModeOrBuff = _playMode.ToString().ToLower()[0];
            }
            _debug._playerText.text = $"{_actorNumber}{_poseIndex}{_debug._playerModeOrBuff}";
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

        private float _debugLogTime;
        private string _debugLogMessage;

        private IEnumerator ThrottledLogger()
        {
            if (!Application.isEditor)
            {
                yield break;
            }
            _debugLogTime = Time.time;
            _debugLogMessage = null;
            for (;;)
            {
                if (Time.time > _debugLogTime)
                {
                    if (_debugLogMessage != null)
                    {
                        Debug.Log(_debugLogMessage);
                        _debugLogMessage = null;
                        _debugLogTime = Time.time + 1f;
                    }
                }
                yield return null;
            }
        }

        #endregion

        #region IPlayerActor Interface

        private float _speed;

        int IPlayerActor.MaxPoseIndex => _avatarPose.MaxPoseIndex;

        Vector2 IPlayerActor.Position => _transform.position;

        float IPlayerActor.Speed
        {
            get => _speed;
            set => _speed = value;
        }

        void IPlayerActor.Rotate(bool isUpsideDown)
        {
            Debug.Log($"{name} isUpsideDown {isUpsideDown}");
            _settings._geometryRoot.Rotate(isUpsideDown);
            UpdatePlayerText();
        }

        void IPlayerActor.MoveTo(Vector2 targetPosition)
        {
            var canDo = CanAcceptMove;
            SetThrottledDebugLogMessage($"{name} MoveTo {(Vector2)_targetPosition} <- {targetPosition} Speed {_speed} canDo {canDo}");
            if (!canDo)
            {
                return;
            }
            _hasTarget = true;
            _targetPosition = targetPosition;
        }

        void IPlayerActor.SetCharacterPose(int poseIndex)
        {
            Debug.Log($"{name} {_poseIndex} <- {poseIndex}");
            _poseIndex = poseIndex;
            _shieldPose.SetPose(poseIndex);
            _avatarPose.SetPose(poseIndex);
            UpdatePlayerText();
        }

        void IPlayerActor.SetPlayMode(BattlePlayMode playMode)
        {
            Debug.Log($"{name} {_playMode} <- {playMode}");
            _playMode = playMode;
            _shieldPose.SetPlayMode(playMode);
            _avatarPose.SetPlayMode(playMode);
            UpdatePlayerText();
        }

        void IPlayerActor.SetShieldVisibility(bool state)
        {
            Debug.Log($"{name} {_shieldPose.IsVisible} <- {state}");
            _shieldPose.SetVisible(state);
            UpdatePlayerText();
        }

        void IPlayerActor.SetBuff(PlayerBuff buff, float duration)
        {
            Debug.Log($"{name} start {buff} {duration:0.0}s");
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

            // Brute force way to disable everything!
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var childCollider in colliders)
            {
                childCollider.enabled = false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper class to manage <c>GameObject</c> hierarchy for <c>PlayerActor</c> (shields and avatars).
    /// </summary>
    internal class PoseManager
    {
        /// <summary>
        /// Internal state that defines how this pose behaves.
        /// </summary>
        private class PoseState
        {
            public bool IsVisible;
            public BattlePlayMode PlayMode;
        }

        private readonly PoseState _state;
        private readonly GameObject[] _avatars;
        private readonly Collider2D[] _colliders;

        public bool IsVisible => _state.IsVisible;

        public int MaxPoseIndex => _childCount - 1;

        private GameObject _currentAvatar;
        private Collider2D _currentCollider;

        private Transform _parentTransform;
        private int _childCount;

        public PoseManager(GameObject[] avatars)
        {
            _state = new PoseState();
            _avatars = avatars;
            _colliders = new Collider2D[avatars.Length];
            var index = -1;
            foreach (var avatar in _avatars)
            {
                _colliders[++index] = avatar.GetComponentsInChildren<Collider2D>(true)[0];
            }
        }

        public void SetVisible(bool isVisible)
        {
            _state.IsVisible = isVisible;
            _currentAvatar.SetActive(isVisible);
            _currentCollider.enabled = _state.PlayMode == BattlePlayMode.Normal || _state.PlayMode == BattlePlayMode.Frozen;
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

        public void Reset(BattlePlayMode playMode, bool isVisible, Color avatarColor, bool setAvatarColor)
        {
            _currentAvatar = _avatars[0];
            _currentCollider = _colliders[0];

            _childCount = _avatars.Length;
            var firstPosition = _currentAvatar.transform.position;
            for (var i = 0; i < _childCount; ++i)
            {
                var child = _avatars[i];
                if (setAvatarColor)
                {
                    var spriteRenderer = child.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = avatarColor;
                }
                if (i > 0)
                {
                    child.transform.position = firstPosition;
                    child.gameObject.SetActive(false);
                }
            }

            _state.PlayMode = playMode;
            _state.IsVisible = isVisible;
            SetPose(0);
        }
    }
}