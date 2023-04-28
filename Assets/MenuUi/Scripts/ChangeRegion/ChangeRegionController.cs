using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace MenuUi.Scripts.ChangeRegion
{
    /// <summary>
    /// <c>Controller</c> for ChangeRegion UI to show all Photon enabled regions and select one of them.
    /// </summary>
    /// <remarks>
    /// We need to disconnect and then connect in order to trigger Photon <c>OnRegionListReceived</c> callback
    /// which is needed to get region list for pinging them. 
    /// </remarks>
    public class ChangeRegionController : MonoBehaviour
    {
        private const float PingRegionsInterval = 3.0f;

        [SerializeField] private ChangeRegionView _view;
        [SerializeField] private PhotonRegionList _photonRegionList;

        private void OnEnable()
        {
            Debug.Log($"{name}", gameObject);
            _view.ResetView();
            _photonRegionList = PhotonRegionList.GetOrCreate();
            StartCoroutine(RegionListUpdater());
        }

        private void OnDisable()
        {
            if (_photonRegionList != null)
            {
                _photonRegionList.enabled = false;
            }
            StopAllCoroutines();
        }

        private IEnumerator RegionListUpdater()
        {
            yield return null;
            Debug.Log($"{name} disconnect first");
            for (; enabled;)
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonLobby.LeaveLobby();
                }
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                }
                if (PhotonWrapper.IsConnectedToMasterServer)
                {
                    // IsConnectedToMasterServer -> Disconnected
                    PhotonLobby.Disconnect();
                }
                if (PhotonWrapper.CanConnect)
                {
                    // Ready to connect for region list.
                    break;
                }
                yield return null;
            }
            Debug.Log($"{name} then connect");
            for (; enabled;)
            {
                if (PhotonWrapper.CanConnect)
                {
                    // Connect -> ConnectedToMasterServer
                    PhotonLobby.Connect(PhotonNetwork.NickName);
                }
                if (PhotonWrapper.IsConnectedToMasterServer)
                {
                    // Ready to start pinging game servers.
                    break;
                }
                yield return null;
            }
            // At this point we should have a region list (without Ping)
            Debug.Log($"{name} done");
            var regionList = _photonRegionList.EnabledRegions.ToList().OrderBy(x => x.Region);
            Debug.Log($"regionList {string.Join(',', regionList)}");

            // Start pinging regions and updating UI.
            _photonRegionList.PingRegions(OnPhotonRegionListUpdate, PingRegionsInterval);
        }

        private void OnPhotonRegionListUpdate(ReadOnlyCollection<PhotonRegionList.PhotonRegion> regions)
        {
            var regionList = regions.ToList().OrderBy(x => x.Ping);
            Debug.Log($"regionList {string.Join(',', regionList)}");
        }
    }
}