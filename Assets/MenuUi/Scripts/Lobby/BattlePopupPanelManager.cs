using Altzone.Scripts.Lobby;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using MenuUi.Scripts.Lobby;
using MenuUi.Scripts.Lobby.CreateRoom;
using Altzone.Scripts.Battle.Photon;
using MenuUi.Scripts.Signals;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private Coroutine _delayedMatchCheckHolder;

    private void OnEnable()
    {
        LobbyManager.OnMatchmakingRoomEntered += SwitchToMatchmakingPanel;
        SignalBus.OnCustomRoomSettingsRequested += OpenCustomRoomSettings;
        WireMainPanelButtons();
        WireCreateRoomButtons();
    }

    private void OnDisable()
    {
        LobbyManager.OnMatchmakingRoomEntered -= SwitchToMatchmakingPanel;
        SignalBus.OnCustomRoomSettingsRequested -= OpenCustomRoomSettings;
    }

    public void SwitchRoom(GameType gameType)
    {
        ClosePanels();

        // If we're already in a matchmaking or queue room, prefer showing the matchmaking panel
        bool inMatchmakingOrQueue = false;
        try
        {
            if (PhotonRealtimeClient.InMatchmakingRoom) inMatchmakingOrQueue = true;
            var curr = PhotonRealtimeClient.LobbyCurrentRoom;
            if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey)) inMatchmakingOrQueue = true;
        }
        catch { }

        bool isLeader = PhotonRealtimeClient.LocalLobbyPlayer != null && PhotonRealtimeClient.LocalLobbyPlayer.IsMasterClient;

        string currRoomName = "<none>";
        try
        {
            var c = PhotonRealtimeClient.LobbyCurrentRoom;
            if (c != null) currRoomName = c.Name ?? "<unnamed>";
        }
        catch { }

        Debug.Log($"BattlePopupPanelManager.SwitchRoom: gameType={gameType}, inMatchmakingOrQueue={inMatchmakingOrQueue}, currRoom={currRoomName}, isLeader={isLeader}");

        switch (gameType)
        {
            case GameType.Custom:
                // If already in a room, prefer showing the correct custom waiting room based on room's custom game mode
                if (PhotonRealtimeClient.InRoom)
                {
                    try
                    {
                        int mode = PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<int>(Altzone.Scripts.Battle.Photon.PhotonBattleRoom.CustomGameModeKey, (int)CustomGameMode.TwoVersusTwo);
                        SwitchCustomRoom((CustomGameMode)mode);
                    }
                    catch
                    {
                        _mainPanel.SetActive(true);
                    }
                }
                else
                {
                    _mainPanel.SetActive(true);
                }
                break;
            case GameType.FriendLobby:
            case GameType.Clan2v2:
            case GameType.Random2v2:
                if (inMatchmakingOrQueue)
                {
                    SwitchToMatchmakingPanel(isLeader);
                }
                else
                {
                    _clanAndRandom2v2WaitingRoom.SetActive(true);
                    // Start a short delayed check to catch race where matchmaking join finishes shortly after popup opens
                    try
                    {
                        if (_delayedMatchCheckHolder != null) { StopCoroutine(_delayedMatchCheckHolder); _delayedMatchCheckHolder = null; }
                        _delayedMatchCheckHolder = StartCoroutine(DelayedMatchCheckCoroutine(isLeader));
                        Debug.Log("BattlePopupPanelManager.SwitchRoom: started delayed match check coroutine");
                    }
                    catch { }
                }
                break;
        }
    }

    private System.Collections.IEnumerator DelayedMatchCheckCoroutine(bool isLeader)
    {
        yield return new WaitForSeconds(0.15f);
        bool inMatchmakingOrQueue = false;
        try
        {
            if (PhotonRealtimeClient.InMatchmakingRoom) inMatchmakingOrQueue = true;
            var curr = PhotonRealtimeClient.LobbyCurrentRoom;
            if (curr != null && curr.GetCustomProperty<bool>(PhotonBattleRoom.IsQueueKey)) inMatchmakingOrQueue = true;
        }
        catch { }

        if (inMatchmakingOrQueue)
        {
            Debug.Log($"BattlePopupPanelManager.DelayedMatchCheckCoroutine: switching to matchmaking panel, isLeader={isLeader}");
            SwitchToMatchmakingPanel(isLeader);
        }

        _delayedMatchCheckHolder = null;
    }

    public void OpenCustomRoomSettings()
    {
        ClosePanels();
        // Ensure the create-room UI has its selectors initialized before showing
        if (_createCustomRoom != null)
        {
            var createComp = _createCustomRoom.GetComponent<MenuUi.Scripts.Lobby.CreateRoom.CreateRoomCustom>();
            if (createComp != null && !createComp.IsCustomRoomOptionsReady)
            {
                createComp.InitializeCustomRoomOptions();
            }
            WireCreateRoomButtons();
            _createCustomRoom.SetActive(true);
        }
    }

    private void WireMainPanelButtons()
    {
        if (_mainPanel == null)
        {
            return;
        }

        Button previewButton = FindButtonByName(_mainPanel.transform, "PreviewBattle_Button");
        if (previewButton != null)
        {
            previewButton.onClick.RemoveListener(OpenCustomRoomSettings);
            previewButton.onClick.AddListener(OpenCustomRoomSettings);

            TMP_Text previewText = previewButton.GetComponentInChildren<TMP_Text>(true);
            if (previewText != null)
            {
                previewText.text = "Esikatsele";
            }
        }
    }

    private void WireCreateRoomButtons()
    {
        if (_createCustomRoom == null) return;

        Button returnButton = FindButtonByName(_createCustomRoom.transform, "ReturnButton") ?? FindButtonByName(_createCustomRoom.transform, "Cancel_Button");
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(ReturnToMain);
            returnButton.onClick.AddListener(ReturnToMain);

            TMP_Text returnText = returnButton.GetComponentInChildren<TMP_Text>(true);
            if (returnText != null)
            {
                returnText.text = "Takaisin";
            }
        }

        // Ensure create button caption is localized
        Button createBtn = FindButtonByName(_createCustomRoom.transform, "CreateRoom_Button") ?? FindButtonByName(_createCustomRoom.transform, "Create_Button");
        if (createBtn != null)
        {
            TMP_Text createText = createBtn.GetComponentInChildren<TMP_Text>(true);
            if (createText != null)
            {
                createText.text = "Luo huone";
            }
        }
    }

    private static Button FindButtonByName(Transform root, string targetName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root)
        {
            if (child.name == targetName)
            {
                return child.GetComponent<Button>() ?? child.GetComponentInChildren<Button>(true);
            }

            Button nested = FindButtonByName(child, targetName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
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

    public void SwitchToMatchmakingPanel(bool isLeader)
    {
        Debug.Log($"BattlePopupPanelManager.SwitchToMatchmakingPanel: isLeader={isLeader}");
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
