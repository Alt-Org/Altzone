using System.Collections.Generic;
using MenuUi.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using static MenuUI.Scripts.Lobby.InLobby.InLobbyController;
using SignalBusPopup = MenuUI.Scripts.SignalBus;
using SignalBusInLobby = MenuUI.Scripts.Lobby.InLobby.SignalBus;
using MenuUi.Scripts.SwipeNavigation;

namespace MenuUi.Scripts.Lobby.BattleButton
{
    /// <summary>
    /// Attached to BT_ALTZONE prefab. Has logic related to selecting game type for opening the battle popup.
    /// </summary>
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
        private SwipeUI _swipe;


        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            _swipe.OnCurrentPageChanged += CloseGameTypeSelection;

            _gameTypeSelection.SetActive(false); // Close selection menu so that it's not open when game opens

            _button = GetComponent<Button>();
            _button.onClick.AddListener(RequestBattlePopup);

            // Instantiate game type option buttons to game type selection menu
            foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
            {
                GameTypeOption gameTypeOption = Instantiate(_gameTypeOptionPrefab).GetComponent<GameTypeOption>();
                gameTypeOption.SetInfo(gameTypeInfo);
                gameTypeOption.transform.SetParent(_gameTypeSelection.transform);
                gameTypeOption.transform.localScale = Vector3.one;
                _gameTypeOptions.Add(gameTypeOption);

                if (gameTypeInfo.gameType == GameType.Custom)
                {
                    UpdateGameType(gameTypeInfo);
                }
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

            _button.onClick.RemoveListener(RequestBattlePopup);
            _swipe.OnCurrentPageChanged -= CloseGameTypeSelection;
        }


        private void OnDisable()
        {
            CloseGameTypeSelection();
        }


        private void RequestBattlePopup()
        {
            SignalBusInLobby.OnBattlePopupRequestedSignal(_selectedGameType);
        }


        /// <summary>
        /// Toggle game type selection vertical layout active and not active.
        /// </summary>
        public void ToggleGameTypeSelection()
        {
            _gameTypeSelection.SetActive(!_gameTypeSelection.activeSelf);
        }


        /// <summary>
        /// Close game type selection vertical panel.
        /// </summary>
        public void CloseGameTypeSelection()
        {
            _gameTypeSelection.SetActive(false);
        }


        private void UpdateGameType(GameTypeInfo gameTypeInfo)
        {
            if (gameTypeInfo.gameType != GameType.Custom)
            {
                SignalBusPopup.OnChangePopupInfoSignal($"Pelimuotoa {gameTypeInfo.Name} ei voi vielä pelata.");
                return;
            }

            _gameTypeIcon.sprite = gameTypeInfo.Icon;
            _selectedGameType = gameTypeInfo.gameType;
        }
    }
}
