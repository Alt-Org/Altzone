using System;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Photon.Client.StructWrapping;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles spawning the room list buttons to the search panel in Battle Popup.
    /// </summary>
    public class RoomSearchPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private TextMeshProUGUI _noRoomText;
        private List<LobbyRoomInfo> _roomsData;
        private Action<string> _onJoinRoom;

        public List<LobbyRoomInfo> RoomsData {
            set
            {
                _roomsData = value;
                if (_roomsData != null)
                {
                    if(gameObject.activeSelf)
                    UpdatePanelContent();
                }
            }
        }

        private void OnEnable()
        {
            if(_roomsData != null) UpdatePanelContent();
        }

        public void SetOnJoinRoom(Action<string> onJoinRoom)
        {
            if(_onJoinRoom == null) _onJoinRoom = onJoinRoom;
        }

        private void UpdatePanelContent()
        {
            if (_slotPrefab == null) return;
            if (_content == null) return;

            ResetPanel(_content);

            if (!_noRoomText.gameObject.activeSelf) _noRoomText.gameObject.SetActive(true);

            foreach(LobbyRoomInfo roomInfo in _roomsData)
            {
                bool gameTypePropertyExist = roomInfo.CustomProperties.TryGetValue(PhotonBattleLobbyRoom.GameTypeKey, out int gameType);
                if (gameTypePropertyExist && gameType == (int)GameType.Custom)
                {
                    GameObject button = Instantiate(_slotPrefab, _content);
                    RoomSlot roomSlot = button.GetComponent<RoomSlot>();
                    if (roomSlot != null)
                    {
                        UpdateButton(roomSlot, roomInfo, _onJoinRoom);
                    }
                    if (_noRoomText.gameObject.activeSelf) _noRoomText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateButton(RoomSlot slot, LobbyRoomInfo room, Action<string> onJoinRoom)
        {
            Button button = slot.transform.GetComponentInChildren<Button>();

            button.onClick.RemoveAllListeners();
            button.interactable = room.IsOpen;
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => onJoinRoom(room.Name));
            }

            slot.SetInfo(room);
        }

        private static void ResetPanel(Transform parent)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i).gameObject;
                Destroy(child);
            }
        }

    }
}
