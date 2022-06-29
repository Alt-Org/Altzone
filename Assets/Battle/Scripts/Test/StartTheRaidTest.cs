using System;
using System.Collections;
using Battle.Scripts.Battle;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class StartTheRaidTest : MonoBehaviour
    {
        [Header("Debug Settings"), SerializeField] private float _raidSimulationTime = 10f;

        [Header("Live Data"), SerializeField] private bool _isRaiding;
        [SerializeField] private Transform _playerTransform;

        public bool CanRaid => !_isRaiding;
        
        private IPlayerManager _playerManager;

        private void OnEnable()
        {
            _playerManager = Context.PlayerManager;
        }

        public void StartTheRaid(int teamNumber)
        {
            Debug.Log($"{teamNumber}");
            var player = _playerManager.GetPlayerByLastBallHitTime(teamNumber);
            if (player == null)
            {
                return;
            }
            _isRaiding = true;
            _playerTransform = player.PlayerTransform;
            StartCoroutine(StartRaid(player, _raidSimulationTime, ReturnFromRaid));
        }

        private static IEnumerator StartRaid(IPlayerDriver player, float raidSimulationTime, Action<IPlayerDriver> returnAction)
        {
            Debug.Log($"{player.NickName} raidSimulationTime {raidSimulationTime}");
            yield return null;
            // Simulate RAID start - player can not move in Battle
            player.SetPlayMode(BattlePlayMode.Ghosted);
            player.SetStunned(raidSimulationTime);
            yield return new WaitForSeconds(raidSimulationTime);
            // Simulate RAID end - set player "movable" again in Battle
            returnAction(player);
        }

        private void ReturnFromRaid(IPlayerDriver player)
        {
            Debug.Log($"{player.NickName}");
            player.SetPlayMode(BattlePlayMode.Ghosted);
            _isRaiding = false;
            _playerTransform = null;
        }
    }
}