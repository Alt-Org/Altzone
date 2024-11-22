using UnityEngine;
using UnityEngine.UI;

public class BattlePopupCreateCustomRoomPanel : MonoBehaviour
{
    [SerializeField] private CustomBattleGameModeSelector _modeSelector;
    [SerializeField] private Button _createRoomButton;

    [Header("Panels")]
    [SerializeField] private GameObject _custom2v2WaitingRoom;

    private void Start()
    {
        _createRoomButton.onClick.AddListener(CreateCustomRoom);
    }

    private void CreateCustomRoom()
    {
        switch (_modeSelector.SelectedGameMode)
        {
            case CustomGameMode.TwoVersusTwo:
                gameObject.SetActive(false);
                _custom2v2WaitingRoom.SetActive(true);
                break;
        }
    }
}
