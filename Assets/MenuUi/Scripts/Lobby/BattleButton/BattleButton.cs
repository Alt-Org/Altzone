using UnityEngine;
using UnityEngine.UI;
using GameType = MenuUI.Scripts.Lobby.InLobby.InLobbyController;
using SignalBus = MenuUI.Scripts.Lobby.InLobby.SignalBus;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    [RequireComponent(typeof(Button))]
    public class BattleButton : MonoBehaviour
    {
        [SerializeField] private Image _gameTypeIcon;
        [SerializeField] private GameObject _gameTypeSelection;

        private GameTypeOption[] _gameTypeOptions;
        private Button _button;

        private void Awake()
        {
            _gameTypeOptions = _gameTypeSelection.GetComponentsInChildren<GameTypeOption>();

            for (int i = 0; i < _gameTypeOptions.Length; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptions[i];
                gameTypeOption.ButtonComponent.onClick.AddListener(ToggleGameTypeSelection);
                gameTypeOption.OnGameTypeOptionSelected += UpdateGameType;
            }
        }


        private void OnDestroy()
        {
            for (int i = 0; i < _gameTypeOptions.Length; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptions[i];
                gameTypeOption.ButtonComponent.onClick.RemoveListener(ToggleGameTypeSelection);
                gameTypeOption.OnGameTypeOptionSelected -= UpdateGameType;
            }
        }


        /// <summary>
        /// Toggle game type selection vertical layout active and not active.
        /// </summary>
        public void ToggleGameTypeSelection()
        {
            _gameTypeSelection.SetActive(!_gameTypeSelection.activeSelf);
        }


        private void UpdateGameType(Sprite gameTypeSprite, GameType gameType)
        {
            _gameTypeIcon.sprite = gameTypeSprite;
            
        }
    }
}
