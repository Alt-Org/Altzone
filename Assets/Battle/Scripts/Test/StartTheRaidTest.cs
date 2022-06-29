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
            StartCoroutine(StartRaid(player));
        }

        private IEnumerator StartRaid(IPlayerDriver player)
        {
            Debug.Log($"{player.NickName} raidSimulationTime {_raidSimulationTime}");
            yield return null;
            // Simulate RAID start - player can not move in Battle
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            yield return new WaitForSeconds(_raidSimulationTime);
            // Simulate RAID end - set player "movable" again in Battle
            StartCoroutine(ReturnFromRaid(player));
        }

        private IEnumerator ReturnFromRaid(IPlayerDriver player)
        {
            Debug.Log($"{player.NickName}");
            player.SetPlayMode(BattlePlayMode.RaidReturn);
            yield return null;
            // TODO: Check if the ball is on our team gameplay are or not and set play mode accordingly ghosted or normal!
            yield return null;
            _isRaiding = false;
            _playerTransform = null;
        }
    }
}