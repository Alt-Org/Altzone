using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Test;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Event interface for <c>RaidManagerForBattle</c>.
    /// </summary>
    internal interface IRaidManagerForBattle
    {
        void RaidWallEvent(int teamNumber, IPlayerDriver playerDriver);
    }

    /// <summary>
    /// <c>RaidManagerForBattle</c> manages Raid gameplay state during bBattle gameplay.
    /// </summary>
    internal class RaidManagerForBattle : MonoBehaviour, IRaidManagerForBattle, IBattleEvent
    {
        private enum RaidOperation
        {
            Start = 1,
            Continue = 2,
            Stop = 3
        }

        private const int MsgRaidEvent = PhotonEventDispatcher.EventCodeBase + 1;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;

        private PhotonEventDispatcher _photonEventDispatcher;

        private RaidBridge _raidBridge;
        private IRaidEvent _externalRaidEvent;
        private IRaidEvent _localRaidEvent;

        private readonly short[] _messageBuffer = { MsgRaidEvent, 0x0, 0x0, 0x0 };

        private void Awake()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            var isDisableRaid = features._isDisableRaid;
            if (isDisableRaid)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            Debug.Log($"");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgRaidEvent, data => { OnRaidEvent((short[])data.CustomData); });
        }

        private IEnumerator Start()
        {
            var failureTime = Time.time + 2f;
            yield return new WaitUntil(() => (_raidBridge ??= FindObjectOfType<RaidBridge>()) != null || Time.time > failureTime);
            _raidBridge.SetBattleEventHandler(this);
            _externalRaidEvent = _raidBridge;
            yield return new WaitUntil(() => (_localRaidEvent ??= FindObjectOfType<LocalRaidEventTest>()) != null || Time.time > failureTime);
        }

        private void OnRaidEvent(RaidOperation operation, int teamNumber, int actorNumber)
        {
            var player = Context.PlayerManager.GetPlayerByActorNumber(actorNumber);
            Debug.Log($"operation {operation} team {teamNumber} actor {actorNumber} player {player}");
            // Duplicate events to both local (Battle) and external (Raid) event handlers. 
            switch (operation)
            {
                case RaidOperation.Start:
                    _localRaidEvent.RaidStart(teamNumber, player);
                    _externalRaidEvent.RaidStart(teamNumber, player);
                    return;
                case RaidOperation.Continue:
                    _localRaidEvent.RaidBonus(teamNumber, player);
                    _externalRaidEvent.RaidBonus(teamNumber, player);
                    return;
                case RaidOperation.Stop:
                    _localRaidEvent.RaidStop(teamNumber, player);
                    _externalRaidEvent.RaidStop(teamNumber, player);
                    return;
                default:
                    throw new UnityException($"invalid RaidOperation {operation}");
            }
        }

        #region IRaidManagerForBattle

        /// <summary>
        /// Manages internal <c>Raid</c> state for <c>Battle</c>.
        /// </summary>
        void IRaidManagerForBattle.RaidWallEvent(int teamNumber, IPlayerDriver playerDriver)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            var actorNumber = playerDriver?.ActorNumber ?? 0;
            Debug.Log($"team {teamNumber} <- {_teamNumber} actor {actorNumber} <- {_actorNumber} " +
                      $"player {playerDriver} isRaiding {_isRaiding}");
            if (!_isRaiding)
            {
                _isRaiding = true;
                _teamNumber = teamNumber;
                _actorNumber = actorNumber;
                SendRaidEvent(RaidOperation.Start, _teamNumber, _actorNumber);
                return;
            }
            if (_teamNumber == teamNumber)
            {
                _actorNumber = actorNumber;
                SendRaidEvent(RaidOperation.Continue, _teamNumber, _actorNumber);
                return;
            }
            // Close Raid can be called from both "sides"!
            ((IBattleEvent)this).PlayerClosedRaid();
      }

        #endregion

        #region IBattleEvent

        void IBattleEvent.PlayerClosedRaid()
        {
            // Anybody can call this - not only master client!
            _isRaiding = false;
            SendRaidEvent(RaidOperation.Stop, _teamNumber, _actorNumber);
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
        }

        #endregion

        #region Photon Events

        private void OnRaidEvent(short[] payload)
        {
            Assert.AreEqual(4, payload.Length, "Invalid message length");
            Assert.AreEqual((short)MsgRaidEvent, payload[0], "Invalid message id");
            var index = 0;
            var operation = payload[++index];
            var teamNumber = payload[++index];
            var actorNumber = payload[++index];
            OnRaidEvent((RaidOperation)operation, teamNumber, actorNumber);
        }

        private void SendRaidEvent(RaidOperation operation, int teamNumber, int actorNumber = 0)
        {
            Debug.Log($"operation {operation} team {teamNumber} actor {actorNumber}");
            if (actorNumber != 0)
            {
                Assert.IsTrue(actorNumber > short.MinValue && actorNumber < short.MaxValue,
                    "actorNumber > short.MinValue && actorNumber < short.MaxValue");
            }
            var index = 0;
            _messageBuffer[++index] = (short)operation;
            _messageBuffer[++index] = (short)teamNumber;
            _messageBuffer[++index] = (short)actorNumber;
            _photonEventDispatcher.RaiseEvent(MsgRaidEvent, _messageBuffer);
        }

        #endregion
    }
}