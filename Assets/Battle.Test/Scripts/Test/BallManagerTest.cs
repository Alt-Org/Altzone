using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Test.Scripts.Battle.Ball;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Test.Scripts.Test
{
    [RequireComponent(typeof(BallManager))]
    internal class BallManagerTest : MonoBehaviour
    {
        [Header("Test Actions")] public bool _setBallState;
        public bool _setBallPosition;
        public bool _setBallSpeed;
        public bool _setBallSpeedAndDir;
        public bool _stopBall;

        [Header("Test Settings")] public Vector2 _position;
        public float _speed = 1f;
        public Vector2 _direction = Vector2.one;
        public BallState _state;

        [Header("Photon Master Client")] public bool _isOnGuiStart;
        public bool _isAutoStart;
        public int _requiredPlayerCount;
        [ReadOnly] public int _realPlayerCount;

        private IBallManager _ball;

        private void Awake()
        {
            Debug.Log($"IsMasterClient {PhotonNetwork.IsMasterClient} OfflineMode {PhotonNetwork.OfflineMode}");
        }

        private IEnumerator Start()
        {
            for (;;)
            {
                _ball = BallManager.Get();
                if (_ball != null)
                {
                    break;
                }
                yield return null;
            }
            var isStart = _isAutoStart || _isOnGuiStart;
            if (!isStart)
            {
                yield break;
            }
            if (_isAutoStart && PhotonNetwork.OfflineMode)
            {
                StartTheBall();
                yield break;
            }
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                yield break;
            }
            if (_isOnGuiStart)
            {
                var guiStart = gameObject.AddComponent<OnGuiStart>();
                guiStart._tester = this;
                yield break;
            }
            while (PhotonNetwork.InRoom)
            {
                _realPlayerCount = PhotonBattle.CountRealPlayers();
                if (_realPlayerCount >= _requiredPlayerCount)
                {
                    StartTheBall();
                    yield break;
                }
                yield return null;
            }
        }

        public void StartTheBall()
        {
            _setBallState = true;
            _setBallPosition = true;
            _setBallSpeedAndDir = true;
        }

        private void Update()
        {
            if (_setBallState)
            {
                _setBallState = false;
                _ball.SetBallState(_state);
            }
            if (_setBallPosition)
            {
                _setBallPosition = false;
                _ball.SetBallPosition(_position);
            }
            if (_setBallSpeed)
            {
                _setBallSpeed = false;
                _ball.SetBallSpeed(_speed, Vector2.zero);
            }
            if (_setBallSpeedAndDir)
            {
                _setBallSpeedAndDir = false;
                _ball.SetBallSpeed(_speed, _direction);
            }
            else if (_stopBall)
            {
                _stopBall = false;
                _ball.SetBallSpeed(0, Vector2.zero);
            }
        }
    }

    internal class OnGuiStart : MonoBehaviour
    {
        public Key _controlKey = Key.F4;

        public BallManagerTest _tester;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private int _windowId;
        private Rect _windowRect;
        private string _windowTitle;

        private void OnEnable()
        {
            _windowId = (int)DateTime.Now.Ticks;
            _windowRect = new Rect(0, 0, Screen.width, Screen.height / 5f);
            _windowTitle = "Start the Ball when players are ready";
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame)
            {
                enabled = false;
                _tester.StartTheBall();
            }
        }
        private void OnGUI()
        {
            _windowRect = GUILayout.Window(_windowId, _windowRect, DebugWindow, _windowTitle);
        }

        private void DebugWindow(int windowId)
        {
            GUILayout.Label(" ");
            if (GUILayout.Button("Start the Ball (F4)"))
            {
                enabled = false;
                _tester.StartTheBall();
            }
        }
#endif
    }
}