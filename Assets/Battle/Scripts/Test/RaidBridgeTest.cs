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
    internal class RaidBridgeTest : MonoBehaviour, IBattleBridge, IRaidBridge
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private bool _isExitRaidModeCalled;
        [SerializeField, ReadOnly] private Transform _playerTransform;

        public bool IsRaiding => _isRaiding;

        public int ActorNumber => _actorNumber;

        public int TeamNumber => _teamNumber;

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
            _teamNumber = PhotonBattle.NoTeamValue;
            _playerTransform = null;
            _isExitRaidModeCalled = false;
        }

        #region Incoming: IBattleBridge officially registered for the bridge

        public void PlayerClosedRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber}");
            if (!_isExitRaidModeCalled)
            {
                StartCoroutine(OnExitRaidMode());
            }
        }

        #endregion

        #region Outgoing: IRaidBridge test implementation

        public void ShowRaid(int actorNumber)
        {
            if (_isRaiding)
            {
                return;
            }
            Debug.Log($"actorNumber {actorNumber}");
            var player = _playerManager.GetPlayerByActorNumber(actorNumber);
            if (player == null)
            {
                return;
            }
            StartCoroutine(OnStartRaidMode(player));
        }

        public void AddRaidBonus()
        {
            if (!_isRaiding)
            {
                return;
            }
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber}");
            if (_raidBridge != null)
            {
                // Currently there are no UI update related stuff to this event - but later could be!
                _raidBridge.AddRaidBonus();
            }
        }

        public void HideRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            if (!_isExitRaidModeCalled)
            {
                StartCoroutine(OnExitRaidMode());
            }
        }

        #endregion

        private IEnumerator OnStartRaidMode(IPlayerDriver player)
        {
            Assert.IsFalse(_isRaiding, "_isRaiding");
            _isRaiding = true;
            _actorNumber = player.ActorNumber;
            _teamNumber = player.TeamNumber;
            _playerTransform = player.PlayerTransform;
            _isExitRaidModeCalled = false;
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber}");
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            yield return null;
            if (_raidBridge != null)
            {
                _raidBridge.ShowRaid(player.ActorNumber);
            }
        }

        private IEnumerator OnExitRaidMode()
        {
            // Raid mode can be exited via different routes and we must prevent these overlapping or "recursive" calls happening.
            _isExitRaidModeCalled = true;
            if (_raidBridge != null)
            {
                _raidBridge.HideRaid();
            }
            yield return null;
            var player = _playerManager.GetPlayerByActorNumber(_actorNumber);
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding} player {player}");
            ResetState();
            player?.SetPlayMode(BattlePlayMode.RaidReturn);
            yield return null;
            this.Publish(new UiEvents.ExitRaidNotification(player));
        }
    }
}