using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Battle
{
    /// <summary>
    /// Bridge between <c>Battle</c> and <c>Raid</c> gameplay modes.
    /// </summary>
    /// <remarks>
    /// Because Battle and Raid are in separate assemblies due to our selected architecture of separation of concerns
    /// and can not access each other we need this bridge that provides two way access
    /// to required functionality on both gameplay modes and implementations. 
    /// </remarks>
    public class RaidBridge : MonoBehaviour, IRaidEvent, IBattleEvent
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _hasRaid;
        [SerializeField, ReadOnly] private bool _hasBattle;

        private IRaidEvent _raidEventHandler;
        private IBattleEvent _battleEventHandler;

        public void SetRaidEventHandler(IRaidEvent eventHandler)
        {
            _raidEventHandler = eventHandler;
            _hasRaid = _raidEventHandler != null;
        }

        public void SetBattleEventHandler(IBattleEvent eventHandler)
        {
            _battleEventHandler = eventHandler;
            _hasBattle = _battleEventHandler != null;
        }

        void IRaidEvent.RaidStart(int teamNumber, IPlayerInfo playerInfo)
        {
            Assert.IsNotNull(_raidEventHandler, "_raidEventHandler != null");
            _raidEventHandler.RaidStart(teamNumber, playerInfo);
        }

        void IRaidEvent.RaidBonus(int teamNumber, IPlayerInfo playerInfo)
        {
            Assert.IsNotNull(_raidEventHandler, "_raidEventHandler != null");
            _raidEventHandler.RaidBonus(teamNumber, playerInfo);
        }

        void IRaidEvent.RaidStop(int teamNumber, IPlayerInfo playerInfo)
        {
            Assert.IsNotNull(_raidEventHandler, "_raidEventHandler != null");
            _raidEventHandler.RaidStop(teamNumber, playerInfo);
        }

        void IBattleEvent.PlayerClosedRaid()
        {
            Assert.IsNotNull(_battleEventHandler, "_battleEventHandler != null");
            _battleEventHandler.PlayerClosedRaid();
        }
    }
}