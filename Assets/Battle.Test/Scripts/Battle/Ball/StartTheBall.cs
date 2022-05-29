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
            StartCoroutine(SelectStartingTeam());
        }

        public void RestartBallInGame()
        {
            Debug.Log($"{name}");
        }

        private IEnumerator SelectStartingTeam()
        {
            _startingTeam = PhotonBattle.NoTeamValue;
            var delay = new WaitForSeconds(_delayToStart);
            var tracker1 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamBlueValue);
            var tracker2 = _gameplayManager.GetTeamSnapshotTracker(PhotonBattle.TeamRedValue);
            yield return delay;
            tracker1.StopTracking();
            tracker2.StopTracking();
            var distance1 = Mathf.Sqrt(tracker1.GetSqrDistance);
            var distance2 = Mathf.Sqrt(tracker2.GetSqrDistance);
            _startingTeam = distance1 > distance2 ? PhotonBattle.TeamBlueValue : PhotonBattle.TeamRedValue;
            var team = _gameplayManager.GetBattleTeam(_startingTeam);
            var playerCount = team.PlayerCount;
            Debug.Log($"{name} dist1 {distance1:0.00} dist2 {distance2:0.00} startingTeam {_startingTeam} players {playerCount}");
            _ballManager.SetBallState(BallState.NoTeam);
            yield return null;
            
            float speed = 1f;
            Vector2 direction;

            var center = Vector3.zero;
            var transform1 = team.FirstPlayer.PlayerTransform;
            var pos1 = transform1.position;
            if (playerCount == 1)
            {
                direction = center - pos1;
            }
            else
            {
                var transform2 = team.SecondPlayer.PlayerTransform;
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
            Debug.Log($"{name} speed {speed} direction {direction.normalized}");
            _ballManager.SetBallSpeed(speed, direction);
        }
    }
}