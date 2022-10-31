using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Ui;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;

namespace Battle.Scripts.Test
{
    internal class UiEventListenerTest : MonoBehaviour
    {
        [Header("Debug Settings"), SerializeField] private bool _isDisableShowComponentErrors;

        private int _roomStartDelay;
        private int _slingshotDelay;

        private int _noComponentStart;
        private int _noComponentRestart;

        private void Awake()
        {
            ScoreFlashNet.RegisterEventListener();

            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _roomStartDelay = variables._battleRoomStartDelay;
            _slingshotDelay = variables._battleSlingshotDelay;
        }

        private void OnEnable()
        {
            var counter = _isDisableShowComponentErrors ? 0 : 3;
            _noComponentStart = counter;
            _noComponentRestart = counter;

            this.Subscribe<UiEvents.StartBattle>(OnStartBattle);
            this.Subscribe<UiEvents.RestartBattle>(OnRestartBattle);

            this.Subscribe<UiEvents.SlingshotStart>(OnSlingshotStart);
            this.Subscribe<UiEvents.SlingshotEnd>(OnSlingshotEnd);
            
            this.Subscribe<UiEvents.HeadCollision>(OnHeadCollision);
            this.Subscribe<UiEvents.ShieldCollision>(OnShieldCollision);
            this.Subscribe<UiEvents.WallCollision>(OnWallCollision);
            this.Subscribe<UiEvents.TeamActivation>(OnTeamActivation);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnStartBattle(UiEvents.StartBattle data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{data}");
            var startTheBallTest = FindObjectOfType<StartTheBallTest>();
            if (startTheBallTest == null)
            {
                if (--_noComponentStart >= 0)
                {
                    ScoreFlashNet.Push("NO START COMPONENT");
                }
                return;
            }
            startTheBallTest.StartBallFirstTime();
            StartCoroutine(SimulateCountdown(_roomStartDelay));
        }

        private void OnRestartBattle(UiEvents.RestartBattle data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{data}");
            var startTheBallTest = FindObjectOfType<StartTheBallTest>();
            if (startTheBallTest == null)
            {
                if (--_noComponentRestart >= 0)
                {
                    ScoreFlashNet.Push("NO RESTART COMPONENT");
                }

                return;
            }
            ScoreFlashNet.Push("RESTART");
            startTheBallTest.RestartBallInGame(data.PlayerToStart);
            StartCoroutine(SimulateCountdown(_slingshotDelay));
        }

        private void OnSlingshotStart(UiEvents.SlingshotStart data)
        {
            Debug.Log($"{data}");
        }
        
        private void OnSlingshotEnd(UiEvents.SlingshotEnd data)
        {
            Debug.Log($"{data}");
        }
        
        private static void OnHeadCollision(UiEvents.HeadCollision data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{data}");
            var collision = data.Collision;
            var contactPoint = collision.GetFirstContactPoint();
            ScoreFlashNet.Push("HEAD", contactPoint.point);
        }

        private static void OnShieldCollision(UiEvents.ShieldCollision data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{data}");
            var collision = data.Collision;
            var contactPoint = collision.GetFirstContactPoint();
            var info = data.HitType;
            ScoreFlashNet.Push($"SHIELD {info}", contactPoint.point);
        }

        private void OnWallCollision(UiEvents.WallCollision data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{data}");
        }

        private void OnTeamActivation(UiEvents.TeamActivation data)
        {
            Debug.Log($"{data}");
        }

        private static IEnumerator SimulateCountdown(int countdownDelay)
        {
            var delay = new WaitForSeconds(1f);
            while (countdownDelay >= 0)
            {
                ScoreFlashNet.Push(countdownDelay.ToString());
                countdownDelay--;
                yield return delay;
            }
        }
    }
}
