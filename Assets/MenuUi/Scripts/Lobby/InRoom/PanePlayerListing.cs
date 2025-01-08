﻿using System.Linq;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Lowest pane in lobby while in room to show current players list that have joined this room.
    /// </summary>
    public class PanePlayerListing : MonoBehaviour
    {
        private const string PlayerPositionKey = PhotonLobbyRoom.PlayerPositionKey;
        private const string PlayerMainSkillKey = PhotonLobbyRoom.PlayerPrefabIdKey;
        private const int PlayerPositionGuest = PhotonLobbyRoom.PlayerPositionGuest;

        private const int PlayerPosition1 = PhotonLobbyRoom.PlayerPosition1;
        private const int PlayerPosition2 = PhotonLobbyRoom.PlayerPosition2;
        private const int PlayerPosition3 = PhotonLobbyRoom.PlayerPosition3;
        private const int PlayerPosition4 = PhotonLobbyRoom.PlayerPosition4;

        [SerializeField] private Text _textTemplate;
        [SerializeField] private Transform _contentRoot;

        private void OnEnable()
        {
            if (PhotonRealtimeClient.InRoom)
            {
                UpdateStatus();
            }

            LobbyManager.LobbyOnPlayerEnteredRoom += OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom += OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate += OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched += OnMasterClientSwitched;

            PhotonRealtimeClient.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonRealtimeClient.RemoveCallbackTarget(this);
            LobbyManager.LobbyOnPlayerEnteredRoom -= OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom -= OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched -= OnMasterClientSwitched;
            DeleteExtraLines(_contentRoot);
        }

        private void UpdateStatus()
        {
            // Use PaneRoomListing.updateStatus() style to manage dynamic text lines - IMHO is has better implementation!
            if (!PhotonRealtimeClient.InRoom)
            {
                DeleteExtraLines(_contentRoot);
                return;
            }
            var players = PhotonRealtimeClient.GetCurrentRoomPlayersByNickName();
            Debug.Log($"updateStatus {PhotonRealtimeClient.State} lines: {_contentRoot.childCount} players: {players.Count}");
            
            // Synchronize line count with player count.
            while (_contentRoot.childCount < players.Count)
            {
                AddTextLine(_contentRoot, _textTemplate);
            }
            // Update text lines
            for (var i = 0; i < players.Count; ++i)
            {
                var player = players[i];
                var lineObject = _contentRoot.GetChild(i).gameObject;
                lineObject.SetActive(true);
                var line = lineObject.GetComponent<Text>();
                UpdatePlayerLine(line, player);
            }
            // Hide extra lines
            if (_contentRoot.childCount > players.Count)
            {
                for (var i = players.Count; i < _contentRoot.childCount; ++i)
                {
                    var lineObject = _contentRoot.GetChild(i).gameObject;
                    if (lineObject.activeSelf)
                    {
                        lineObject.SetActive(false);
                    }
                }
            }
        }

        private void AddTextLine(Transform parent, Text template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, parent);
            instance.SetActive(true);
        }

        private static void UpdatePlayerLine(Text line, LobbyPlayer player)
        {
            var text = line.GetComponent<Text>();
            var nickName = player.IsLocal ? RichText.Blue(player.NickName) : player.NickName;
            var pos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            var status = $" p={pos}";
            if (pos >= PlayerPosition1 && pos <= PlayerPosition4)
            {
                var prefabIndex = player.GetCustomProperty(PlayerMainSkillKey, 0);
                status += $" #={RichText.Green(prefabIndex)}";
            }
            if (player.IsMasterClient)
            {
                status += $" {RichText.Yellow("M")}";
            }
            var playerText = $"{nickName} {status}";
            Debug.Log($"update '{text.text}' -> '{playerText}'");
            text.text = playerText;
        }

        private static void DeleteExtraLines(Transform parent)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        void OnPlayerEnteredRoom(LobbyPlayer newPlayer)
        {
            UpdateStatus();
        }

        void OnPlayerLeftRoom(LobbyPlayer otherPlayer)
        {
            UpdateStatus();
        }

        void OnRoomPropertiesUpdate(LobbyPhotonHashtable propertiesThatChanged)
        {
            // NOP
        }

        void OnPlayerPropertiesUpdate(LobbyPlayer targetPlayer, LobbyPhotonHashtable changedProps)
        {
            UpdateStatus();
        }

        void OnMasterClientSwitched(LobbyPlayer newMasterClient)
        {
            // NOP
        }
    }
}
