using System.Collections;
using Battle0.Scripts.Battle;
using Battle0.Scripts.Ui;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle0.Scripts.Test
{
    internal class BallManagerTest : MonoBehaviour
    {
        private const string Tooltip1 = "Set ball moving using IBallManager interface after level has been loaded";
        private const string Tooltip2 = "Set ball moving using StartBattle event after level has been loaded";

        [Header("Test Actions")] public bool _setBallState;
        public bool _setBallPosition;
        public bool _setBallSpeed;
        public bool _setBallSpeedAndDir;
        public bool _stopBall;

        [Header("Test Settings")] public Vector2 _position;
        public float _speed = 1f;
        public Vector2 _direction = Vector2.one;
        public BallState _state;

        [Header("Photon Master Client"), Tooltip(Tooltip1)] public bool _isAutoStartBall;
        public bool _isStartOnGui;
        [Tooltip(Tooltip2)]public bool _isStartTheBallTest;

        [Header("Auto Start Delay")] public float _autoStartDelay;
        
        private IBallManager _ballManager;

        private void Awake()
        {
            Debug.Log($"IsMasterClient {PhotonNetwork.IsMasterClient} OfflineMode {PhotonNetwork.OfflineMode}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
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
            yield return new WaitUntil(() => (_ballManager ??= Context.BallManager) != null);
            yield return null;
            var isAutoStart = _isAutoStartBall || _isStartOnGui || _isStartTheBallTest;
            if (!isAutoStart)
            {
                yield break;
            }
            if (_autoStartDelay > 0)
            {
                yield return new WaitForSeconds(_autoStartDelay);
            }
            if (PhotonNetwork.OfflineMode && _isAutoStartBall)
            {
                TestAutoStart();
                yield break;
            }
            yield return new WaitUntil(() => PhotonNetwork.InRoom);
            if (!PhotonNetwork.IsMasterClient)
            {
                yield break;
            }
            if (_isAutoStartBall)
            {
                TestAutoStart();
                yield break;
            }
            if (_isStartOnGui)
            {
                StartOnGui();
                yield break;
            }
            StartTheBallTest();
        }

        private void TestAutoStart()
        {
            _setBallState = true;
            _setBallPosition = true;
            _setBallSpeedAndDir = true;
        }

        private void StartOnGui()
        {
            var guiStart = gameObject.AddComponent<OnGuiWindowHelper>();
            guiStart._windowTitle = "Start the Ball when ALL players are ready";
            guiStart._buttonCaption = $" \r\nStart the Ball ({guiStart._controlKey})\r\n ";
            guiStart.OnKeyPressed = TestAutoStart;
        }

        private void StartTheBallTest()
        {
            this.Publish(new UiEvents.StartBattle());
            this.Publish(new UiEvents.StartAnimation());
        }
    }
}
