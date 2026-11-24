using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.CreateRoom
{
    public class PlayerCountSelector : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _currentGameModeText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _previousButton;
        private List<PlayerCount> _playerCountFlipper = new();
        private List<PlayerCount> _playerCountPingPong = new();
        private List<PlayerCount> _playerCountListInUse;
        private PlayerCount _playerCount;


        public PlayerCount PlayerCount
        {
            get => _playerCount;
            private set
            {
                _playerCount = value;
                _currentGameModeText.text = _playerCount.GetString();
            }
        }

        private void Awake()
        {
            InitializeGameTypes();
            OnGameModeChange(CustomBattleGameModeSelector.SelectedGameMode);
            _nextButton.onClick.AddListener(OnNextOptionClicked);
            _previousButton.onClick.AddListener(OnPreviousOptionClicked);
            _currentGameModeText.text = PlayerCount.GetString();
            CustomBattleGameModeSelector.OnGameModeChange += OnGameModeChange;
        }

        private void OnDestroy()
        {
            CustomBattleGameModeSelector.OnGameModeChange -= OnGameModeChange;
        }

        private void OnNextOptionClicked()
        {
            if (_playerCountListInUse.Count <= 1) return;
            bool isLast = PlayerCount == _playerCountListInUse.Last();
            PlayerCount = isLast ? _playerCountListInUse.First() : _playerCountListInUse[_playerCountListInUse.IndexOf(PlayerCount) + 1];
        }

        private void OnPreviousOptionClicked()
        {
            if (_playerCountListInUse.Count <= 1) return;
            bool isFirst = PlayerCount == _playerCountListInUse.First();
            PlayerCount = isFirst ? _playerCountListInUse.Last() : _playerCountListInUse[_playerCountListInUse.IndexOf(PlayerCount) - 1];
        }

        private void OnGameModeChange(GameType type)
        {
            if (type is GameType.BattleFlipperGame) _playerCountListInUse = _playerCountFlipper;
            else _playerCountListInUse = _playerCountPingPong;

            if (_playerCountListInUse.Contains(PlayerCount)) return;

            if (type is GameType.BattleFlipperGame) PlayerCount = PlayerCount.Two;
            else PlayerCount = PlayerCount.Four;
        }

        private void InitializeGameTypes()
        {
            _playerCountFlipper = Enum.GetValues(typeof(PlayerCount)).Cast<PlayerCount>().ToList();
            _playerCountFlipper.Remove(PlayerCount.Four);
            _playerCountPingPong = Enum.GetValues(typeof(PlayerCount)).Cast<PlayerCount>().ToList();
        }
    }
}
