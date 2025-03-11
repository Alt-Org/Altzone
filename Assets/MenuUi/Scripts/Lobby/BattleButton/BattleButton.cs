using System.Collections.Generic;
using MenuUi.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;
using SignalBusPopup = MenuUI.Scripts.SignalBus;
using MenuUi.Scripts.Signals;
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
        [SerializeField] private GameObject _gameTypeSelectionMenu;
        [SerializeField] private GameObject _gameTypeOptionPrefab;
        [SerializeField] private GameTypeReference _gameTypeReference;
        [SerializeField] private GameObject _touchBlocker;

        private GameType _selectedGameType;

        private List<GameTypeOption> _gameTypeOptionList = new();
        private Button _button;
        private SwipeUI _swipe;


        private void Awake()
        {
            _swipe = FindObjectOfType<SwipeUI>();
            _swipe.OnCurrentPageChanged += CloseGameTypeSelection;

            _gameTypeSelectionMenu.SetActive(false); // Close selection menu so that it's not open when game opens

            _button = GetComponent<Button>();
            _button.onClick.AddListener(RequestBattlePopup);

            // Instantiate game type option buttons to game type selection menu
            foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
            {
                GameTypeOption gameTypeOption = Instantiate(_gameTypeOptionPrefab).GetComponent<GameTypeOption>();
                bool selected = gameTypeInfo.gameType == _selectedGameType;
                gameTypeOption.SetInfo(gameTypeInfo, selected);
                gameTypeOption.transform.SetParent(_gameTypeSelectionMenu.transform);
                gameTypeOption.transform.localScale = Vector3.one;
                _gameTypeOptionList.Add(gameTypeOption);

                if (gameTypeInfo.gameType == GameType.Custom)
                {
                    UpdateGameType(gameTypeInfo);
                }
            }

            for (int i = 0; i < _gameTypeOptionList.Count; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptionList[i];
                gameTypeOption.ButtonComponent.onClick.AddListener(ToggleGameTypeSelection);
                gameTypeOption.OnGameTypeOptionSelected += UpdateGameType;
            }
        }


        private void OnDestroy()
        {
            for (int i = 0; i < _gameTypeOptionList.Count; i++)
            {
                GameTypeOption gameTypeOption = _gameTypeOptionList[i];
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
            if (_gameTypeSelectionMenu.activeSelf)
            {
                CloseGameTypeSelection();
            }
            else
            {
                SignalBus.OnBattlePopupRequestedSignal(_selectedGameType);
            }
        }


        /// <summary>
        /// Toggle game type selection vertical layout active and not active.
        /// </summary>
        public void ToggleGameTypeSelection()
        {
            _gameTypeSelectionMenu.SetActive(!_gameTypeSelectionMenu.activeSelf);
            _touchBlocker.SetActive(_gameTypeSelectionMenu.activeSelf);
        }


        /// <summary>
        /// Close game type selection vertical panel.
        /// </summary>
        public void CloseGameTypeSelection()
        {
            _gameTypeSelectionMenu.SetActive(false);
            _touchBlocker.SetActive(false);
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

            // Setting selected visuals for option buttons
            foreach (GameTypeOption gameTypeOption in _gameTypeOptionList) 
            {
                bool selected = gameTypeOption.Info.gameType == _selectedGameType;
                gameTypeOption.SetSelected(selected);
            }
        }
    }
}
