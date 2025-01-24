using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
/*using ClientState = Battle1.PhotonRealtime.Code.ClientState;
using DisconnectCause = Battle1.PhotonRealtime.Code.DisconnectCause;
using IConnectionCallbacks = Battle1.PhotonRealtime.Code.IConnectionCallbacks;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Region = Battle1.PhotonRealtime.Code.Region;
using RegionHandler = Battle1.PhotonRealtime.Code.RegionHandler;*/

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Wrapper for Photon <c>RegionHandler</c> to 'ping' those regions that have been enabled in Photon Dashboard.
    /// </summary>
    /// <remarks>
    /// Note that <b>ping</b>'ing happens in <b>background thread</b>!<br />
    /// How To Show A Region List:<br />
    /// https://doc.photonengine.com/realtime/current/connection-and-authentication/regions#how_to_show_a_region_list
    /// </remarks>
    public class PhotonRegionList : MonoBehaviour
    {
        public static PhotonRegionList GetOrCreate()
        {
            var regionList = FindObjectOfType<PhotonRegionList>();
            if (regionList == null)
            {
                regionList = UnitySingleton.CreateGameObjectAndComponent<PhotonRegionList>();
            }
            if (!regionList.enabled)
            {
                regionList.enabled = true;
            }
            return regionList;
        }

        [Header("Debug"), SerializeField] private string _currentPingResult;

        private List<PhotonRegion> _enabledRegions = new();

        #region Public API

        public PhotonRegion BestRegion { get; private set; }

        public string SummaryToCache { get; private set; }

        public IReadOnlyList<PhotonRegion> EnabledRegions => _enabledRegions.AsReadOnly();

        public int EnabledRegionsCount => _enabledRegions.Count;

        #endregion

        private MyConnectionCallbacks _connectionCallbacks;
        private RegionHandler _curRegionHandler;

        private void Awake()
        {
            Debug.Log($"{name}", gameObject);
            _connectionCallbacks = new MyConnectionCallbacks(this);
        /*    if (PhotonNetwork.NetworkingClient != null)
            {
                PhotonNetwork.AddCallbackTarget(_connectionCallbacks);
            }*/
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            if (_connectionCallbacks == null)
            {
                _connectionCallbacks = new MyConnectionCallbacks(this);
             /*   PhotonNetwork.AddCallbackTarget(_connectionCallbacks);*/
            }
        }

        private void OnDisable()
        {
            Debug.Log($"{name}");
            if (_connectionCallbacks != null)
            {
                /*PhotonNetwork.RemoveCallbackTarget(_connectionCallbacks);*/
                _connectionCallbacks = null;
            }
            StopAllCoroutines();
        }

        public void PingRegions(Action<IReadOnlyList<PhotonRegion>> onPingRegionsReady, float pingRegionsInterval = 0f)
        {
            Debug.Log($"{name} {_curRegionHandler}");
            StartCoroutine(PingMinimumOfRegions(onPingRegionsReady, pingRegionsInterval));
        }

        private IEnumerator PingMinimumOfRegions(Action<IReadOnlyList<PhotonRegion>> onPingRegionsReady, float pingRegionsInterval)
        {
            Assert.IsNotNull(onPingRegionsReady);
            yield return null;
            if (_curRegionHandler == null)
            {
                // Either we have missed OnRegionListReceived callback due to timings how we start Photon connection to master server
                // or Photon Available Regions are not available due to ServerSettings config that prevents their usage.
                onPingRegionsReady(new List<PhotonRegion>().AsReadOnly());
                yield break;
            }
            var pingRegionsDelay = new WaitForSeconds(pingRegionsInterval);
           /* while (enabled && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                // Start full ping operation without 'previousSummary' because we want all regions to be updated continuously.
                var isPingDone = false;
                var isStarted = _curRegionHandler.PingMinimumOfRegions((handler) =>
                    {
                        // Note that this not in UNITY MainThread context and we have to switch back to it in order to update UI etc.
                        UpdateRegionHandler(handler);
                        isPingDone = true;
                    },
                    null);
                Assert.IsTrue(isStarted, "PingMinimumOfRegions failed to start");

                // Wait for new ping data to arrive and update in UNITY MainThread.
                yield return new WaitUntil(() => isPingDone || !enabled || PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer);
                if (isPingDone && enabled && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    // Update public API properties before calling the callback.
                    _currentPingResult = _curRegionHandler.SummaryToCache;
                    SummaryToCache = _currentPingResult;
                    var bestRegion = _curRegionHandler.BestRegion;
                    if (BestRegion == null || BestRegion.Region != bestRegion.Code)
                    {
                        BestRegion = new PhotonRegion(bestRegion);
                    }
                    onPingRegionsReady(EnabledRegions);
                }
                if (pingRegionsInterval == 0f)
                {
                    yield break;
                }
                yield return pingRegionsDelay;
            }*/
        }

        private void UpdateRegionHandler(RegionHandler regionHandler)
        {
            // This can be outside of UNITY MainThread context!
            _curRegionHandler = regionHandler;
            _enabledRegions = new List<PhotonRegion>();
            foreach (var enabledRegion in regionHandler.EnabledRegions)
            {
                _enabledRegions.Add(new PhotonRegion(enabledRegion));
            }
        }

        private void OnRegionListReceived(RegionHandler regionHandler)
        {
            UpdateRegionHandler(regionHandler);
            Debug.Log($"BestRegion={new PhotonRegion(regionHandler.BestRegion)} EnabledRegions={string.Join(',', _enabledRegions)}");
        }

        public class PhotonRegion
        {
            public readonly string Region;
            public readonly int Ping;

            public PhotonRegion(string region, int ping)
            {
                Region = region;
                Ping = ping == int.MaxValue ? -1 : ping;
            }

            public PhotonRegion(Region region) : this(region.Code, region.Ping)
            {
            }

            public override string ToString()
            {
                return $"'{Region}' {Ping} ms";
            }
        }

        /// <summary>
        /// Private inner class to hide Photon IConnectionCallbacks implementation.
        /// </summary>
        private class MyConnectionCallbacks : IConnectionCallbacks
        {
            private readonly PhotonRegionList _photonRegionList;

            public MyConnectionCallbacks(PhotonRegionList photonRegionList)
            {
                _photonRegionList = photonRegionList;
            }

            public void OnRegionListReceived(RegionHandler regionHandler)
            {
                _photonRegionList.OnRegionListReceived(regionHandler);
            }

            public void OnConnected()
            {
            }

            public void OnConnectedToMaster()
            {
            }

            public void OnDisconnected(DisconnectCause cause)
            {
            }

            public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
            {
            }

            public void OnCustomAuthenticationFailed(string debugMessage)
            {
            }
        }
    }
}
