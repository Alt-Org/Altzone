using UnityEngine;
using UnityEngine.UI;

public class BattlePopupPanelManager : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject _topPanel;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _custom2v2WaitingRoom;

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

    public void ReturnToMain()
    {
        foreach (Transform t in transform)
        {
            if (ReferenceEquals(t.gameObject, _topPanel)) continue;
            t.gameObject.SetActive(false);
        }
        _mainPanel.SetActive(true);
    }
}
