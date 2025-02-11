using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUI.Scripts.Lobby.InLobby
{
    public class InLobbyController : AltMonoBehaviour
    {
        [SerializeField] private InLobbyView _view;
        [SerializeField] private SelectedCharactersPopup _selectedCharactersPopup;
        [SerializeField] private GameObject _popupContents;

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
            var networkClientState = PhotonRealtimeClient.LobbyNetworkClientState;
            Debug.Log($"{networkClientState}");
            var delay = new WaitForSeconds(0.1f);
            while (!PhotonRealtimeClient.InLobby)
            {
                if (networkClientState != PhotonRealtimeClient.LobbyNetworkClientState)
                {
                    // Even with delay we must reduce NetworkClientState logging to only when it changes to avoid flooding (on slower connections).
                    networkClientState = PhotonRealtimeClient.LobbyNetworkClientState;
                    Debug.Log($"{networkClientState}");
                }
                if (PhotonRealtimeClient.InRoom)
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
                    PhotonRealtimeClient.JoinLobbyWithWrapper(null);
                }
                yield return delay;
            }
            UpdateTitle();
            _view.EnableButtons();
        }

        private void Update()
        {
            if (!PhotonRealtimeClient.InLobby && !PhotonRealtimeClient.InRoom)
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
            if (_popupContents.gameObject.activeSelf)
            {
                CloseWindow();
            }
            else
            {
                StartCoroutine(GetPlayerData(playerData =>
                {
                    // Check if player has all 3 characters selected or no

                    if (playerData != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (playerData.SelectedCharacterIds[i] == 0) // if any of the selected characters is missing
                            {
                                StartCoroutine(ShowSelectedCharactersPopup());
                                return;
                            }
                        }
                    }
                    // Open battle popup if all 3 are selected
                    OpenWindow();
                }));
            }
        }


        private IEnumerator ShowSelectedCharactersPopup()
        {
            yield return StartCoroutine(_selectedCharactersPopup.ShowPopup(showBattlePopup =>
            {
                if (showBattlePopup == true)
                {
                    OpenWindow();
                }
            }));
        }


        private void OpenWindow()
        {
            _popupContents.SetActive(true);
            _view.ShowMainPanel();
        }


        public void CloseWindow()
        {
            _popupContents.SetActive(false);
        }

        private void CharacterButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }

        private void RoomButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }

        private void RaidButtonOnClick()
        {
            SceneManager.LoadScene("te-test-raid-demo");
        }

        private void QuickGameButtonOnClick()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
        }
    }
}
