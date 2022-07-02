using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Attributes;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test
{
    internal class LocalRaidEventTest : MonoBehaviour, IRaidEvent
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private bool _isExitRaidModeCalled;

        private IPlayerManager _playerManager;

        private void Awake()
        {
            ScoreFlashNet.RegisterEventListener();
        }

        private void OnEnable()
        {
            _playerManager = Context.PlayerManager;
        }

        #region IRaidEvent implementation for "local internal" Battle <-> Raid testing

        void IRaidEvent.RaidStart(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            _isRaiding = true;
            _isExitRaidModeCalled = false;
            _teamNumber = teamNumber;
            if (playerInfo == null)
            {
                // During testing it is possible that all teams do not have all (if any) players!
                _actorNumber = 0;
                return;
            }
            _actorNumber = playerInfo.ActorNumber;
            var player = _playerManager.GetPlayerByActorNumber(playerInfo.ActorNumber);
            StartCoroutine(OnStartRaidMode(player));
        }

        void IRaidEvent.RaidBonus(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            Assert.IsTrue(_isRaiding, "_isRaiding");
            Assert.AreEqual(_teamNumber, teamNumber);
            var info = $"{_teamNumber}";
            ScoreFlashNet.Push($"RAID BONUS {info}");
        }

        void IRaidEvent.RaidStop(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            Assert.IsTrue(_isRaiding, "_isRaiding");
            Assert.IsFalse(_isExitRaidModeCalled, "_isExitRaidModeCalled");
            StartCoroutine(OnExitRaidMode());
        }

        #endregion

        private IEnumerator OnStartRaidMode(IPlayerDriver player)
        {
            Debug.Log($"team {_teamNumber} actor {_actorNumber} player {player}");
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            yield return null;
            var info = $"{_teamNumber}:{player.NickName.ToUpper()}";
            ScoreFlashNet.Push($"RAID START {info}");
        }

        private IEnumerator OnExitRaidMode()
        {
            var exitingPlayer = _playerManager.GetPlayerByActorNumber(_actorNumber);
            Debug.Log($"team {_teamNumber} actor {_actorNumber} isRaiding {_isRaiding} player {exitingPlayer}");
            var info = $"{_teamNumber}";
            _isRaiding = false;
            _isExitRaidModeCalled = true;
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
            _playerManager.ForEach(player =>
            {
                if (player.BattlePlayMode == BattlePlayMode.RaidGhosted)
                {
                    player.SetPlayMode(BattlePlayMode.RaidReturn);
                }
            });
            yield return null;
            this.Publish(new UiEvents.ExitRaidNotification(exitingPlayer));
            yield return null;
            ScoreFlashNet.Push($"RAID EXIT {info}");
        }
    }
}