using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Battle.Scripts.Test
{
    internal class BattleBridgeTest : MonoBehaviour, IBattleBridge, IRaidBridge
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

        #region IBattleBridge implementation for external Raid testing

        public void PlayerClosedRaid()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber} isExitRaidModeCalled {_isExitRaidModeCalled}");
            if (!_isExitRaidModeCalled)
            {
                StartCoroutine(OnExitRaidMode());
            }
        }

        #endregion

        #region IRaidBridge implementation for internal Battle testing

        void IRaidBridge.ShowRaid(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"teamNumber {teamNumber} playerInfo {playerInfo} isRaiding {_isRaiding}");
            if (_isRaiding)
            {
                if (teamNumber == _teamNumber)
                {
                    _actorNumber = playerInfo?.ActorNumber ?? 0;
                    // AddRaidBonus -> UI event
                    return;
                }
                // HideRaid -> UI event
                if (!_isExitRaidModeCalled)
                {
                    StartCoroutine(OnExitRaidMode());
                }
                return;
            }
            _isRaiding = true;
            _teamNumber = teamNumber;
            _actorNumber = playerInfo.ActorNumber;
            var player = _playerManager.GetPlayerByActorNumber(playerInfo.ActorNumber);
            StartCoroutine(OnStartRaidMode(player));
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
                ((IRaidBridge)_raidBridge).ShowRaid(_teamNumber, player);
            }
        }

        private IEnumerator OnExitRaidMode()
        {
            var exitingPlayer = _playerManager.GetPlayerByActorNumber(_actorNumber);
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber} isRaiding {_isRaiding} player {exitingPlayer}");
            _isRaiding = false;
            _isExitRaidModeCalled = true;
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
            yield return null;
            _playerManager.ForEach(player =>
            {
                if (player.BattlePlayMode == BattlePlayMode.RaidGhosted)
                {
                    player.SetPlayMode(BattlePlayMode.RaidReturn);
                }
            });
            yield return null;
            this.Publish(new UiEvents.ExitRaidNotification(exitingPlayer));
        }
    }
}