using System.Collections;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Battle.Photon;
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
        
        [SerializeField] private Button _inviteOnlinePlayerButton;
        [SerializeField] private InRoomInviteSelectorPanel _inviteSelectorPanel;

        private Coroutine _inviteLifecycleHolder;
        private const float InviteLifecycleTickSeconds = 1f;
        private const long InviteExpirationSeconds = 60;

        private void Awake()
        {
            //buttons[0].onClick.AddListener(SetPlayerAsGuest);
            //buttons[1].onClick.AddListener(SetPlayerAsSpectator);
            _startGameButton.onClick.AddListener(StartPlaying);
            _backButton.onClick.AddListener(GoBack);
            // premade target-mode selector removed until prefab wiring is fixed
            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.onClick.AddListener(OnInviteOnlinePlayerButtonPressed);
            //buttons[3].onClick.AddListener(StartRaidTest);
        }

        private void OnEnable()
        {
            if (_startGameButton != null) _startGameButton.interactable = true;
            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.interactable = InLobbyController.SelectedGameType == GameType.FriendLobby;

            switch (InLobbyController.SelectedGameType)
            {
                case GameType.Custom:
                    if (_title != null) StartCoroutine(SetRoomTitle());
                    if (_conflictText != null) StartCoroutine(CycleConflicts());
                    break;
                case GameType.FriendLobby:
                    if (_title != null) _title.text = "Friend Lobby";
                    if (_noticeText != null) _noticeText.text = "Kutsu yksi online-pelaaja ja valitse haettava 2v2 pelimuoto.";
                    if (_sendInviteToFriendText != null) _sendInviteToFriendText.text = "Kutsu online-pelaaja";
                    EnsureInviteSelectorPanel();
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

            if (InLobbyController.SelectedGameType == GameType.FriendLobby)
            {
                StartInviteLifecycleMonitoring();
            }
            else
            {
                StopInviteLifecycleMonitoring();
            }
        }

        private void OnDestroy()
        {
            // premade target-mode selector removed until prefab wiring is fixed
            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.onClick.RemoveListener(OnInviteOnlinePlayerButtonPressed);
            if (_inviteSelectorPanel != null) _inviteSelectorPanel.HideSilently();
            StopInviteLifecycleMonitoring();
            _startGameButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        private void OnDisable()
        {
            if (_inviteSelectorPanel != null) _inviteSelectorPanel.HideSilently();
            StopInviteLifecycleMonitoring();
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
            void RestoreStartButton()
            {
                if (_startGameButton != null) _startGameButton.interactable = true;
            }

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

                case GameType.FriendLobby:
                    if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
                    {
                        RestoreStartButton();
                        return;
                    }

                    if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
                    {
                        PopupSignalBus.OnChangePopupInfoSignal("Vain huoneen johtaja voi aloittaa matchmakingin.");
                        RestoreStartButton();
                        return;
                    }

                    if (PhotonLobbyRoom.CountRealPlayers() != PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        PopupSignalBus.OnChangePopupInfoSignal($"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                        RestoreStartButton();
                        return;
                    }

                    GameType targetGameType = InLobbyController.SelectedPremadeTargetGameType;
                    if (targetGameType != GameType.Random2v2 && targetGameType != GameType.Clan2v2)
                    {
                        targetGameType = GameType.Random2v2;
                    }

                    string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId ?? string.Empty;
                    string teammateUserId = string.Empty;
                    foreach (var player in PhotonRealtimeClient.CurrentRoom.Players.Values)
                    {
                        if (player == null || player.UserId == localUserId) continue;
                        teammateUserId = player.UserId;
                        break;
                    }

                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)targetGameType);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, localUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, teammateUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                    this.Publish(new LobbyManager.StartMatchmakingEvent(targetGameType, true));
                    break;

                case GameType.Clan2v2:
                    if (PhotonLobbyRoom.CountRealPlayers() == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        // Prevent starting matchmaking when this room is actually a queue room
                        try
                        {
                            var curr = PhotonRealtimeClient.LobbyCurrentRoom;
                            if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey))
                            {
                                Debug.Log("StartPlaying suppressed: current room is a queue room (Clan2v2).");
                                RestoreStartButton();
                                return;
                            }
                        }
                        catch { }
                        this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedGameType));
                    }
                    else
                    {
                        PopupSignalBus.OnChangePopupInfoSignal($"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                        RestoreStartButton();
                    }
                    break;
                case GameType.Random2v2:
                    // Prevent starting matchmaking when this room is a queue room
                    try
                    {
                        var curr = PhotonRealtimeClient.LobbyCurrentRoom;
                        if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey))
                        {
                            Debug.Log("StartPlaying suppressed: current room is a queue room (Random2v2).");
                            RestoreStartButton();
                            return;
                        }
                    }
                    catch { }
                    this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedGameType));
                    break;
            }
        }

        // Premade target selector UI path temporarily removed.

        private void OnInviteOnlinePlayerButtonPressed()
        {
            if (InLobbyController.SelectedGameType != GameType.FriendLobby) return;
            StartCoroutine(InviteOnlinePlayerRoutine());
        }

        private IEnumerator InviteOnlinePlayerRoutine()
        {
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null) yield break;

            if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                PopupSignalBus.OnChangePopupInfoSignal("Vain huoneen johtaja voi lähettää kutsun.");
                yield break;
            }

            List<ServerOnlinePlayer> candidates = null;
            yield return StartCoroutine(GetInviteCandidatesRoutine(result => candidates = result));

            if (candidates == null || candidates.Count == 0)
            {
                PopupSignalBus.OnChangePopupInfoSignal("Ei sopivia online-pelaajia kutsuttavaksi.");
                yield break;
            }

            EnsureInviteSelectorPanel();
            if (_inviteSelectorPanel == null)
            {
                PopupSignalBus.OnChangePopupInfoSignal("Kutsulistaa ei voitu avata. Yrita uudelleen.");
                yield break;
            }

            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.interactable = false;
            _inviteSelectorPanel.Show(
                candidates,
                selectedPlayer =>
                {
                    if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.interactable = true;
                    SendInviteToOnlinePlayer(selectedPlayer);
                },
                () =>
                {
                    if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.interactable = true;
                    PopupSignalBus.OnChangePopupInfoSignal("Kutsun lähetys peruttu.");
                });
        }

        private IEnumerator GetInviteCandidatesRoutine(Action<List<ServerOnlinePlayer>> callback)
        {
            List<ServerOnlinePlayer> onlinePlayers = null;
            if (ServerManager.Instance != null)
            {
                List<ServerOnlinePlayer> fetchedPlayers = null;
                yield return StartCoroutine(ServerManager.Instance.GetOnlinePlayersFromServer(players => fetchedPlayers = players));

                // Always prefer a fresh server snapshot when opening the invite list.
                onlinePlayers = fetchedPlayers ?? ServerManager.Instance.OnlinePlayers;
            }

            callback?.Invoke(FilterInviteCandidates(onlinePlayers));
        }

        private List<ServerOnlinePlayer> FilterInviteCandidates(List<ServerOnlinePlayer> onlinePlayers)
        {
            List<ServerOnlinePlayer> candidates = new();
            if (onlinePlayers == null || onlinePlayers.Count == 0) return candidates;

            string localUserId = GetLocalUserId();
            foreach (ServerOnlinePlayer onlinePlayer in onlinePlayers)
            {
                if (onlinePlayer == null || string.IsNullOrEmpty(onlinePlayer._id)) continue;
                if (onlinePlayer._id == localUserId) continue;
                if (IsPlayerAlreadyInCurrentRoom(onlinePlayer._id)) continue;
                candidates.Add(onlinePlayer);
            }

            return candidates;
        }

        private void SendInviteToOnlinePlayer(ServerOnlinePlayer onlinePlayer)
        {
            if (onlinePlayer == null || string.IsNullOrEmpty(onlinePlayer._id))
            {
                PopupSignalBus.OnChangePopupInfoSignal("Virheellinen kutsuttava pelaaja.");
                return;
            }

            if (!TrySendInviteToUserId(onlinePlayer._id))
            {
                return;
            }

            PopupSignalBus.OnChangePopupInfoSignal($"Kutsu lähetetty pelaajalle {GetOnlinePlayerDisplayName(onlinePlayer)}.");
        }

        private bool TrySendInviteToUserId(string invitedUserId)
        {
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
            {
                return false;
            }

            if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                PopupSignalBus.OnChangePopupInfoSignal("Vain huoneen johtaja voi lähettää kutsun.");
                return false;
            }

            if (string.IsNullOrEmpty(invitedUserId))
            {
                PopupSignalBus.OnChangePopupInfoSignal("Sopivaa kutsuttavaa online-pelaajaa ei löytynyt.");
                return false;
            }

            string localUserId = GetLocalUserId();
            if (string.IsNullOrEmpty(localUserId) || invitedUserId == localUserId) return false;
            if (IsPlayerAlreadyInCurrentRoom(invitedUserId))
            {
                PopupSignalBus.OnChangePopupInfoSignal("Valittu pelaaja on jo huoneessa.");
                return false;
            }

            int inviteState = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateNone);
            string currentInvitedUserId = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
            if (inviteState == PhotonBattleRoom.PremadeInviteStatePending && currentInvitedUserId == invitedUserId)
            {
                PopupSignalBus.OnChangePopupInfoSignal("Kutsu on jo lähetetty tälle pelaajalle.");
                return false;
            }

            try
            {
                PhotonRealtimeClient.LobbyCurrentRoom.ClearExpectedUsers();
                PhotonRealtimeClient.LobbyCurrentRoom.SetExpectedUsers(new[] { invitedUserId });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"TrySendInviteToUserId: failed to set expected users: {ex.Message}");
            }

            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeLeaderUserIdKey, localUserId);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInvitedUserIdKey, invitedUserId);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStatePending);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteTimestampKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)InLobbyController.SelectedPremadeTargetGameType);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, localUserId);
            PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, string.Empty);

            return true;
        }

        private string GetLocalUserId()
        {
            string localUserId = PhotonRealtimeClient.LocalPlayer?.UserId;
            if (string.IsNullOrEmpty(localUserId) && ServerManager.Instance?.Player != null)
            {
                localUserId = ServerManager.Instance.Player._id;
            }

            return localUserId;
        }

        private bool IsPlayerAlreadyInCurrentRoom(string userId)
        {
            if (string.IsNullOrEmpty(userId) || PhotonRealtimeClient.CurrentRoom == null) return false;

            foreach (var player in PhotonRealtimeClient.CurrentRoom.Players.Values)
            {
                if (player == null || string.IsNullOrEmpty(player.UserId)) continue;
                if (player.UserId == userId) return true;
            }

            return false;
        }

        private static string GetOnlinePlayerDisplayName(ServerOnlinePlayer onlinePlayer)
        {
            if (onlinePlayer == null) return "Tuntematon";
            if (!string.IsNullOrWhiteSpace(onlinePlayer.name)) return onlinePlayer.name;
            return string.IsNullOrEmpty(onlinePlayer._id) ? "Tuntematon" : onlinePlayer._id;
        }

        private void StartInviteLifecycleMonitoring()
        {
            if (_inviteLifecycleHolder != null)
            {
                return;
            }

            _inviteLifecycleHolder = StartCoroutine(InviteLifecycleRoutine());
        }

        private void StopInviteLifecycleMonitoring()
        {
            if (_inviteLifecycleHolder == null)
            {
                return;
            }

            StopCoroutine(_inviteLifecycleHolder);
            _inviteLifecycleHolder = null;
        }

        private IEnumerator InviteLifecycleRoutine()
        {
            WaitForSecondsRealtime delay = new(InviteLifecycleTickSeconds);

            while (true)
            {
                if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.CurrentRoom == null)
                {
                    yield return delay;
                    continue;
                }

                var localLobbyPlayer = PhotonRealtimeClient.LocalLobbyPlayer;
                if (localLobbyPlayer == null || !localLobbyPlayer.IsMasterClient)
                {
                    yield return delay;
                    continue;
                }

                GameType roomGameType = GameType.FriendLobby;
                bool failedToReadRoomGameType = false;
                try
                {
                    roomGameType = (GameType)PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);
                }
                catch
                {
                    failedToReadRoomGameType = true;
                }

                if (failedToReadRoomGameType)
                {
                    yield return delay;
                    continue;
                }

                if (roomGameType != GameType.FriendLobby)
                {
                    yield return delay;
                    continue;
                }

                int inviteState = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateNone);
                if (inviteState != PhotonBattleRoom.PremadeInviteStatePending)
                {
                    yield return delay;
                    continue;
                }

                string invitedUserId = PhotonRealtimeClient.CurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                if (string.IsNullOrEmpty(invitedUserId))
                {
                    yield return delay;
                    continue;
                }

                if (IsPlayerAlreadyInCurrentRoom(invitedUserId))
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateAccepted);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, invitedUserId);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteTimestampKey, 0L);
                    if (PhotonRealtimeClient.CurrentRoom.PlayerCount >= PhotonRealtimeClient.CurrentRoom.MaxPlayers)
                    {
                        PhotonRealtimeClient.CurrentRoom.IsOpen = false;
                    }
                    yield return delay;
                    continue;
                }

                long inviteTimestampSeconds = 0;
                try
                {
                    if (PhotonRealtimeClient.CurrentRoom.CustomProperties != null
                        && PhotonRealtimeClient.CurrentRoom.CustomProperties.ContainsKey(PhotonBattleRoom.PremadeInviteTimestampKey))
                    {
                        inviteTimestampSeconds = Convert.ToInt64(PhotonRealtimeClient.CurrentRoom.CustomProperties[PhotonBattleRoom.PremadeInviteTimestampKey]);
                    }
                }
                catch
                {
                    inviteTimestampSeconds = 0;
                }

                long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (inviteTimestampSeconds <= 0)
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteTimestampKey, nowSeconds);
                    yield return delay;
                    continue;
                }

                if (nowSeconds - inviteTimestampSeconds >= InviteExpirationSeconds)
                {
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteStateKey, PhotonBattleRoom.PremadeInviteStateExpired);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInvitedUserIdKey, string.Empty);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, string.Empty);
                    PhotonRealtimeClient.CurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeInviteTimestampKey, 0L);

                    try { PhotonRealtimeClient.LobbyCurrentRoom.ClearExpectedUsers(); }
                    catch (Exception ex) { Debug.LogWarning($"InviteLifecycleRoutine: failed to clear expected users on expiry: {ex.Message}"); }

                    PopupSignalBus.OnChangePopupInfoSignal("Kutsu vanheni. Voit lahettaa uuden kutsun.");
                }

                yield return delay;
            }
        }

        private void EnsureInviteSelectorPanel()
        {
            if (_inviteSelectorPanel != null)
            {
                return;
            }

            _inviteSelectorPanel = GetComponentInChildren<InRoomInviteSelectorPanel>(true);
            if (_inviteSelectorPanel == null)
            {
                Debug.LogWarning("InRoomController: InRoomInviteSelectorPanel prefab instance is missing from Battle Popup hierarchy.");
                if (_inviteOnlinePlayerButton != null)
                {
                    _inviteOnlinePlayerButton.interactable = false;
                }
                return;
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
