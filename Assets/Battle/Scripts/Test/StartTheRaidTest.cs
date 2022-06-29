using System.Collections;
using Battle.Scripts.Battle;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Test
{
    internal class StartTheRaidTest : MonoBehaviour
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

        public void StartTheRaid(IPlayerDriver player)
        {
            Debug.Log($"{player}");
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
            _isRaiding = false;
            _playerTransform = null;
            this.Publish(new UiEvents.ExitRaid(player));
        }
    }
}