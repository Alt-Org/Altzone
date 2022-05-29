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
            _delayToStart = variables._ballRestartDelay;
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
            var distance1= tracker1.GetDistance;
            var distance2= tracker2.GetDistance;
            _startingTeam = distance1 > distance2 ? PhotonBattle.TeamBlueValue : PhotonBattle.TeamRedValue;
            Debug.Log($"{name} dist1 {distance1:0.00} dist2 {distance2:0.00} startingTeam {_startingTeam}");
            _ballManager.SetBallState(BallState.NoTeam);
        }
    }
}