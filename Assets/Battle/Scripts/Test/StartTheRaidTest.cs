using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test
{
    internal class StartTheRaidTest : MonoBehaviour, IBattleBridge
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private Transform _playerTransform;

        public bool IsRaiding => _isRaiding;

        public int ActorNumber => _actorNumber;

        private IPlayerManager _playerManager;
        private RaidBridge _raidBridge;

        private IEnumerator Start()
        {
            var failureTime = Time.time + 2f;
            yield return new WaitUntil(() => (_raidBridge ??= FindObjectOfType<RaidBridge>()) != null || Time.time > failureTime);
            if (_raidBridge != null)
            {
                _raidBridge.SetBattleBridge(this);
            }
        }

        private void OnEnable()
        {
            _playerManager = Context.PlayerManager;
        }

        private void OnDestroy()
        {
            if (_raidBridge != null)
            {
                _raidBridge.SetBattleBridge(null);
            }
        }

        private void ResetState()
        {
            _isRaiding = false;
            _actorNumber = 0;
            _playerTransform = null;
        }

        public void ClosedRaid(int actorNumber)
        {
            Debug.Log($"actorNumber  {_actorNumber} <- {actorNumber}");
            var player = _playerManager.GetPlayerByActorNumber(actorNumber);
            if (player == null)
            {
                ResetState();
                return;
            }
            StartCoroutine(OnClosedRaid(player));
        }

        public void StartTheRaid(IPlayerDriver player)
        {
            Assert.IsFalse(_isRaiding, "_isRaiding");
            _isRaiding = true;
            _actorNumber = player.ActorNumber;
            _playerTransform = player.PlayerTransform;
            StartCoroutine(StartRaid(player));
        }

        public void ContinueTheRaid(int actorNumber)
        {
            if (!_isRaiding || _actorNumber != actorNumber)
            {
                return;
            }
            Debug.Log($"actorNumber {_actorNumber}");
            if (_raidBridge != null)
            {
                _raidBridge.AddRaidBonus(_actorNumber);
            }
        }

        private IEnumerator StartRaid(IPlayerDriver player)
        {
            Debug.Log($"actorNumber {_actorNumber}");
            yield return null;
            // Simulate RAID start - player can not move in Battle
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            if (_raidBridge != null)
            {
                _raidBridge.ShowRaid(player.ActorNumber);
            }
        }

        private IEnumerator OnClosedRaid(IPlayerDriver player)
        {
            Debug.Log($"actorNumber {_actorNumber}");
            player.SetPlayMode(BattlePlayMode.RaidReturn);
            yield return null;
            ResetState();
            this.Publish(new UiEvents.ExitRaid(player));
        }
    }
}