using System;
using Prg.Scripts.Common.Photon;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players2
{
    public class PhotonEventHelper
    {
        private readonly PhotonEventDispatcher _photonEventDispatcher;
        private readonly byte _playerId;

        public PhotonEventHelper(PhotonEventDispatcher photonEventDispatcher, byte playerId)
        {
            Assert.IsTrue(playerId != 0, "playerId != 0");
            _photonEventDispatcher = photonEventDispatcher;
            _playerId = playerId;
        }

        public void RegisterEvent(byte msgId, Action<byte[]> onMsgReceived)
        {
            _photonEventDispatcher.RegisterEventListener(msgId, data =>
            {
                var payload = (byte[])data.CustomData;
                Assert.IsTrue(payload.Length >= 1, "payload.Length >= 1");
                Assert.IsTrue(payload[0] != 0, "payload[0] != 0");
                if (payload[0] == _playerId)
                {
                    onMsgReceived(payload);
                }
            });
        }

        public void SendEvent(byte msgId, byte[] payload)
        {
            Assert.IsTrue(payload.Length >= 1, "payload.Length >= 1");
            payload[0] = _playerId;
            _photonEventDispatcher.RaiseEvent(msgId, payload);
        }
    }
}