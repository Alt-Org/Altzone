using System.Collections;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Sets the room's title inside a room. Handles calling going back and starting matchmaking from room when pressing buttons in the UI.
    /// </summary>
    public class InRoomController : MonoBehaviour
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
            StartCoroutine(SetRoomTitle());
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

        private IEnumerator SetRoomTitle()
        {
            do
            {
                _title.text = PhotonRealtimeClient.InRoom ? PhotonRealtimeClient.LobbyCurrentRoom.Name : "<color=red>Not in room</color>";
            } while (!PhotonRealtimeClient.InRoom);

            yield return null;
        }
    }
}
