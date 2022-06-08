using System.Collections;
using Battle.Test.Scripts.Battle.Ball;
using Battle.Test.Scripts.Battle.Players;
using Battle.Test.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

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

        [Header("Photon Master Client")] public bool _isStartTheBallStart;
        public bool _isStartOnGui;
        public bool _isAutoStartTest;
        public bool _useAttackSpeed;
        public int _requiredPlayerCount;
        [ReadOnly] public int _realPlayerCount;

        [Header("Start The Ball")] public StartTheBallTest _startTheBall;

        [Header("Event Settings"), SerializeField] private bool _isListenEvents;

        [Header("Timer")] public SimpleRoomTimer _timer;

        private IBallManager _ballManager;
        private bool _isKeyPressed;
        private bool _isGamePlayStarted;

        private void Awake()
        {
            Debug.Log($"IsMasterClient {PhotonNetwork.IsMasterClient} OfflineMode {PhotonNetwork.OfflineMode}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            ScoreFlashNet.RegisterEventListener();
            if (_isListenEvents)
            {
                Debug.Log($"{name} listen for TeamsAreReadyForGameplay event");
                _isGamePlayStarted = false;
                this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
            }
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            Debug.Log($"{data.TeamBlue} and {data.TeamRed?.ToString() ?? "null"}");
            _isGamePlayStarted = true;
        }

        private void Update()
        {
            if (_setBallState)
            {
                _setBallState = false;
                _ballManager.SetBallState(_state);
            }
            if (_setBallPosition)
            {
                _setBallPosition = false;
                _ballManager.SetBallPosition(_position);
            }
            if (_setBallSpeed)
            {
                _setBallSpeed = false;
                _ballManager.SetBallSpeed(_speed);
            }
            if (_setBallSpeedAndDir)
            {
                _setBallSpeedAndDir = false;
                _ballManager.SetBallSpeed(_speed, _direction);
            }
            else if (_stopBall)
            {
                _stopBall = false;
                _ballManager.SetBallSpeed(0, Vector2.zero);
            }
        }

        private IEnumerator Start()
        {
            // Start flag priorities
            if (_startTheBall == null)
            {
                _isStartTheBallStart = false;
            }
            if (_isStartTheBallStart)
            {
                _isStartOnGui = false;
                _isAutoStartTest = false;
            }
            else if (_isStartOnGui)
            {
                _isAutoStartTest = false;
            }
            yield return new WaitUntil(() => (_ballManager ??= BallManager.Get()) != null);

            var isStart = _isStartTheBallStart || _isStartOnGui || _isAutoStartTest;
            if (!isStart)
            {
                yield break;
            }
            var gameplayManager = GameplayManager.Get();
            if (_useAttackSpeed)
            {
                yield return new WaitUntil(() => gameplayManager.LocalPlayer != null);
                var localPlayer = gameplayManager.LocalPlayer;
                _speed = localPlayer.CharacterModel.Attack;
            }
            if (_isAutoStartTest && PhotonNetwork.OfflineMode)
            {
                TestAutoStart();
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
            if (_isStartTheBallStart || _isStartOnGui)
            {
                var guiStart = gameObject.AddComponent<OnGuiWindowTest>();
                guiStart._windowTitle = "Start the Ball when ALL players are ready";
                guiStart._buttonCaption = $" \r\nStart the Ball ({guiStart._controlKey})\r\n ";
                guiStart.OnKeyPressed = TestAutoStart;
                if (_isListenEvents)
                {
                    yield return new WaitUntil(() => _isKeyPressed || _isGamePlayStarted);
                    if (!_isKeyPressed && _isGamePlayStarted)
                    {
                        guiStart.Hide();
                        ScoreFlashNet.Push("AUTOSTART");
                        TestAutoStart();
                    }
                }
                yield break;
            }
            if (PhotonNetwork.InRoom)
            {
                yield return new WaitUntil(() => gameplayManager.PlayerCount >= _requiredPlayerCount);
                TestAutoStart();
            }
        }

        private void TestAutoStart()
        {
            if (_timer != null)
            {
                _timer.StartTimer();
            }
            if (_isStartTheBallStart)
            {
                _startTheBall.StartBallFirstTime();
                return;
            }
            _isKeyPressed = true;
            _setBallState = true;
            _setBallPosition = true;
            _setBallSpeedAndDir = true;
        }
    }
}