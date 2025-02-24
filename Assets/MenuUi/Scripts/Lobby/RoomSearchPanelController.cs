using System;
using System.Collections;
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

                UpdateButton(button, roomInfo, _onJoinRoom);
            }
        }

        private void UpdateButton(GameObject slot, LobbyRoomInfo room, Action<string> onJoinRoom)
        {
            GameObject buttonObject = slot.transform.GetChild(0).gameObject;
            Button button;
            button = buttonObject.GetComponent<Button>();

            var roomText = $"{room.Name}";
            string playerCountText;
            if (roomText.Length > 21)
            {
                roomText = roomText.Substring(0, 20) + "…";
            }
            if (room.IsOpen)
            {
                playerCountText = $"Pelaajia {room.PlayerCount}/4";
                playerCountText = $"<color=blue>{playerCountText}</color>";
                button.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = $"Liity Huoneeseen";
            }
            else
            {
                playerCountText = $"Pelaajia {room.PlayerCount}/4";
                playerCountText = $"<color=brown>{playerCountText}</color>";
                button.transform.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = $"Peli käynnissä";
            }
            var roomNameText = buttonObject.transform.Find("InfoPanel").Find("Room name").GetComponent<TextMeshProUGUI>();
            Debug.Log($"update '{roomNameText.text}' -> '{roomText}' for {room}");
            roomNameText.text = roomText;
            var playerCountLabel = buttonObject.transform.Find("InfoPanel").Find("Player count").GetComponent<TextMeshProUGUI>();
            Debug.Log($"update '{playerCountLabel.text}' -> '{playerCountText}' for {room}");
            playerCountLabel.text = playerCountText;

            button.onClick.RemoveAllListeners();
            button.interactable = room.IsOpen;
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => onJoinRoom(room.Name));
            }
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
