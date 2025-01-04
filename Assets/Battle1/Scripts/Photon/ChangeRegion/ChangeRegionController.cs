using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Photon;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

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

        [Header("Settings"), SerializeField] private ChangeRegionView _view;

        [Header("Live Data"), SerializeField] private PhotonRegionList _photonRegionList;
        [SerializeField] private string _currentPhotonRegion;

        private static string ConvertRegionForUI(string photonRegion) => string.IsNullOrEmpty(photonRegion)
            ? ChangeRegionView.DefaultRegionButtonCode
            : photonRegion;

        private static string ConvertRegionForSettings(string photonRegion) =>
            photonRegion == ChangeRegionView.DefaultRegionButtonCode
                ? string.Empty
                : photonRegion;

        private void Start()
        {
            // Set just once!
            _view.SetRegionChangedCallback(OnRegionSelected);
        }

        private void OnEnable()
        {
            Debug.Log($"{name}", gameObject);
            _view.ResetView();
            _view.TitleText = "Loading Regions";
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            _currentPhotonRegion = ConvertRegionForUI(playerSettings.PhotonRegion);
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

        private void UpdateTitle()
        {
            var selectedRegion = _currentPhotonRegion;
            _view.TitleText = $"Server Region '{selectedRegion}'";
        }

        private void OnRegionSelected(string regionCode)
        {
            Debug.Log($"update {_currentPhotonRegion} <- {regionCode}");
            _currentPhotonRegion = regionCode;
            UpdateTitle();

            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            playerSettings.PhotonRegion = ConvertRegionForSettings(regionCode);
        }

        private IEnumerator RegionListUpdater()
        {
            yield return null;
            Debug.Log($"{name} disconnect first");
            for (; enabled;)
            {
                /*if (PhotonNetwork.InLobby)
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
                }*/
                yield return null;
            }
            Debug.Log($"{name} then connect");
            for (; enabled;)
            {
               /* if (PhotonWrapper.CanConnect)
                {
                    // Connect -> ConnectedToMasterServer
                    PhotonLobby.Connect(PhotonNetwork.NickName);
                }
                if (PhotonWrapper.IsConnectedToMasterServer)
                {
                    // Ready to start pinging game servers.
                    break;
                }*/
                yield return null;
            }
            // At this point we should have a region list (without Ping values)
            Debug.Log($"{name} done regions: {_photonRegionList.EnabledRegionsCount}");

            if (_photonRegionList.EnabledRegionsCount == 0)
            {
                _view.TitleText = "<b>Server Region</b> is not <b>Selectable</b> in this game";
                yield break;
            }
            UpdateTitle();
            OnPhotonRegionListUpdate(_photonRegionList.EnabledRegions);

            // Start pinging regions and updating UI.
            _photonRegionList.PingRegions(OnPhotonRegionListUpdate, PingRegionsInterval);
        }

        private void OnPhotonRegionListUpdate(IReadOnlyList<PhotonRegionList.PhotonRegion> regions)
        {
            var regionList = regions.ToList().OrderBy(x => x.Ping).ToList();
            regionList.Add(new PhotonRegionList.PhotonRegion(ChangeRegionView.DefaultRegionButtonCode, 0));
            _view.UpdateRegionList(regionList, _currentPhotonRegion);
        }
    }
}
