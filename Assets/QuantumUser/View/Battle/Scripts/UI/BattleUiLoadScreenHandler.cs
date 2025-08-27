using UnityEngine;
using Quantum;
using TMPro;

using MenuUi.Scripts.Lobby.SelectedCharacters;
using Battle.QSimulation.Game;

namespace Battle.View.UI
{
    public class BattleUiLoadScreenHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private BattlePopupCharacterSlotController[] _characterSlotControllers;
        [SerializeField] private TextMeshProUGUI[] _playerNames;

        public void PlayerConnected(BattlePlayerSlot playerSlot, int[] characterIds)
        {
            int slotIndex = playerSlot switch
            {
                BattlePlayerSlot.Slot1 => 0,
                BattlePlayerSlot.Slot2 => 1,
                BattlePlayerSlot.Slot3 => 2,
                BattlePlayerSlot.Slot4 => 3,
                _ => -1,
            };

            if (slotIndex == -1) return;

            _characterSlotControllers[slotIndex].gameObject.SetActive(true);
            _characterSlotControllers[slotIndex].SetCharacters(characterIds);

            _playerNames[(int)playerSlot - 1].alpha = 1;
        }

        public void Show(BattleParameters.PlayerType[] playerSlotTypes, FixedArray<QString64> playerNames)
        {
            for (int i = 0; i < playerSlotTypes.Length; i++)
            {
                if (playerSlotTypes[i] == BattleParameters.PlayerType.Player)
                {
                    _playerNames[i].text = playerNames[i];
                    _playerNames[i].gameObject.SetActive(true);
                }
            }
        }

        public void Hide()
        {
            _loadingScreen.SetActive(false);
        }
    }
}
