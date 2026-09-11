using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.CreateRoom
{
    /// <summary>
    /// Used for selecting the game mode when creating a custom game from the Battle Popup.
    /// </summary>
    public class CustomBattleGameModeSelector : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _currentGameModeText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _previousButton;
        private List<GameType> _gameTypes = new();

        public GameType SelectedGameMode { get; private set; } = GameType.BattlePingPong;

        private void Awake()
        {
            InitializeGameTypes();
            _nextButton.onClick.AddListener(OnNextOptionClicked);
            _previousButton.onClick.AddListener(OnPreviousOptionClicked);
            _currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnNextOptionClicked()
        {
            if (_gameTypes.Count <= 1) return;
            bool isLast = SelectedGameMode == _gameTypes.Last();
            SelectedGameMode = isLast ? _gameTypes.First() : _gameTypes[_gameTypes.IndexOf(SelectedGameMode) + 1];
            _currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnPreviousOptionClicked()
        {
            if (_gameTypes.Count <= 1) return;
            bool isFirst = SelectedGameMode == _gameTypes.First();
            SelectedGameMode = isFirst ? _gameTypes.Last() : _gameTypes[_gameTypes.IndexOf(SelectedGameMode) - 1];
            _currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void InitializeGameTypes()
        {
            _gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();
            _gameTypes.Remove(GameType.None);
            _gameTypes.Remove(GameType.Raid);
        }
    }
}
