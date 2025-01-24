#if false
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using IInRoomCallbacks = Battle1.PhotonRealtime.Code.IInRoomCallbacks;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;

namespace Battle1.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Lowest pane in lobby while in room to show current players list that have joined this room.
    /// </summary>
    public class PanePlayerListing : MonoBehaviour, IInRoomCallbacks
    {
        private const string PlayerPositionKey = PhotonBattle.PlayerPositionKey;
        private const string PlayerMainSkillKey = PhotonBattle.PlayerPrefabIdKey;
        private const int PlayerPositionGuest = PhotonBattle.PlayerPositionGuest;

        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;

        [SerializeField] private Text _textTemplate;
        [SerializeField] private Transform _contentRoot;

        private void OnEnable()
        {
            if (PhotonNetwork.InRoom)
            {
                UpdateStatus();
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            DeleteExtraLines(_contentRoot);
        }

        private void UpdateStatus()
        {
            // Use PaneRoomListing.updateStatus() style to manage dynamic text lines - IMHO is has better implementation!
            if (!PhotonNetwork.InRoom)
            {
                DeleteExtraLines(_contentRoot);
                return;
            }
            var players = PhotonNetwork.CurrentRoom.GetPlayersByNickName().ToList();
            Debug.Log($"updateStatus {PhotonNetwork.NetworkClientState} lines: {_contentRoot.childCount} players: {players.Count}");

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

        private static void UpdatePlayerLine(Text line, Player player)
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

        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // NOP
        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            UpdateStatus();
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            // NOP
        }
    }
}
#endif
