using MenuUi.Scripts.Lobby;
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
    [SerializeField] private GameObject _clan2v2WaitingRoom;

    public void SwitchRoom()
    {
        SwitchCustomRoom(CustomGameMode.TwoVersusTwo);
    }

    private void SwitchCustomRoom(CustomGameMode mode)
    {
        foreach(Transform t in transform)
        {
            if (ReferenceEquals(t.gameObject,_topPanel)) continue;
            t.gameObject.SetActive(false);
        }

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

    private void ClosePanels()
    {
        foreach (Transform t in transform)
        {
            if (ReferenceEquals(t.gameObject, _topPanel)) continue;
            t.gameObject.SetActive(false);
        }
    }

    public void ReturnToMain()
    {
        ClosePanels();
        _mainPanel.SetActive(true);
    }

    public void OpenPanel(GameType gameType)
    {
        if (_custom2v2WaitingRoom.activeSelf || _clan2v2WaitingRoom.activeSelf)
        {
            return;
        }

        switch (gameType)
        {
            case GameType.Custom:
                ReturnToMain();
                break;
            case GameType.Clan2v2:
                ClosePanels();
                _clan2v2WaitingRoom.SetActive(true);
                break;
            case GameType.Random2v2:
                break;
        }
    }
}
