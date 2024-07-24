using System;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;

namespace MenuUI.Scripts.Lobby.InLobby
{
    public class LobbyRoomListingView : MonoBehaviour
    {
        [SerializeField] private GameObject _roomButtonPrefab;
        [SerializeField] private Transform _buttonParent;

        public Action RoomButtonOnClick
        {
            set { _roomButtonPrefab.GetComponentInChildren<Button>().onClick.AddListener(() => value()); }
        }

        public void Reset()
        {
            DeleteExtraButtons(_buttonParent);
        }

        public void UpdateStatus(List<RoomInfo> rooms, Action<string> onJoinRoom)
        {
            // Synchronize button count with room count.
            int roomsCount = rooms.Count;
            int slotCount = _buttonParent.childCount;
            for (int i = 0; i < slotCount; ++i)
            {
                if (roomsCount <= i) break;
                Transform buttonslot = _buttonParent.GetChild(i);
                if (_buttonParent.GetChild(i).childCount == 0)
                {
                    AddButton(_buttonParent.GetChild(i), _roomButtonPrefab);
                }
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
                        buttonObject.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
        }

        private static void AddButton(Transform parent, GameObject template)
        {
            GameObject instance = Instantiate(template, parent);
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
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
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
                var child = parent.GetChild(i).GetChild(0).gameObject;
                Destroy(child);
            }
        }
    }
}
