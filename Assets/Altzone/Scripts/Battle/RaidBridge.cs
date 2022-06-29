using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Battle
{
    public interface IRaidBridge
    {
        void ShowRaid(int actorNumber);
        void AddRaidBonus(int actorNumber);
        void HideRaid();
    }

    public interface IBattleBridge
    {
        void ClosedRaid(int actorNumber);
    }

    /// <summary>
    /// Bridge between <c>Battle</c> and <c>Raid</c> gameplay modes.
    /// </summary>
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

        public void AddRaidBonus(int actorNumber)
        {
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
            _raidBridge.AddRaidBonus(actorNumber);
        }

        public void HideRaid()
        {
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
            _raidBridge.HideRaid();
        }

        public void ClosedRaid(int actorNumber)
        {
            Assert.IsNotNull(_battleBridge, "_battleBridge != null");
            _battleBridge.ClosedRaid(actorNumber);
        }
    }
}