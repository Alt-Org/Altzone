using Altzone.Scripts.Battle;
using Battle.Scripts.Test;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    internal enum RaidOperation
    {
        Start = 1,
        Bonus = 2,
        Exit = 3,
    }

    internal interface IRaidManager : IRaidBridge
    {
        bool IsRaiding { get; }

        int ActorNumber { get; }

        int TeamNumber { get; }
    }

    internal class RaidManagerForBattle : MonoBehaviour, IRaidManager
    {
        private const int MsgRaidEvent = PhotonEventDispatcher.EventCodeBase + 1;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private int _teamNumber;

        private PhotonEventDispatcher _photonEventDispatcher;
        private IRaidBridge _raidBridge;

        private readonly short[] _messageBuffer = { MsgRaidEvent, 0x0, 0x0 };

        private void OnEnable()
        {
            Debug.Log($"");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgRaidEvent, data => { OnRaidEvent((short[])data.CustomData); });

            _raidBridge = FindObjectOfType<RaidBridgeTest>();
            Assert.IsNotNull(_raidBridge, "_raidBridge != null");
        }

        private void OnRaidEvent(RaidOperation operation, int actorNumber)
        {
            switch (operation)
            {
                case RaidOperation.Start:
                    OnShowRaid(actorNumber);
                    return;
                case RaidOperation.Bonus:
                    OnAddRaidBonus();
                    return;
                case RaidOperation.Exit:
                    OnHideRaid();
                    return;
                default:
                    throw new UnityException($"invalid operation {operation}");
            }
        }

        #region Local Battle to Raid integration implementation

        private void OnShowRaid(int actorNumber)
        {
            var player = Context.PlayerManager.GetPlayerByActorNumber(actorNumber);
            _isRaiding = true;
            _actorNumber = player?.ActorNumber ?? actorNumber;
            _teamNumber = player?.TeamNumber ?? PhotonBattle.NoTeamValue;
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} player {player}");
            _raidBridge.ShowRaid(player);
        }

        private void OnAddRaidBonus()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            _raidBridge.AddRaidBonus();
        }

        private void OnHideRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            _isRaiding = false;
            _actorNumber = 0;
            _teamNumber = PhotonBattle.NoTeamValue;
            _raidBridge.HideRaid();
        }

        #endregion

        #region IRaidManager (extended IRaidBridge)

        public bool IsRaiding => _isRaiding;

        public int ActorNumber => _actorNumber;

        public int TeamNumber => _teamNumber;

        void IRaidBridge.ShowRaid(IPlayerInfo playerInfo)
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding} playerInfo {playerInfo}");
            SendRaidEvent(RaidOperation.Start, playerInfo.ActorNumber);
        }

        void IRaidBridge.AddRaidBonus()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            SendRaidEvent(RaidOperation.Bonus);
        }

        void IRaidBridge.HideRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            SendRaidEvent(RaidOperation.Exit);
        }

        #endregion

        #region Photon Events

        private void OnRaidEvent(short[] payload)
        {
            Assert.AreEqual(3, payload.Length, "Invalid message length");
            Assert.AreEqual((short)MsgRaidEvent, payload[0], "Invalid message id");
            var operation = payload[1];
            var actorNumber = payload[2];
            OnRaidEvent((RaidOperation)operation, actorNumber);
        }

        private void SendRaidEvent(RaidOperation operation, int actorNumber = 0)
        {
            if (actorNumber != 0)
            {
                Assert.IsTrue(actorNumber > short.MinValue && actorNumber < short.MaxValue,
                    "actorNumber > short.MinValue && actorNumber < short.MaxValue");
            }
            _messageBuffer[1] = (short)operation;
            _messageBuffer[2] = (short)actorNumber;
            _photonEventDispatcher.RaiseEvent(MsgRaidEvent, _messageBuffer);
        }

        #endregion
    }
}