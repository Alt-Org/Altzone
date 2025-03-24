using Altzone.Scripts.Lobby;
using UnityEngine;

/// <summary>
/// Handles switching Battle Popup panels to a battle room and back to the main panel.
/// </summary>
public class BattlePopupPanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _topPanel;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _custom2v2WaitingRoom;
    [SerializeField] private GameObject _clanAndRandom2v2WaitingRoom;
    [SerializeField] private GameObject _matchmakingPanel;

    public void SwitchRoom(GameType gameType)
    {
        switch (gameType)
        {
            case GameType.Custom:
                SwitchCustomRoom(CustomGameMode.TwoVersusTwo);
                break;
            case GameType.Clan2v2:
                ClosePanels();
                _clanAndRandom2v2WaitingRoom.SetActive(true);
                break;
            case GameType.Random2v2:
                _clanAndRandom2v2WaitingRoom.SetActive(true);
                break;
        }
    }

    private void SwitchCustomRoom(CustomGameMode mode)
    {
        ClosePanels();

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

    public void ClosePanels()
    {
        foreach (Transform t in transform)
        {
            if (ReferenceEquals(t.gameObject, _topPanel)) continue;
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
