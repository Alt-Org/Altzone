using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Battle
{
    /// <summary>
    /// Raid gameplay mode interface for Battle.
    /// </summary>
    public interface IRaidBridge
    {
        void ShowRaid(int actorNumber);
        void AddRaidBonus();
        void HideRaid();
    }

    /// <summary>
    /// Battle gameplay mode interface for Raid.
    /// </summary>
    public interface IBattleBridge
    {
        void PlayerClosedRaid();
    }

    /// <summary>
    /// Bridge between <c>Battle</c> and <c>Raid</c> gameplay modes.
    /// </summary>
    /// <remarks>
    /// Because Battle and Raid are in separate assemblies due to our selected architecture of separation of concerns
    /// and can not access each other we need this bridge that provides two way access
    /// to required functionality on both gameplay modes and implementations. 
    /// </remarks>
    public class RaidBridge : MonoBehaviour, IRaidBridge, IBattleBridge
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _hasRaid;
        [SerializeField, ReadOnly] private bool _hasBattle;
        
        private IRaidBridge _raidBridge;
        private IBattleBridge _battleBridge;

        public void SetRaidBridge(IRaidBridge bridge)
        {
            _raidBridge = bridge;
            _hasRaid = _raidBridge != null;
        }

        public void SetBattleBridge(IBattleBridge bridge)
        {
            _battleBridge = bridge;
            _hasBattle = _battleBridge != null;
        }

        public void ShowRaid(int actorNumber)
        {
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
            _raidBridge.ShowRaid(actorNumber);
        }

        public void AddRaidBonus()
        {
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
            _raidBridge.AddRaidBonus();
        }

        public void HideRaid()
        {
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
            _raidBridge.HideRaid();
        }

        public void PlayerClosedRaid()
        {
            Assert.IsNotNull(_battleBridge, "_battleBridge != null");
            _battleBridge.PlayerClosedRaid();
        }
    }
}