using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Scripts.InLobby
{
    /// <summary>
    /// Shows list of (open/closed) rooms and buttons for creating a new room or joining existing one.
    /// </summary>
    public class LobbyRoomListing : MonoBehaviour
    {
        [SerializeField] private Button _templateButton;
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private PhotonRoomList _photonRoomList;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _templateButton.onClick.AddListener(CreateRoomForMe);
        }

        private void OnEnable()
        {
            if (PhotonNetwork.InLobby)
            {
                UpdateStatus();
            }
            _photonRoomList.roomsUpdated += UpdateStatus;
        }

        private void OnDisable()
        {
            _photonRoomList.roomsUpdated -= UpdateStatus;
            DeleteExtraButtons(_buttonParent);
        }

        private void UpdateStatus()
        {
            if (!PhotonNetwork.InLobby)
            {
                DeleteExtraButtons(_buttonParent);
                return;
            }
            var rooms = _photonRoomList.currentRooms.ToList();
            rooms.Sort((a, b) =>
            {
                // First open rooms by name, then closed (aka playing) rooms by name
                var result = a.IsOpen.CompareTo(b.IsOpen);
                return result == 0
                    ? 0
                    : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
            Debug.Log($"updateStatus enter {PhotonNetwork.NetworkClientState} buttons: {_buttonParent.childCount} rooms: {rooms.Count}");

            // Synchronize button count with room count.
            while (_buttonParent.childCount < rooms.Count)
            {
                AddButton(_buttonParent, _templateButton);
            }
            // Update button captions
            for (var i = 0; i < rooms.Count; ++i)
            {
                var room = rooms[i];
                var buttonObject = _buttonParent.GetChild(i).gameObject;
                buttonObject.SetActive(true);
                var button = buttonObject.GetComponent<Button>();
                UpdateButton(button, room);
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
            Debug.Log($"updateStatus exit {PhotonNetwork.NetworkClientState} buttons: {_buttonParent.childCount} rooms: {rooms.Count}");
        }

        private static void CreateRoomForMe()
        {
            Debug.Log("createRoomForMe");
            PhotonLobby.createRoom($"Room{DateTime.Now.Second:00}");
        }

        private void JoinRoom(string roomName)
        {
            Debug.Log($"joinRoom '{roomName}'");
            var rooms = _photonRoomList.currentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name == roomName && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    PhotonLobby.joinRoom(roomInfo);
                }
            }
        }

        private static void AddButton(Transform parent, Button template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, parent);
            instance.SetActive(true);
        }

        private void UpdateButton(Button button, RoomInfo room)
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
            Debug.Log($"update '{text.text}' -> '{roomText}'");
            text.text = roomText;
            button.onClick.RemoveAllListeners();
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => JoinRoom(room.Name));
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