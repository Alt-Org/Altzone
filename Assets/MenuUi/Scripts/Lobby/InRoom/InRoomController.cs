using System.Collections;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using MenuUi.Scripts.Lobby.InLobby;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using SignalBus = MenuUi.Scripts.Signals.SignalBus;
using PopupSignalBus = MenuUI.Scripts.SignalBus;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using System;
using Random = UnityEngine.Random;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Sets the room's title inside a room. Handles calling going back and starting matchmaking from room when pressing buttons in the UI.
    /// </summary>
    public class InRoomController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextLanguageSelectorCaller _conflictText;
        [SerializeField] private List<Conflicts> _conflicts;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private TMP_Text _noticeText;
        [SerializeField] private TMP_Text _sendInviteToFriendText;

        private void Awake()
        {
            //buttons[0].onClick.AddListener(SetPlayerAsGuest);
            //buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            _startGameButton.onClick.AddListener(StartPlaying);
            _backButton.onClick.AddListener(GoBack);
            //buttons[3].onClick.AddListener(StartRaidTest);
        }

        private void OnEnable()
        {
            switch (InLobbyController.SelectedGameType)
            {
                case GameType.Custom:
                    if (_title != null) StartCoroutine(SetRoomTitle());
                    if (_conflictText != null) StartCoroutine(CycleConflicts());
                    break;
                case GameType.Random2v2:
                    //if (_title != null) _title.text = "Keräily 2v2";
                    //if (_noticeText != null) _noticeText.text = "Tätä pelimuotoa voi mennä pelaamaan yksin tai kaverin kanssa (työn alla). Huom. Jos menet pelaamaan yksin, paikan valinnalla ei ole merkitystä.";
                    //if (_sendInviteToFriendText != null) _sendInviteToFriendText.text = "Lähetä kutsu kaverille";
                    _roomSwitcher.ClosePanels();
                    StartPlaying();
                    break;
                case GameType.Clan2v2:
                    if (_title != null) _title.text = "Klaani 2v2";
                    if (_noticeText != null) _noticeText.text = "Kutsun lähettäminen ei vielä toimi. Saman klaanin jäsen voi liittyä tähän huoneeseen menemällä peliin 2v2 klaanijäsenen kanssa.";
                    if (_sendInviteToFriendText != null) _sendInviteToFriendText.text = "Lähetä kutsu yhdelle klaanin jäsenelle";
                    break;
            }
        }

        private void OnDestroy()
        {
            _startGameButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
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
            //if (!PhotonLobbyRoom.IsValidAllSelectedCharacters())
            //{
            //    SignalBus.OnChangePopupInfoSignal("Kaikkien pelaajien pitää ensin valita 3 puolustushahmoa.");
            //    return;
            //}
            _startGameButton.interactable = false;

            switch (InLobbyController.SelectedGameType)
            {
                case GameType.Custom:
                    this.Publish(new LobbyManager.StartPlayingEvent());
                    break;

                case GameType.Clan2v2:
                    if (PhotonLobbyRoom.CountRealPlayers() == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedGameType));
                    }
                    else
                    {
                        PopupSignalBus.OnChangePopupInfoSignal($"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                    }
                    break;
                case GameType.Random2v2:
                    this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedGameType));
                    break;
            }
        }
        private void GoBack()
        {
            Debug.Log($"leavingRoom");
            PhotonRealtimeClient.LeaveRoom();
            if (InLobbyController.SelectedGameType != GameType.Clan2v2) SignalBus.OnCloseBattlePopupRequestedSignal();
            //this.Publish(new LobbyManager.StartPlayingEvent());
        }

        private void StartRaidTest()
        {
            Debug.Log($"startPlaying");
            this.Publish(new LobbyManager.StartRaidTestEvent());
        }

        private IEnumerator SetRoomTitle()
        {
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);
            // Getting room name either from custom properties or from the room's name itself.
            string roomName = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PhotonLobbyRoom.RoomNameKey);
            if (string.IsNullOrEmpty(roomName)) roomName = PhotonRealtimeClient.LobbyCurrentRoom.Name;
            _title.text = roomName;
        }

        private IEnumerator CycleConflicts()
        {
            if (_conflicts == null || _conflicts.Count == 0) yield break;
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);
            int previousConflict = -1;
            while (PhotonRealtimeClient.InRoom)
            {
                int selectedConflict = Random.Range(0, _conflicts.Count);
                if (selectedConflict == previousConflict) continue;
                _conflictText.SetText(_conflicts[selectedConflict].ConlictText);
                yield return new WaitForSecondsRealtime(7);
            }
            
        }
    }
    [Serializable]
    public class Conflicts
    {
        [SerializeField, TextArea(1, 5)] private string _finnishConflictText;
        [SerializeField, TextArea(1, 5)] private string _englishConflictText;

        public string ConlictText
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish: return _finnishConflictText;
                    case SettingsCarrier.LanguageType.English: return _englishConflictText;
                    default: return _finnishConflictText;
                }
            }
        }
    }
}
