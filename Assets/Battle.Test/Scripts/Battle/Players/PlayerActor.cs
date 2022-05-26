using System;
using System.Collections;
using System.Diagnostics;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using TMPro;
using UnityEngine;

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
        private const int InvalidPlayerDriver = -1;

        [Serializable]
        internal class PlayerSettings
        {
            public Transform _geometryRoot;
            public GameObject _shield;
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
        private Transform _transform;
        private int _actorNumber;

        private bool _isMoving;
        private Vector3 _targetPosition;
        private Vector3 _tempPosition;

        private BattlePlayMode _playMode;
        private int _poseIndex;

        private bool IsBuffedOrDeBuffed => _isStunned;

        private bool _isStunned;
        private Coroutine _stunnedCoroutine;

        private Color[] _skillColors;

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
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.color = _skillColors[(int)model.MainDefence];
            }
            UpdatePlayerText();
            StartCoroutine(ThrottledLogger());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (!_isMoving || _isStunned)
            {
                return;
            }
            _tempPosition = Vector3.MoveTowards(_transform.position, _targetPosition, _speed * Time.deltaTime);
            _transform.position = _tempPosition;
            _isMoving = !(Mathf.Approximately(_tempPosition.x, _targetPosition.x) && Mathf.Approximately(_tempPosition.y, _targetPosition.y));
        }

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdatePlayerText()
        {
            if (!_debug._isShowPlayerText)
            {
                return;
            }
            if (_actorNumber == InvalidPlayerDriver)
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
            SetThrottledDebugLogMessage($"{name} MoveTo {(Vector2)_targetPosition} <- {targetPosition} Speed {_speed}");
            _isMoving = true;
            _targetPosition = targetPosition;
        }

        void IPlayerActor.SetCharacterPose(int poseIndex)
        {
            Debug.Log($"{name} {_poseIndex} <- {poseIndex}");
            _poseIndex = poseIndex;
            UpdatePlayerText();
        }

        void IPlayerActor.SetPlayMode(BattlePlayMode playMode)
        {
            Debug.Log($"{name} {_playMode} <- {playMode}");
            _playMode = playMode;
            UpdatePlayerText();
        }

        void IPlayerActor.SetShieldVisibility(bool state)
        {
            Debug.Log($"{name} {_settings._shield.activeSelf} <- {state}");
            _settings._shield.SetActive(state);
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
            Debug.Log($"{name} {_actorNumber} <- {InvalidPlayerDriver}");
            _playerDriver = null;

            _actorNumber = InvalidPlayerDriver;
            UpdatePlayerText();

            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var childCollider in colliders)
            {
                childCollider.enabled = false;
            }
        }

        #endregion
    }
}