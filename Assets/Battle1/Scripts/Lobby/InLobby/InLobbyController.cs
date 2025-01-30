#if false
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Battle1.PhotonUnityNetworking.Code;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using DisconnectCause = Battle1.PhotonRealtime.Code.DisconnectCause;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;

namespace Battle1.Scripts.Lobby.InLobby
{
    public class InLobbyController : MonoBehaviourPunCallbacks
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

        public override void OnEnable()
        {
            base.OnEnable();

            var cloudRegion = PhotonNetwork.NetworkingClient?.CloudRegion;
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var photonRegion = string.IsNullOrEmpty(playerSettings.PhotonRegion) ? null : playerSettings.PhotonRegion;
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState} CloudRegion={cloudRegion} PhotonRegion={photonRegion}");
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

        public override void OnDisable()
        {
            CloseWindow();
        }

        private void UpdateTitle()
        {
            // Save region for later use because getting it is not cheap (b ut not very expensive either). 
            /*_currentRegion = PhotonLobby.GetRegion();
            _view.TitleText = $"{Application.productName} {PhotonLobby.GameVersion}";*/
        }

        private IEnumerator StartLobby(string playerGuid, string photonRegion)
        {
            var networkClientState = PhotonNetwork.NetworkClientState;
            Debug.Log($"{networkClientState}");
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonNetwork.InLobby)
            {
                if (networkClientState != PhotonNetwork.NetworkClientState)
                {
                    // Even with delay we must reduce NetworkClientState logging to only when it changes to avoid flooding (on slower connections).
                    networkClientState = PhotonNetwork.NetworkClientState;
                    Debug.Log($"{networkClientState}");
                }
                /*if (PhotonNetwork.InRoom)
                {
                    PhotonLobby.LeaveRoom();
                }
                else if (PhotonWrapper.CanConnect)
                {
                    var store = Storefront.Get();
                    PlayerData playerData = null;
                    store.GetPlayerData(playerGuid, p => playerData = p);
                    yield return new WaitUntil(() => playerData != null);
                    PhotonLobby.Connect(playerData.Name, photonRegion);
                }
                else if (PhotonWrapper.CanJoinLobby)
                {
                    PhotonLobby.JoinLobby();
                }*/
                yield return delay;
            }
            UpdateTitle();
            _view.EnableButtons();
        }

        private void Update()
        {
            if (!PhotonNetwork.InLobby)
            {
                _view.LobbyText = "Wait";
                return;
            }
            var playerCount = PhotonNetwork.CountOfPlayers;
            _view.LobbyText = $"Alue: {_currentRegion} : {PhotonNetwork.GetPing()} ms";
            _view.PlayerCountText = $"Pelaajia online: {playerCount}";
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnDisconnected {cause}");

            if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
            {
                OnEnable();
            }
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }

        private void RaidButtonOnClick()
        {
            SceneManager.LoadScene("te-test-raid-demo");
        }

        private void QuickGameButtonOnClick()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
        }
    }
}
#endif
