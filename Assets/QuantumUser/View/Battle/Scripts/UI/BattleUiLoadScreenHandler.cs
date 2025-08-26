using System.Collections;
using Battle.QSimulation.Game;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using Quantum;
using TMPro;
using UnityEngine;

public class BattleUiLoadScreenHandler : MonoBehaviour
{
    [SerializeField]
    private BattlePopupCharacterSlotController[] _characterSlotControllers;
    [SerializeField]
    private TextMeshProUGUI[] _playerNames;

    public void PlayerConnected(BattlePlayerSlot playerSlot, int[] characterIds)
    {
        if (playerSlot == BattlePlayerSlot.Guest || playerSlot == BattlePlayerSlot.Spectator)
        {
            return;
        }

        _characterSlotControllers[(int)playerSlot - 1].gameObject.SetActive(true);
        _characterSlotControllers[(int)playerSlot - 1].SetCharacters(characterIds);

        _playerNames[(int)playerSlot - 1].alpha = 1;
    }

    public void Show(BattleParameters.PlayerType[] playerSlotTypes)
    {
        for (int i = 0; i < playerSlotTypes.Length; i++)
        {
            if (playerSlotTypes[i] == BattleParameters.PlayerType.Player)
            {
                _playerNames[i].gameObject.SetActive(true);
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
