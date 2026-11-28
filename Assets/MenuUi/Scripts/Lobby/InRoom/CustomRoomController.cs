using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Language;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>Presentation, start, timeout and leave behavior for the custom-room panel.</summary>
    public class CustomRoomController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _gameType;
        [SerializeField] private TextLanguageSelectorCaller _conflictText;
        [SerializeField] private List<Conflicts> _conflicts;
        [SerializeField] private UnityEngine.UI.Button _startGameButton;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private AttentionSpanHandler _readyPanel;

        private const float CustomRoomTimeoutSeconds = 300f;
        private bool _leavingRoom;
        public static event Action OnLeaveRoom;

        private void Awake()
        {
            if (_startGameButton != null) _startGameButton.onClick.AddListener(StartPlaying);
            if (_backButton != null) _backButton.onClick.AddListener(OnBackButtonPressed);
        }

        private void OnEnable()
        {
            _leavingRoom = false;
            InRoomPlayerSlot.OnActivateReadyPanel += OpenReadyPanel;
            if (_title != null || _gameType != null) StartCoroutine(SetRoomTitle());
            if (_conflictText != null) StartCoroutine(CycleConflicts());
            StartCoroutine(CustomRoomTimeoutRoutine());
        }

        private void OnDisable()
        {
            InRoomPlayerSlot.OnActivateReadyPanel -= OpenReadyPanel;
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            if (_startGameButton != null) _startGameButton.onClick.RemoveListener(StartPlaying);
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackButtonPressed);
        }

        private void StartPlaying()
        {
            if (!PhotonRealtimeClient.InRoom || !PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient) return;
            if (_startGameButton != null) _startGameButton.interactable = false;
            this.Publish(new LobbyManager.StartPlayingEvent());
        }

        private void OpenReadyPanel()
        {
            if (_readyPanel != null) _readyPanel.OpenPanel();
        }

        private IEnumerator CustomRoomTimeoutRoutine()
        {
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);
            string roomName = PhotonRealtimeClient.LobbyCurrentRoom.Name;
            yield return new WaitForSecondsRealtime(CustomRoomTimeoutSeconds);
            if (!PhotonRealtimeClient.InRoom || PhotonRealtimeClient.LobbyCurrentRoom.Name != roomName) yield break;
            if (_startGameButton != null && !_startGameButton.interactable) yield break;
            yield return GoBack();
        }

        private void OnBackButtonPressed()
        {
            StartCoroutine(GoBack());
        }

        private IEnumerator GoBack()
        {
            if (_leavingRoom) yield break;
            _leavingRoom = true;
            if (PhotonRealtimeClient.InRoom) PhotonRealtimeClient.LeaveRoom();
            yield return new WaitUntil(() => !PhotonRealtimeClient.InRoom);
            OnLeaveRoom?.Invoke();
        }

        private IEnumerator SetRoomTitle()
        {
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);
            // Getting room name either from custom properties or from the room's name itself.
            string roomName = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<string>(PhotonBattleRoom.VisibleRoomNameKey);
            bool testRoom = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<bool>(PhotonLobbyRoom.TestModeKey);
            string gameType = ((GameType)PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey)).GetString();
            if (string.IsNullOrEmpty(roomName)) roomName = PhotonRealtimeClient.LobbyCurrentRoom.Name;
            if (_title != null) _title.text = testRoom ? $"{roomName} (test)" : roomName;
            if (_gameType != null) _gameType.text = gameType;
        }

        private IEnumerator CycleConflicts()
        {
            if (_conflicts == null || _conflicts.Count == 0) yield break;
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);
            while (PhotonRealtimeClient.InRoom)
            {
                int selectedConflict = Random.Range(0, _conflicts.Count);
                _conflictText.SetText(_conflicts[selectedConflict].ConlictText);
                yield return new WaitForSecondsRealtime(7);
            }
        }
    }
    [Serializable]
    public class Conflicts
    {
        [SerializeField, TextArea(1, 5)] private string _finnishConflictText;
        [SerializeField, TextArea(1, 5)] private string _englishConflictText;

        public string ConlictText
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish: return _finnishConflictText;
                    case SettingsCarrier.LanguageType.English: return _englishConflictText;
                    default: return _finnishConflictText;
                }
            }
        }
    }
}
