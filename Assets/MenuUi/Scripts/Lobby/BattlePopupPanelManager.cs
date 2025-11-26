using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Lobby;
using MenuUi.Scripts.Lobby.CreateRoom;
using MenuUi.Scripts.Signals;
using UnityEngine;

/// <summary>
/// Handles switching Battle Popup panels to a battle room and back to the main panel.
/// </summary>
public class BattlePopupPanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _topPanel;
    [SerializeField] private GameObject _border;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _createCustomRoom;
    [SerializeField] private GameObject _custom2v2WaitingRoom;
    [SerializeField] private GameObject _clanAndRandom2v2WaitingRoom;
    [SerializeField] private MatchmakingPanel _matchmakingPanel;

    private void OnEnable()
    {
        LobbyManager.OnMatchmakingRoomEntered += SwitchToMatchmakingPanel;
        SignalBus.OnCustomRoomSettingsRequested += OpenCustomRoomSettings;
    }

    private void OnDisable()
    {
        LobbyManager.OnMatchmakingRoomEntered -= SwitchToMatchmakingPanel;
        SignalBus.OnCustomRoomSettingsRequested -= OpenCustomRoomSettings;
    }

    public void SwitchRoom(GameType gameType)
    {
        ClosePanels();

        switch (gameType)
        {
            case GameType.Custom:
                SwitchCustomRoom(CustomGameMode.TwoVersusTwo);
                break;
            case GameType.Clan2v2:
                _clanAndRandom2v2WaitingRoom.SetActive(true);
                break;
            case GameType.Random2v2:
                _clanAndRandom2v2WaitingRoom.SetActive(true);
                break;
        }
    }

    public void OpenCustomRoomSettings()
    {
        ClosePanels();
        _createCustomRoom.SetActive(true);
    }

    private void SwitchCustomRoom(CustomGameMode mode)
    {
        switch (mode)
        {
            case CustomGameMode.TwoVersusTwo:
                _custom2v2WaitingRoom.SetActive(true);
                break;
            default:
                _mainPanel.SetActive(true);
                break;
        }
    }

    private void SwitchToMatchmakingPanel(bool isLeader)
    {
        ClosePanels();
        _matchmakingPanel.SetCancelButton(isLeader);
        _matchmakingPanel.gameObject.SetActive(true);
    }

    public void ClosePanels()
    {
        foreach (Transform t in transform)
        {
            if (ReferenceEquals(t.gameObject, _topPanel)) continue;
            if (ReferenceEquals(t.gameObject, _border)) continue;
            t.gameObject.SetActive(false);
        }
    }

    public void ReturnToMain()
    {
        if (PhotonRealtimeClient.InRoom)
        {
            return;
        }

        ClosePanels();
        _mainPanel.SetActive(true);
    }
}
