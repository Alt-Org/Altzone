using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Test.Scripts.Battle.Players;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Ball
{
    public class StartTheBall : MonoBehaviour
    {
        [Header("Live Data"), SerializeField, ReadOnly] private int _startingTeam;

        private IGameplayManager _gameplayManager;
        private IBallManager _ballManager;
        private float _delayToStart;

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _gameplayManager = GameplayManager.Get();
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _delayToStart = variables._roomStartDelay;
            _startingTeam = PhotonBattle.NoTeamValue;
        }

        public void StartBallFirstTime()
        {
            _ballManager = BallManager.Get();
            Debug.Log($"{name} {_ballManager} delayToStart {_delayToStart}");
            Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
            Assert.IsTrue(PhotonNetwork.IsMasterClient, "PhotonNetwork.IsMasterClient");
            Assert.IsNotNull(_ballManager, "_ballManager != null");
            _ballManager.SetBallState(BallState.Hidden);
            StartCoroutine(StartBallFirstTimeRoutine());
        }

        public void RestartBallInGame()
        {
            Debug.Log($"{name}");
        }

        private IEnumerator StartBallFirstTimeRoutine()
        {
            _startingTeam = PhotonBattle.NoTeamValue;
            var delay = new WaitForSeconds(_delayToStart);
            var tracker1 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
            var tracker2 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
            yield return delay;
            tracker1.StopTracking();
            tracker2.StopTracking();
            _ballManager.SetBallState(BallState.NoTeam);
            yield return null;

            float startDistance;
            float otherDistance;
            {
                var distance1 = Mathf.Sqrt(tracker1.GetSqrDistance);
                var distance2 = Mathf.Sqrt(tracker2.GetSqrDistance);
                if (distance1 > distance2)
                {
                    _startingTeam = PhotonBattle.TeamBlueValue;
                    startDistance = distance1;
                    otherDistance = distance2;
                }
                else
                {
                    _startingTeam = PhotonBattle.TeamRedValue;
                    startDistance = distance2;
                    otherDistance = distance1;
                }
            }
            var startTeam = _gameplayManager.GetBattleTeam(_startingTeam);
            var otherTeam = _gameplayManager.GetOppositeTeam(_startingTeam);
            var startAttack = startTeam.Attack;
            var otherAttack = otherTeam?.Attack ?? 0;

            Debug.Log(
                $"{name} START attack {startAttack} dist {startDistance:0.00} - OTHER attack {otherAttack} dist {otherDistance:0.00} TEAM {_startingTeam}");

            Vector2 direction;
            var center = Vector3.zero;
            var transform1 = startTeam.FirstPlayer.PlayerTransform;
            var pos1 = transform1.position;
            if (startTeam.PlayerCount == 1)
            {
                direction = center - pos1;
            }
            else
            {
                var transform2 = startTeam.SecondPlayer.PlayerTransform;
                var pos2 = transform2.position;
                var dist1 = Mathf.Abs((pos1 - center).sqrMagnitude);
                var dist2 = Mathf.Abs((pos2 - center).sqrMagnitude);
                if (dist1 < dist2)
                {
                    direction = pos1 - pos2;
                }
                else
                {
                    direction = pos2 - pos1;
                }
            }

            var speed = startAttack * startDistance - otherAttack * otherDistance;
            Debug.Log($"{name} speed {speed} direction {direction.normalized}");
            if (speed == 0)
            {
                // This can happen if
                // - both teams has the same attack value and
                // - no player has moved (thus distances can be floating point exactly the same for both teams)
                speed = 1;
                if (otherTeam == null)
                {
                    direction = Vector2.one * (_startingTeam == PhotonBattle.TeamBlueValue ? 1 : -1);
                }
                else
                {
                    direction = Vector2.one * (Time.frameCount % 2 == 0 ? 1 : -1);
                }
            }
            _ballManager.SetBallSpeed(speed, direction);
        }
    }
}