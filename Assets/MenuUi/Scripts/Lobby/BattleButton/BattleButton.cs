using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    public class BattleButton : MonoBehaviour
    {
        [SerializeField] private Image _gameTypeIcon;
        [SerializeField] private GameObject _gameTypeSelection;

        private Button[] _gameTypeButtons;


        private void Awake()
        {
            _gameTypeButtons = _gameTypeSelection.GetComponentsInChildren<Button>();

            foreach (Button button in _gameTypeButtons)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ToggleGameTypeSelection);
            }
        }


        public void ToggleGameTypeSelection()
        {
            _gameTypeSelection.SetActive(!_gameTypeSelection.activeSelf);
        }
    }
}
