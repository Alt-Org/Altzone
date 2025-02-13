using System.Collections.Generic;
using MenuUi.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using static MenuUI.Scripts.Lobby.InLobby.InLobbyController;
using SignalBus = MenuUI.Scripts.Lobby.InLobby.SignalBus;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    [RequireComponent(typeof(Button))]
    public class BattleButton : MonoBehaviour
    {
        [SerializeField] private Image _gameTypeIcon;
        [SerializeField] private GameObject _gameTypeSelection;
        [SerializeField] private GameObject _gameTypeOptionPrefab;
        [SerializeField] private GameTypeReference _gameTypeReference;

        private GameType _selectedGameType;

        private List<GameTypeOption> _gameTypeOptions = new();
        private Button _button;


        private void Awake()
        {
            _gameTypeSelection.SetActive(false);
        }


        private void Start()
        {
            foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
            {
                GameTypeOption gameTypeOption = Instantiate(_gameTypeOptionPrefab).GetComponent<GameTypeOption>();
                gameTypeOption.SetInfo(gameTypeInfo);
                gameTypeOption.transform.SetParent(_gameTypeSelection.transform);
                gameTypeOption.transform.localScale = Vector3.one;
                _gameTypeOptions.Add(gameTypeOption);
            }

            for (int i = 0; i < _gameTypeOptions.Count; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptions[i];
                gameTypeOption.ButtonComponent.onClick.AddListener(ToggleGameTypeSelection);
                gameTypeOption.OnGameTypeOptionSelected += UpdateGameType;
            }
        }


        private void OnDestroy()
        {
            for (int i = 0; i < _gameTypeOptions.Count; i++)
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


        private void UpdateGameType(GameTypeInfo gameTypeInfo)
        {
            _gameTypeIcon.sprite = gameTypeInfo.Icon;
            _selectedGameType = gameTypeInfo.gameType;
        }
    }
}
