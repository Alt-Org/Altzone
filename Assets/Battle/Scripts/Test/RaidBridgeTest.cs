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
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private bool _isExitRaidModeCalled;

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

        #region Incoming: IBattleBridge officially registered for the bridge

        public void PlayerClosedRaid()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber}");
            if (!_isExitRaidModeCalled)
            {
                StartCoroutine(OnExitRaidMode());
            }
        }

        #endregion

        #region Outgoing: IRaidBridge test implementation

        public void ShowRaid(IPlayerInfo playerInfo)
        {
            Debug.Log($"playerInfo {playerInfo} isRaiding {_isRaiding}");
            if (_isRaiding)
            {
                return;
            }
            var player = _playerManager.GetPlayerByActorNumber(playerInfo.ActorNumber);
            _isRaiding = true;
            StartCoroutine(OnStartRaidMode(player));
        }

        public void AddRaidBonus()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber} isRaiding {_isRaiding}");
            var validState = _isRaiding && !_isExitRaidModeCalled;
            if (!validState)
            {
                return;
            }
            if (_raidBridge != null)
            {
                // Currently there are no UI update related stuff to this event - but later could be!
                _raidBridge.AddRaidBonus();
            }
        }

        public void HideRaid()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber} isRaiding {_isRaiding}");
            var validState = _isRaiding && !_isExitRaidModeCalled;
            if (!validState)
            {
                StartCoroutine(OnExitRaidMode());
            }
        }

        #endregion

        private IEnumerator OnStartRaidMode(IPlayerDriver player)
        {
            _isRaiding = true;
            _isExitRaidModeCalled = false;
            _teamNumber = player.TeamNumber;
            _actorNumber = player.ActorNumber;
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber}");
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            yield return null;
            if (_raidBridge != null)
            {
                _raidBridge.ShowRaid(player);
            }
        }

        private IEnumerator OnExitRaidMode()
        {
            // Raid mode can be exited via different routes and we must prevent these overlapping or "recursive" calls happening.
            var exitingPlayer = _playerManager.GetPlayerByActorNumber(_actorNumber);
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber} isRaiding {_isRaiding} player {exitingPlayer}");
            _isRaiding = false;
            _isExitRaidModeCalled = true;
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
            if (_raidBridge != null)
            {
                _raidBridge.HideRaid();
            }
            yield return null;
            exitingPlayer?.SetPlayMode(BattlePlayMode.RaidReturn);
            yield return null;
            this.Publish(new UiEvents.ExitRaidNotification(exitingPlayer));
        }
    }
}