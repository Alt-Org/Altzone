using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Lobby;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Middle pane in lobby while in room to manage current player "state".
    /// </summary>
    public class PaneInRoom : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text _battleID;
        [SerializeField] private Button[] buttons;

        private void Start()
        {
            buttons[0].onClick.AddListener(SetPlayerAsGuest);
            buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            buttons[2].onClick.AddListener(StartPlaying);
            buttons[3].onClick.AddListener(StartRaidTest);
        }

        private void SetPlayerAsGuest()
        {
            Debug.Log($"setPlayerAsGuest {PhotonBattleLobbyRoom.PlayerPositionGuest}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonBattleLobbyRoom.PlayerPositionGuest));
        }

        private void SetPlayerAsSpectator()
        {
            Debug.Log($"setPlayerAsSpectator {PhotonBattleLobbyRoom.PlayerPositionSpectator}");
            this.Publish(new LobbyManager.PlayerPosEvent(PhotonBattleLobbyRoom.PlayerPositionSpectator));
        }

        private void StartPlaying()
        {
            Debug.Log($"startPlaying");
            this.Publish(new LobbyManager.StartPlayingEvent());
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
            _battleID.text = PhotonRealtimeClient.InRoom ? $"({PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>("bid")})" : "<color=red>Not in room</color>";
            title.text = PhotonRealtimeClient.InRoom ? PhotonRealtimeClient.LobbyCurrentRoom.Name : "<color=red>Not in room</color>";
        }
    }
}
