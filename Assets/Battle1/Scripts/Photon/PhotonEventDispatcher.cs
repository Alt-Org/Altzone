#if false
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using Battle1.PhotonRealtime.Code;
using UnityEngine;
using UnityEngine.Assertions;
using IOnEventCallback = Battle1.PhotonRealtime.Code.IOnEventCallback;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using ReceiverGroup = Battle1.PhotonRealtime.Code.ReceiverGroup;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Wrapper and helper for PhotonNetwork.RaiseEvent for sending and receiving events on one place.
    /// </summary>
    /// <remarks>
    /// Note 1. We use lazy initialization and can not survive level load.<br />
    /// Note 2. By default we use PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance set to <b>true</b>.
    /// Call DisableReuseEventInstance() if you do not want this behaviour.
    /// </remarks>
    public class PhotonEventDispatcher : MonoBehaviour, IOnEventCallback
    {
        public const int EventCodeBase = 10;
        private const int EventCodeCount = 10;
        private const int EventCodeMax = EventCodeBase + EventCodeCount - 1;

        /// <summary>
        /// Photon event listeners for event codes between <c>eventCodeBase</c> and <c>eventCodeMax</c> inclusive;
        /// </summary>
        private readonly Action<EventData>[] _listeners = new Action<EventData>[EventCodeCount];

        private readonly RaiseEventOptions _raiseEventOptions = new()
        {
            Receivers = ReceiverGroup.All
        };

        public static PhotonEventDispatcher Get()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PhotonEventDispatcher>();
                if (_instance == null)
                {
                    UnitySingleton.CreateGameObjectAndComponent<PhotonEventDispatcher>();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Sets PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance to <b>false</b>.
        /// </summary>
        public static void DisableReuseEventInstance()
        {
            _instance._isReuseEventInstance = false;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = false;
        }

        /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }*/

        private static PhotonEventDispatcher _instance;

        private bool _isReuseEventInstance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            // https://doc.photonengine.com/en-us/pun/v2/gameplay/optimization
            // Reuse EventData to decrease garbage collection but EventData will be overwritten for every event!
            _isReuseEventInstance = true;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnEnable()
        {
            Assert.IsTrue(FindObjectsOfType<PhotonEventDispatcher>().Length == 1,
                "FindObjectsOfType<PhotonEventDispatcher>().Length == 1");
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void RegisterEventListener(byte eventCode, Action<EventData> callback)
        {
            if (eventCode < EventCodeBase || eventCode > EventCodeMax)
            {
                throw new UnityException($"invalid event code {eventCode}, valid range is [{EventCodeBase}-{EventCodeMax}]");
            }
            var index = eventCode - EventCodeBase;
            if (_listeners[index] == null)
            {
                _listeners[index] = callback;
            }
            else
            {
                _listeners[index] += callback;
            }
        }

        public void RaiseEvent(byte eventCode, object data)
        {
            PhotonNetwork.RaiseEvent(eventCode, data, _raiseEventOptions, SendOptions.SendReliable);
        }

        void IOnEventCallback.OnEvent(EventData photonEvent)
        {
            Assert.AreEqual(_isReuseEventInstance, PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance,
                "Internal and external Photon Reuse EventData state mismatch");
            // https://doc.photonengine.com/en-us/pun/current/gameplay/rpcsandraiseevent#raiseevent
            var eventCode = photonEvent.Code;
            if (eventCode < EventCodeBase || eventCode > EventCodeMax)
            {
                return; // internal events or not our
            }
            var index = eventCode - EventCodeBase;
            _listeners[index]?.Invoke(photonEvent);
        }
    }
}
#endif
