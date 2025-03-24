using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.CreateRoom
{
    /// <summary>
    /// This script helps with accessing player inputed room info when creating a custom game.
    /// </summary>
    public class CreateRoomCustom : MonoBehaviour
    {
        [SerializeField] TMP_InputField _roomName;
        [SerializeField] TMP_InputField _roomPassword;
        [SerializeField] Toggle _privateToggle;
        [SerializeField] Toggle _showToFriends;
        [SerializeField] Toggle _showToClan;
        [SerializeField] Button _createRoom;

        public string RoomName { get { return _roomName.text; } }
        public string RoomPassword { get { return _roomPassword.text; } }
        public bool IsPrivate { get { return _privateToggle.isOn; } }
        public bool ShowToFriends {  get { return _showToFriends.isOn; } }
        public bool ShowToClan { get {  return _showToClan.isOn; } }
        public Button CreateRoomButton { get { return _createRoom; } }

        private void OnEnable()
        {
            _roomName.text = "";
            _roomPassword.text = "";
            _privateToggle.isOn = false;
        }
    }
}

