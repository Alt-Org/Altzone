using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InLobby
{
    public class LobbyRoomListingView : MonoBehaviour
    {
        [SerializeField] private Button _roomButton;
        [SerializeField] private Transform _buttonParent;

        public Button RoomButton => _roomButton;
        
        public void Reset()
        {
            DeleteExtraButtons(_buttonParent);
        }

        public void UpdateStatus(List<RoomInfo> rooms, Action<string> onJoinRoom)
        {
            // Synchronize button count with room count.
            while (_buttonParent.childCount < rooms.Count)
            {
                AddButton(_buttonParent, _roomButton);
            }
            // Update button captions
            for (var i = 0; i < rooms.Count; ++i)
            {
                var room = rooms[i];
                var buttonObject = _buttonParent.GetChild(i).gameObject;
                buttonObject.SetActive(true);
                var button = buttonObject.GetComponent<Button>();
                UpdateButton(button, room, onJoinRoom);
            }
            // Hide extra lines
            if (_buttonParent.childCount > rooms.Count)
            {
                for (var i = rooms.Count; i < _buttonParent.childCount; ++i)
                {
                    var buttonObject = _buttonParent.GetChild(i).gameObject;
                    if (buttonObject.activeSelf)
                    {
                        buttonObject.SetActive(false);
                    }
                }
            }
        }
        
        private static void AddButton(Transform parent, Button template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, parent);
            instance.SetActive(true);
        }

        private void UpdateButton(Button button, RoomInfo room, Action<string> onJoinRoom)
        {
            var roomText = $"{room.Name}";
            if (roomText.Length > 21)
            {
                roomText = roomText.Substring(0, 20) + "…";
            }
            if (room.IsOpen)
            {
                roomText += $" ({room.PlayerCount} in room)";
                roomText = $"<color=blue>{roomText}</color>";
            }
            else
            {
                roomText += $" ({room.PlayerCount} playing)";
                roomText = $"<color=brown>{roomText}</color>";
            }
            var text = button.GetComponentInChildren<Text>();
            Debug.Log($"update '{text.text}' -> '{roomText}' for {room.GetDebugLabel()}");
            text.text = roomText;
            button.onClick.RemoveAllListeners();
            button.interactable = room.IsOpen;
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => onJoinRoom(room.Name));
            }
        }

        private static void DeleteExtraButtons(Transform parent)
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