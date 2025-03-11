using System;
using System.Collections.Generic;
using Altzone.Scripts.Lobby.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InLobby
{
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

            if(_roomsData.Count == 0) _noRoomText.gameObject.SetActive(true);
            else _noRoomText.gameObject.SetActive(false);

            foreach(LobbyRoomInfo roomInfo in _roomsData)
            {
                GameObject button = Instantiate(_slotPrefab, _content);
                RoomSlot roomSlot = button.GetComponent<RoomSlot>();
                if (roomSlot != null)
                {
                    UpdateButton(roomSlot, roomInfo, _onJoinRoom);
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
