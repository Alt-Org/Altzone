using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Middle pane in lobby while in room to manage current player "state".
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _battleID;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;

        private void Start()
        {
            //buttons[0].onClick.AddListener(SetPlayerAsGuest);
            //buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            _startGameButton.onClick.AddListener(StartPlaying);
            _backButton.onClick.AddListener(GoBack);
            //buttons[3].onClick.AddListener(StartRaidTest);
        }

        private void SetPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {PhotonLobbyRoom.PlayerPositionGuest}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonLobbyRoom.PlayerPositionGuest));
        }

        private void SetPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {PhotonLobbyRoom.PlayerPositionSpectator}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonLobbyRoom.PlayerPositionSpectator));
        }

        private void StartPlaying()
        {
            Debug.Log($"startPlaying");
            this.Publish(new LobbyManager.StartPlayingEvent());
        }
        private void GoBack()
        {
            Debug.Log($"leavingRoom");
            PhotonRealtimeClient.LeaveRoom();
            _roomSwitcher.ReturnToMain();
            //this.Publish(new LobbyManager.StartPlayingEvent());
        }

        private void StartRaidTest()
        {
            Debug.Log($"startPlaying");
            this.Publish(new LobbyManager.StartRaidTestEvent());
        }

        /// <summary>
        /// Stupid way to poll network state changes on every frame!
        /// </summary>
        private void Update()
        {
            //_battleID.text = PhotonRealtimeClient.InRoom ? $"({PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>("bid")})" : "<color=red>Not in room</color>";
            _title.text = PhotonRealtimeClient.InRoom ? PhotonRealtimeClient.LobbyCurrentRoom.Name : "<color=red>Not in room</color>";
        }
    }
}
