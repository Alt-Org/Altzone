using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Photon
{
    /// <summary>
    /// Activates networked components when all participants have been created, one for each player.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    internal class NetworkSync : MonoBehaviour
    {
        private const int MsgNetworkCreated = PhotonEventDispatcher.EventCodeBase + 1;
        private const int MsgNetworkReady = PhotonEventDispatcher.EventCodeBase + 2;

        [Header("Settings"), Min(1), SerializeField] private int _componentTypeId;
        [SerializeField, Min(1)] private float _failSafeActivationDelay;
        [SerializeField] private MonoBehaviour[] _componentsToActivate;

        [Header("Live Data"), SerializeField] private int _requiredComponentCount;
        [SerializeField] private int _currentComponentCount;
        [SerializeField] private int _ownerActorNr;
        [SerializeField] private float _failSafeActivationTime;
        [SerializeField] private bool _isMsgNetworkReadySent;

        private PhotonView _photonView;
        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            Assert.IsTrue(_componentsToActivate.Length > 0, "No components to activate");
            foreach (var component in _componentsToActivate)
            {
                Assert.IsFalse(component.enabled, $"Component is active: {component.GetType().Name}");
            }
            if (PhotonNetwork.OfflineMode)
            {
                ActivateAllComponents();
                enabled = false;
                return;
            }
            _photonView = PhotonView.Get(this);
            _requiredComponentCount = PhotonBattle.CountRealPlayers();
            if (!_photonView.IsRoomView)
            {
                // This is Peer-To-Peer network: total is 1, 4, 9 or 16 instances to confirm its creation
                _requiredComponentCount *= _requiredComponentCount;
            }
            _currentComponentCount = 0;
            _ownerActorNr = _photonView.OwnerActorNr;
            _failSafeActivationTime = float.MaxValue;
            _isMsgNetworkReadySent = false;
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgNetworkCreated, data => { OnMsgNetworkCreated(data.CustomData); });
            _photonEventDispatcher.RegisterEventListener(MsgNetworkReady, data => { OnMsgNetworkReady(data.CustomData); });
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable type {_componentTypeId} owner {_ownerActorNr} {name} required {_requiredComponentCount}");
            SendMsgNetworkCreated();
            _failSafeActivationTime = Time.time + _failSafeActivationDelay;
        }

        private void OnDisable()
        {
            Debug.Log(
                $"OnDisable type {_componentTypeId} owner {_ownerActorNr} {name} required {_requiredComponentCount} current {_currentComponentCount}");
        }

        private void Update()
        {
            if (_isMsgNetworkReadySent)
            {
                return;
            }
            if (Time.time < _failSafeActivationTime)
            {
                return;
            }
            SendMsgNetworkReady();
        }

        #region Photon Events

        private void SendMsgNetworkCreated()
        {
            var payload = new object[] { (byte)MsgNetworkCreated, _componentTypeId, _ownerActorNr };
            _photonEventDispatcher.RaiseEvent(MsgNetworkCreated, payload);
        }

        private void OnMsgNetworkCreated(object data)
        {
            _failSafeActivationTime = Time.time + _failSafeActivationDelay;
            var payload = (object[])data;
            Assert.AreEqual(3, payload.Length, "Invalid message length");
            Assert.AreEqual((byte)MsgNetworkCreated, (byte)payload[0], "Invalid message id");
            var componentTypeId = (int)payload[1];
            if (componentTypeId != _componentTypeId)
            {
                return;
            }
            var ownerActorNr = (int)payload[2];
            _currentComponentCount += 1;
            Debug.Log(
                $"OnNetworkCreated type {_componentTypeId} owner {ownerActorNr} {name} required {_requiredComponentCount} current {_currentComponentCount} master {PhotonNetwork.IsMasterClient}");
            Assert.IsTrue(_currentComponentCount <= _requiredComponentCount);
            if (_currentComponentCount < _requiredComponentCount)
            {
                return;
            }
            SendMsgNetworkReady();
        }

        private void SendMsgNetworkReady()
        {
            if (_isMsgNetworkReadySent)
            {
                return;
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (!(_photonView.IsMine || _photonView.IsRoomView))
            {
                return;
            }
            _isMsgNetworkReadySent = true;
            var payload = new object[] { (byte)MsgNetworkReady, _componentTypeId };
            _photonEventDispatcher.RaiseEvent(MsgNetworkReady, payload);
            _failSafeActivationTime = Time.time + _failSafeActivationDelay;
        }

        private void OnMsgNetworkReady(object data)
        {
            _failSafeActivationTime = Time.time + _failSafeActivationDelay;
            var payload = (object[])data;
            Assert.AreEqual(2, payload.Length, "Invalid message length");
            Assert.AreEqual((byte)MsgNetworkReady, (byte)payload[0], "Invalid message id");
            var componentTypeId = (int)payload[1];
            if (componentTypeId != _componentTypeId)
            {
                return;
            }
            Debug.Log($"OnMsgNetworkReady type {_componentTypeId} {name}");
            ActivateAllComponents();
            enabled = false;
        }

        #endregion

        private void ActivateAllComponents()
        {
            Debug.Log($"ActivateAllComponents {name} components {_componentsToActivate.Length} type {_componentTypeId}");
            StartCoroutine(ActivateComponents());
        }

        private IEnumerator ActivateComponents()
        {
            // Enable one component per frame in array sequence
            for (var i = 0; i < _componentsToActivate.LongLength; i++)
            {
                yield return null;
                _componentsToActivate[i].enabled = true;
            }
            yield return null;
            this.Publish(new RoomManager.ActorReportEvent(_componentTypeId));
        }
    }
}