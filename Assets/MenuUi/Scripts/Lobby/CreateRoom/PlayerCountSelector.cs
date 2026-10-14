using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCountSelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentGameModeText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _previousButton;
    private List<PlayerCount> _gameTypes = new();

    public PlayerCount PlayerCount { get; private set; } = PlayerCount.Four;

    private void Awake()
    {
        InitializeGameTypes();
        _nextButton.onClick.AddListener(OnNextOptionClicked);
        _previousButton.onClick.AddListener(OnPreviousOptionClicked);
        _currentGameModeText.text = PlayerCount.GetString();
    }

    private void OnNextOptionClicked()
    {
        if (_gameTypes.Count <= 1) return;
        bool isLast = PlayerCount == _gameTypes.Last();
        PlayerCount = isLast ? _gameTypes.First() : _gameTypes[_gameTypes.IndexOf(PlayerCount) + 1];
        _currentGameModeText.text = PlayerCount.GetString();
    }

    private void OnPreviousOptionClicked()
    {
        if (_gameTypes.Count <= 1) return;
        bool isFirst = PlayerCount == _gameTypes.First();
        PlayerCount = isFirst ? _gameTypes.Last() : _gameTypes[_gameTypes.IndexOf(PlayerCount) - 1];
        _currentGameModeText.text = PlayerCount.GetString();
    }

    private void InitializeGameTypes()
    {
        _gameTypes = Enum.GetValues(typeof(PlayerCount)).Cast<PlayerCount>().ToList();
    }
}
