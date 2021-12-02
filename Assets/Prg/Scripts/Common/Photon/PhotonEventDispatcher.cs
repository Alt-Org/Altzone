using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Wrapper and helper for PhotonNetwork.RaiseEvent and receiving events on one place.
    /// </summary>
    /// <remarks>
    /// We use lazy initialization and <c>DontDestroyOnLoad</c> to stay alive (for ever).
    /// </remarks>
    public class PhotonEventDispatcher : MonoBehaviour, IOnEventCallback
    {
        public const int eventCodeBase = 10;
        private const int eventCodeCount = 10;
        private const int eventCodeMax = eventCodeBase + eventCodeCount - 1;

        /// <summary>
        /// Photon event listeners for event codes between <c>eventCodeBase</c> and <c>eventCodeMax</c> inclusive;
        /// </summary>
        private readonly Action<EventData>[] listeners = new Action<EventData>[eventCodeCount];

        private readonly RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        public static PhotonEventDispatcher Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<PhotonEventDispatcher>();
                if (_Instance == null)
                {
                    UnityExtensions.CreateGameObjectAndComponent<PhotonEventDispatcher>(nameof(PhotonEventDispatcher), false);
                }
            }
            return _Instance;
        }

        private static PhotonEventDispatcher _Instance;

        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
            // https://doc.photonengine.com/en-us/pun/v2/gameplay/optimization
            // Reuse EventData to decrease garbage collection but EventData will be overwritten for every event!
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            if (_Instance == this)
            {
                _Instance = null;
            }
        }

        public void registerEventListener(byte eventCode, Action<EventData> callback)
        {
            if (eventCode < eventCodeBase || eventCode > eventCodeMax)
            {
                throw new UnityException($"invalid event code {eventCode}, valid range is [{eventCodeBase}-{eventCodeMax}]");
            }
            var index = eventCode - eventCodeBase;
            if (listeners[index] == null)
            {
                listeners[index] = callback;
            }
            else
            {
                listeners[index] += callback;
            }
        }

        public void RaiseEvent(byte eventCode, object data)
        {
            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        void IOnEventCallback.OnEvent(EventData photonEvent)
        {
            // https://doc.photonengine.com/en-us/pun/current/gameplay/rpcsandraiseevent#raiseevent
            var eventCode = photonEvent.Code;
            if (eventCode < eventCodeBase || eventCode > eventCodeMax)
            {
                return; // internal events or not our
            }
            var index = eventCode - eventCodeBase;
            listeners[index]?.Invoke(photonEvent);
        }
    }
}