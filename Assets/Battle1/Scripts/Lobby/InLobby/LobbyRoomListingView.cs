using System;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;
/*using RoomInfo = Battle1.PhotonRealtime.Code.RoomInfo;*/

namespace Battle1.Scripts.Lobby.InLobby
{
    public class LobbyRoomListingView : MonoBehaviour
    {
        [SerializeField] private GameObject _roomButtonPrefab;
        [SerializeField] private Button _roomCreateButton;
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private RoomSearchPanelController _searchPanelController;
        [SerializeField] private bool _oldDesign;
        private int _listPos = 0;

        public Action RoomButtonOnClick
        {
            set { _roomCreateButton.onClick.AddListener(() => value()); }
        }

        public void Reset()
        {
            DeleteExtraButtons(_buttonParent);
        }

        public void UpdateStatus(List<RoomInfo> rooms, Action<string> onJoinRoom)
        {
            _searchPanelController.SetOnJoinRoom(onJoinRoom);
            int lastPos = CheckListPosition(rooms);

            // Synchronize button count with room count.
            if (!_oldDesign)
            {
                int roomsCount = rooms.Count;
                int slotCount = _buttonParent.childCount;
                for (int i = _listPos; i <= lastPos; ++i)
                {
                    if (roomsCount <= i) break;
                    Transform buttonslot = _buttonParent.GetChild(i - _listPos);
                    if (buttonslot.childCount == 0)
                    {
                        GameObject button = AddButton(buttonslot, _roomButtonPrefab);

                        UpdateButton(button, rooms[i], onJoinRoom);
                    }
                }

            }
            else
            {
                while (_buttonParent.childCount < rooms.Count)
                {
                    AddButton(_buttonParent, _roomButtonPrefab);
                }
            }
            // Update button captions
            /*for (var i = 0; i < rooms.Count; ++i)
            {
                var room = rooms[i];
                var buttonObject = _buttonParent.GetChild(i).gameObject;
                buttonObject.SetActive(true);
                //var button = buttonObject.GetComponent<Button>();
                UpdateButton(buttonObject, room, onJoinRoom);
            }*/
            _searchPanelController.RoomsData = rooms;

            // Hide extra lines
            if (_buttonParent.childCount > rooms.Count)
            {
                for (var i = rooms.Count; i < _buttonParent.childCount; ++i)
                {
                    var buttonObject = _buttonParent.GetChild(i).gameObject;
                    if (buttonObject.transform.childCount > 0)
                    {
                        buttonObject.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
        }

        private static GameObject AddButton(Transform parent, GameObject template)
        {
            GameObject instance = Instantiate(template, parent);
            instance.SetActive(true);
            return instance;
        }

        private void UpdateButton(GameObject buttonObject, RoomInfo room, Action<string> onJoinRoom)
        {
            Button button;
            if (_oldDesign)
            button = buttonObject.GetComponent<Button>();
            else
            {
                button = buttonObject.GetComponent<Button>();

                //if(button == null) button = buttonObject.transform.Find("Button")?.GetComponent<Button>();
                if (button == null) button = buttonObject.transform.Find("Panel")?.GetComponent<Button>();
            }
            if (_oldDesign)
            {
                var roomText = $"{room.Name}";
                if (roomText.Length > 21)
                {
                    roomText = roomText.Substring(0, 20) + "…";
                }
                if (room.IsOpen)
                {
                    roomText += $" ({room.PlayerCount}/4 in room)";
                    roomText = $"<color=blue>{roomText}</color>";
                }
                else
                {
                    roomText += $" ({room.PlayerCount}/4 playing)";
                    roomText = $"<color=brown>{roomText}</color>";
                }
                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"update '{text.text}' -> '{roomText}' for {room.GetDebugLabel()}");
                text.text = roomText;
            }
            else
            {
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
                Debug.Log($"update '{roomNameText.text}' -> '{roomText}' for {room.GetDebugLabel()}");
                roomNameText.text = roomText;
                var playerCountLabel = buttonObject.transform.Find("InfoPanel").Find("Player count").GetComponent<TextMeshProUGUI>();
                Debug.Log($"update '{playerCountLabel.text}' -> '{playerCountText}' for {room.GetDebugLabel()}");
                playerCountLabel.text = playerCountText;
            }

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
                if (parent.GetChild(i).childCount == 0)
                {
                    continue;
                }
                var child = parent.GetChild(i).gameObject;
                if (child.GetComponent<Button>() == null) child = child.transform.GetChild(0).gameObject;
                Destroy(child);
            }
        }

        private int CheckListPosition(List<RoomInfo> rooms)
        {
            int lastPos;
            if (_listPos + 4 <= rooms.Count)
            {
                lastPos = _listPos + 3;
            }
            else
            {
                _listPos = rooms.Count - 4;
                if (_listPos < 0) _listPos = 0;
                lastPos = rooms.Count - 1;
            }
            return lastPos;
        }
    }
}
