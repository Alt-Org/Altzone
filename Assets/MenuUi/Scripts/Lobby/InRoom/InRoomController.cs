using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby.InLobby;
using MenuUi.Scripts.Window;
using MenuUI.Scripts;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SignalBus = MenuUi.Scripts.Signals.SignalBus;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Handles friend invites and starting matchmaking from the matchmaking lobby panel.
    /// </summary>
    public class InRoomController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _gameType;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private TMP_Text _noticeText;
        [SerializeField] private TMP_Text _sendInviteToFriendText;

        [SerializeField] private Button _inviteOnlinePlayerButton;
        [SerializeField] private InRoomInviteSelectorPanel _inviteSelectorPanel;

        private bool _inviteRequestInFlight;
        private void Awake()
        {
            if (_startGameButton != null) _startGameButton.onClick.AddListener(StartPlaying);
            if (_backButton != null) _backButton.onClick.AddListener(OnBackButtonPressed);

            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.onClick.AddListener(OnInviteOnlinePlayerButtonPressed);

        }

        private void OnEnable()
        {
            if (_startGameButton != null) _startGameButton.interactable = true;
            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.interactable = InLobbyController.SelectedMatchmakingType == MatchmakingType.FriendLobby;

            switch (InLobbyController.SelectedMatchmakingType)
            {
                case MatchmakingType.FriendLobby:
                    if (_title != null) _title.text = "Friend Lobby";
                    if (_gameType) _gameType.text = InLobbyController.SelectedPremadeTargetMatchmakingType.ToString();
                    if (_noticeText != null) _noticeText.text = "Kutsu yksi online-pelaaja ja valitse haettava 2v2 pelimuoto.";
                    if (_sendInviteToFriendText != null) _sendInviteToFriendText.text = "Kutsu online-pelaaja";
                    EnsureInviteSelectorPanel();
                    break;
                case MatchmakingType.Random2v2:
                    _roomSwitcher.ClosePanels();
                    StartPlaying();
                    break;
                case MatchmakingType.Clan2v2:
                    if (_title != null) _title.text = "Klaani 2v2";
                    if (_noticeText != null) _noticeText.text = "Kutsun lähettäminen ei vielä toimi. Saman klaanin jäsen voi liittyä tähän huoneeseen menemällä peliin 2v2 klaanijäsenen kanssa.";
                    if (_sendInviteToFriendText != null) _sendInviteToFriendText.text = "Lähetä kutsu yhdelle klaanin jäsenelle";
                    break;
            }

        }

        private void OnDestroy()
        {
            if (_inviteOnlinePlayerButton != null) _inviteOnlinePlayerButton.onClick.RemoveListener(OnInviteOnlinePlayerButtonPressed);
            if (_inviteSelectorPanel != null) _inviteSelectorPanel.HideSilently();
            if (_startGameButton != null) _startGameButton.onClick.RemoveListener(StartPlaying);
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackButtonPressed);
        }

        private void OnDisable()
        {
            if (_inviteSelectorPanel != null) _inviteSelectorPanel.HideSilently();
        }

        private void StartPlaying()
        {
            void RestoreStartButton()
            {
                if (_startGameButton != null) _startGameButton.interactable = true;
            }

            if (_startGameButton != null) _startGameButton.interactable = false;

            switch (InLobbyController.SelectedMatchmakingType)
            {

                case MatchmakingType.FriendLobby:
                    if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.LobbyCurrentRoom == null)
                    {
                        RestoreStartButton();
                        return;
                    }

                    if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
                    {
                        OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, "Vain huoneen johtaja voi aloittaa matchmakingin.");
                        RestoreStartButton();
                        return;
                    }

                    if (PhotonLobbyRoom.CountRealPlayers() != PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, $"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                        RestoreStartButton();
                        return;
                    }

                    MatchmakingType targetMatchmakingType = InLobbyController.SelectedPremadeTargetMatchmakingType;
                    GameType targetGameType = InLobbyController.SelectedGameType;
                    if (targetMatchmakingType != MatchmakingType.Random2v2 && targetMatchmakingType != MatchmakingType.Clan2v2)
                    {
                        targetMatchmakingType = MatchmakingType.Random2v2;
                    }

                    string localUserId = PhotonRealtimeClient.LocalLobbyPlayer?.UserId ?? string.Empty;
                    string teammateUserId = string.Empty;
                    foreach (var player in PhotonRealtimeClient.LobbyCurrentRoom.Players.Values)
                    {
                        if (player == null || player.UserId == localUserId) continue;
                        teammateUserId = player.UserId;
                        break;
                    }

                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeModeKey, true);
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetMatchmakingTypeKey, (int)targetMatchmakingType);
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeTargetGameTypeKey, (int)targetGameType);
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId1Key, localUserId);
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUsername1Key, GetPlayerName(localUserId));
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUserId2Key, teammateUserId);
                    PhotonRealtimeClient.LobbyCurrentRoom.SetCustomProperty(PhotonBattleRoom.PremadeUsername2Key, GetPlayerName(teammateUserId));
                    this.Publish(new LobbyManager.StartMatchmakingEvent(targetMatchmakingType, targetGameType, true));
                    break;

                case MatchmakingType.Clan2v2:
                    if (PhotonLobbyRoom.CountRealPlayers() == PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers)
                    {
                        this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedMatchmakingType, InLobbyController.SelectedGameType, teamSize: InLobbyController.SelectedTeamSize));
                    }
                    else
                    {
                        OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, $"Huoneessa pitää olla {PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers} pelaajaa.");
                        RestoreStartButton();
                    }
                    break;
                case MatchmakingType.Random2v2:
                    this.Publish(new LobbyManager.StartMatchmakingEvent(InLobbyController.SelectedMatchmakingType, InLobbyController.SelectedGameType, teamSize: InLobbyController.SelectedTeamSize));
                    break;
            }
        }

        private void OnInviteOnlinePlayerButtonPressed()
        {
            if (InLobbyController.SelectedMatchmakingType != MatchmakingType.FriendLobby) return;
            StartCoroutine(InviteOnlinePlayerRoutine());
        }

        private IEnumerator InviteOnlinePlayerRoutine()
        {
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.LobbyCurrentRoom == null) yield break;

            if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, "Vain huoneen johtaja voi lähettää kutsun.");
                yield break;
            }

            List<ServerOnlinePlayer> candidates = null;
            yield return StartCoroutine(GetInviteCandidatesRoutine(result => candidates = result));

            if (candidates == null || candidates.Count == 0)
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Info, "Ei sopivia online-pelaajia kutsuttavaksi.");
                yield break;
            }

            EnsureInviteSelectorPanel();
            if (_inviteSelectorPanel == null)
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Error, "Kutsulistaa ei voitu avata. Yrita uudelleen.");
                yield break;
            }

            SetInviteButtonInteractable(false);
            _inviteSelectorPanel.Show(
                candidates,
                selectedPlayer =>
                {
                    SetInviteButtonInteractable(true);
                    SendInviteToOnlinePlayer(selectedPlayer);
                },
                () =>
                {
                    SetInviteButtonInteractable(true);
                    OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, "Kutsun lähetys peruttu.");
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
            List<ClanMember> members = null;
            if (InLobbyController.SelectedPremadeTargetMatchmakingType == MatchmakingType.Clan2v2)
            {
                ClanData clan = null;
                Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, data => clan = data);
                members = clan.Members;
            }

            foreach (ServerOnlinePlayer onlinePlayer in onlinePlayers)
            {
                if (onlinePlayer == null || string.IsNullOrEmpty(onlinePlayer._id)) continue;
                if (onlinePlayer._id == localUserId) continue;
                if (IsPlayerAlreadyInCurrentRoom(onlinePlayer._id)) continue;
                if (InLobbyController.SelectedPremadeTargetMatchmakingType == MatchmakingType.Clan2v2)
                {
                    if(members.Find((m) => m.Id == onlinePlayer._id) == null) continue;
                }
                candidates.Add(onlinePlayer);
            }

            return candidates;
        }

        private void SendInviteToOnlinePlayer(ServerOnlinePlayer onlinePlayer)
        {
            if (onlinePlayer == null || string.IsNullOrEmpty(onlinePlayer._id))
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Error, "Virheellinen kutsuttava pelaaja.");
                return;
            }

            StartCoroutine(SendServerInviteToOnlinePlayer(onlinePlayer));
        }

        private IEnumerator SendServerInviteToOnlinePlayer(ServerOnlinePlayer onlinePlayer)
        {
            if (_inviteRequestInFlight) yield break;

            string invitedUserId = onlinePlayer._id;
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.LobbyCurrentRoom == null)
            {
                yield break;
            }

            if (!PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient)
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, "Vain huoneen johtaja voi lähettää kutsun.");
                yield break;
            }

            if (string.IsNullOrEmpty(invitedUserId))
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Error, "Sopivaa kutsuttavaa online-pelaajaa ei löytynyt.");
                yield break;
            }

            string localUserId = GetLocalUserId();

            if (string.IsNullOrEmpty(localUserId) || invitedUserId == localUserId) yield break;
            if (IsPlayerAlreadyInCurrentRoom(invitedUserId))
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Warning, "Valittu pelaaja on jo huoneessa.");
                yield break;
            }

            if (ServerManager.Instance == null)
            {
                OverlayPanelCheck.ActivateInfoPopup(InfoLevel.Error, "Vakava virhe: palvelinyhteyttä ei ole alustettu. Käynnistä peli uudelleen.");
                yield break;
            }

            _inviteRequestInFlight = true;
            SetInviteButtonInteractable(false);
            bool inviteSent = false;
            try
            {
                yield return StartCoroutine(ServerManager.Instance.MatchmakingSendInviteToPlayer(invitedUserId, success => inviteSent = success));
            }
            finally
            {
                _inviteRequestInFlight = false;
                SetInviteButtonInteractable(true);
            }

            OverlayPanelCheck.ActivateInfoPopup(
                inviteSent ? InfoLevel.Info : InfoLevel.Error,
                inviteSent
                    ? $"Kutsu lähetetty pelaajalle {GetOnlinePlayerDisplayName(onlinePlayer)}."
                    : "Kutsun lähetys epäonnistui. Yritä uudelleen.");
        }

        private void SetInviteButtonInteractable(bool interactable)
        {
            if (_inviteOnlinePlayerButton != null)
            {
                _inviteOnlinePlayerButton.interactable = interactable;
            }
        }

        private string GetLocalUserId()
        {
            string localUserId = PhotonRealtimeClient.LocalLobbyPlayer?.UserId;
            if (string.IsNullOrEmpty(localUserId) && ServerManager.Instance?.Player != null)
            {
                localUserId = ServerManager.Instance.Player._id;
            }

            return localUserId;
        }

        private bool IsPlayerAlreadyInCurrentRoom(string userId)
        {
            if (string.IsNullOrEmpty(userId) || PhotonRealtimeClient.LobbyCurrentRoom == null) return false;

            foreach (var player in PhotonRealtimeClient.LobbyCurrentRoom.Players.Values)
            {
                if (player == null || string.IsNullOrEmpty(player.UserId)) continue;
                if (player.UserId == userId) return true;
            }

            return false;
        }

        private string GetPlayerName(string userId)
        {
            if (string.IsNullOrEmpty(userId) || PhotonRealtimeClient.LobbyCurrentRoom == null) return string.Empty;

            foreach (var player in PhotonRealtimeClient.LobbyCurrentRoom.Players.Values)
            {
                if (player == null || string.IsNullOrEmpty(player.UserId)) continue;
                if (player.UserId == userId) return player.NickName;
            }

            return string.Empty;
        }

        private static string GetOnlinePlayerDisplayName(ServerOnlinePlayer onlinePlayer)
        {
            if (onlinePlayer == null) return "Tuntematon";
            if (!string.IsNullOrWhiteSpace(onlinePlayer.name)) return onlinePlayer.name;
            return string.IsNullOrEmpty(onlinePlayer._id) ? "Tuntematon" : onlinePlayer._id;
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

        private void OnBackButtonPressed()
        {
            StartCoroutine(GoBack());
        }

        private IEnumerator GoBack()
        {
            Debug.Log($"leavingRoom");
            PhotonRealtimeClient.LeaveRoom();
            yield return new WaitUntil(() => !PhotonRealtimeClient.InRoom);
            SignalBus.OnCloseBattlePopupRequestedSignal();
        }

    }
}
