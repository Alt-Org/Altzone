using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUI.Scripts.Lobby.InLobby
{
    public class InLobbyController : MonoBehaviour
    {
        [SerializeField] private InLobbyView _view;

        private string _currentRegion;

        private void Awake()
        {
            //_view.CharacterButtonOnClick = CharacterButtonOnClick;
            //_view.RoomButtonOnClick = RoomButtonOnClick;
            //_view.RaidButtonOnClick = RaidButtonOnClick;
            //_view.QuickGameButtonOnClick = QuickGameButtonOnClick;
        }

        public void OnEnable()
        {
            //base.OnEnable();

            //var cloudRegion = PhotonNetwork.NetworkingClient?.CloudRegion;
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            //Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} CloudRegion={cloudRegion} PhotonRegion={photonRegion}");
            /*if (PhotonWrapper.IsConnectedToMasterServer && photonRegion != cloudRegion)
            {
                // We need to disconnect from current region because it is not the same as in player settings.
                PhotonLobby.Disconnect();
            }*/
            _view.Reset();
            UpdateTitle();
            _view.LobbyText = string.Empty;
            StartCoroutine(StartLobby(playerSettings.PlayerGuid, playerSettings.PhotonRegion));
        }

        public void OnDisable()
        {
            CloseWindow();
        }

        private void UpdateTitle()
        {
            // Save region for later use because getting it is not cheap (b ut not very expensive either). 
            _currentRegion = PhotonRealtimeClient.CloudRegion != null ? PhotonRealtimeClient.CloudRegion : "";
            _view.TitleText = $"{Application.productName} {PhotonRealtimeClient.GameVersion}";
        }

        private IEnumerator StartLobby(string playerGuid, string photonRegion)
        {
            var networkClientState = PhotonRealtimeClient.NetworkClientState;
            Debug.Log($"{networkClientState}");
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonRealtimeClient.Client.InLobby)
            {
                if (networkClientState != PhotonRealtimeClient.NetworkClientState)
                {
                    // Even with delay we must reduce NetworkClientState logging to only when it changes to avoid flooding (on slower connections).
                    networkClientState = PhotonRealtimeClient.NetworkClientState;
                    Debug.Log($"{networkClientState}");
                }
                if (PhotonRealtimeClient.Client.InRoom)
                {
                    PhotonRealtimeClient.LeaveRoom();
                }
                else if (PhotonRealtimeClient.CanConnect)
                {
                    var store = Storefront.Get();
                    PlayerData playerData = null;
                    store.GetPlayerData(playerGuid, p => playerData = p);
                    yield return new WaitUntil(() => playerData != null);
                    PhotonRealtimeClient.Connect(playerData.Name, photonRegion);
                }
                else if (PhotonRealtimeClient.CanJoinLobby)
                {
                    PhotonRealtimeClient.JoinLobby(null);
                }
                yield return delay;
            }
            UpdateTitle();
            _view.EnableButtons();
        }

        private void Update()
        {
            if (!PhotonRealtimeClient.Client.InLobby)
            {
                _view.LobbyText = "Wait";
                return;
            }
            UpdateTitle();
            _view.EnableButtons();
            var playerCount = PhotonRealtimeClient.CountOfPlayers;
            _view.LobbyText = $"Alue: {_currentRegion} : {PhotonRealtimeClient.GetPing()} ms";
            _view.PlayerCountText = $"Pelaajia online: {playerCount}";
        }

        /*public void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");

            if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
            {
                OnEnable();
            }
        }*/

        public void ToggleWindow()
        {
            if(transform.GetChild(0).gameObject.activeSelf) CloseWindow();
            else transform.GetChild(0).gameObject.SetActive(true);
        }

        public void CloseWindow()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.NetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.NetworkClientState}");
        }

        private void RaidButtonOnClick()
        {
            SceneManager.LoadScene("te-test-raid-demo");
        }

        private void QuickGameButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.NetworkClientState}");
        }
    }
}
