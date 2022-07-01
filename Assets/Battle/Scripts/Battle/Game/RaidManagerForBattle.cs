using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Test;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Game
{
    internal class RaidManagerForBattle : MonoBehaviour, IRaidBridge
    {
        private const int MsgRaidEvent = PhotonEventDispatcher.EventCodeBase + 1;

        private PhotonEventDispatcher _photonEventDispatcher;
        private IRaidBridge _raidBridge;
        private IRaidBridge _localRaidBridge;

        private readonly short[] _messageBuffer = { MsgRaidEvent, 0x0, 0x0 };

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

            _localRaidBridge = FindObjectOfType<BattleBridgeTest>();
        }

        private void OnRaidEvent(int teamNumber, int actorNumber)
        {
            var player = Context.PlayerManager.GetPlayerByActorNumber(actorNumber);
            Debug.Log($"teamNumber {teamNumber} player {player}");
            _raidBridge.ShowRaid(teamNumber, player);
            // Duplicate events to local Battle IRaidBridge which manages player state during "raid event"
            if (player != null && player.IsLocal)
            {
                _localRaidBridge.ShowRaid(teamNumber, player);
            }
        }

        #region IRaidBridge

        void IRaidBridge.ShowRaid(int teamNumber, IPlayerInfo playerInfo)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            var actorNumber = playerInfo?.ActorNumber ?? 0;
            Debug.Log($"teamNumber {teamNumber} actorNumber {actorNumber} playerInfo {playerInfo}");
            SendRaidEvent(teamNumber, actorNumber);
        }

        #endregion

        #region Photon Events

        private void OnRaidEvent(short[] payload)
        {
            Assert.AreEqual(3, payload.Length, "Invalid message length");
            Assert.AreEqual((short)MsgRaidEvent, payload[0], "Invalid message id");
            var teamNumber = payload[1];
            var actorNumber = payload[2];
            OnRaidEvent(teamNumber, actorNumber);
        }

        private void SendRaidEvent(int teamNumber, int actorNumber = 0)
        {
            if (actorNumber != 0)
            {
                Assert.IsTrue(actorNumber > short.MinValue && actorNumber < short.MaxValue,
                    "actorNumber > short.MinValue && actorNumber < short.MaxValue");
            }
            _messageBuffer[1] = (short)teamNumber;
            _messageBuffer[2] = (short)actorNumber;
            _photonEventDispatcher.RaiseEvent(MsgRaidEvent, _messageBuffer);
        }

        #endregion
    }
}