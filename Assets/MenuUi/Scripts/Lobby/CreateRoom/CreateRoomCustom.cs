using Altzone.Scripts.Common;
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
        [SerializeField] private TMP_InputField _roomPassword;
        [SerializeField] private Toggle _privateToggle;
        [SerializeField] private Toggle _showToFriends;
        [SerializeField] private Toggle _showToClan;
        [SerializeField] private Button _createRoom;
        [SerializeField] private MapAndRoomNameSelector _mapAndRoomNameSelector;
        [SerializeField] private CustomBattleGameModeSelector _customBattleGameModeSelector;

        public string RoomName { get { return _mapAndRoomNameSelector.SelectedEmotionalSituation.SituationName; } }
        public Emotion SelectedEmotion { get { return _mapAndRoomNameSelector.SelectedEmotionalSituation.SituationEmotion; } }
        public string RoomPassword { get { return _roomPassword.text; } }
        public bool IsPrivate { get { return _privateToggle.isOn; } }
        public bool ShowToFriends {  get { return _showToFriends.isOn; } }
        public bool ShowToClan { get {  return _showToClan.isOn; } }
        public Button CreateRoomButton { get { return _createRoom; } }
        public CustomGameMode SelectedCustomGameMode { get { return _customBattleGameModeSelector.SelectedGameMode; } }

        private void OnEnable()
        {
            _roomPassword.text = "";
            _privateToggle.isOn = false;
        }
    }
}

