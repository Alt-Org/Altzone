using System.Collections;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using MenuUi.Scripts.Lobby.InLobby;
using MenuUI.Scripts;
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
        [SerializeField] private TMP_Text _searchingText;

        private void OnEnable()
        {
            if (_searchingText != null && _searchingText.gameObject.activeSelf) _searchingText.gameObject.SetActive(false);
            if (!_startGameButton.gameObject.activeSelf) _startGameButton.gameObject.SetActive(true);
        }

        private void Start()
        {
            //buttons[0].onClick.AddListener(SetPlayerAsGuest);
            //buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            _startGameButton.onClick.AddListener(StartPlaying);
            _backButton.onClick.AddListener(GoBack);
            //buttons[3].onClick.AddListener(StartRaidTest);
            if (_title != null) StartCoroutine(SetRoomTitle());
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
            if (!PhotonLobbyRoom.IsValidAllSelectedCharacters())
            {
                SignalBus.OnChangePopupInfoSignal("Kaikkien pelaajien pitää ensin valita 3 puolustushahmoa.");
                return;
            }

            switch (InLobbyController.SelectedGameType)
            {
                case GameType.Custom:
                    this.Publish(new LobbyManager.StartPlayingEvent());
                    break;

                case GameType.Clan2v2:
                    if (PhotonLobbyRoom.CountRealPlayers() == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        _searchingText.gameObject.SetActive(true);
                        _startGameButton.gameObject.SetActive(false);
                        this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedGameType));
                    }
                    else
                    {
                        SignalBus.OnChangePopupInfoSignal($"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                    }
                    break;
            }
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
