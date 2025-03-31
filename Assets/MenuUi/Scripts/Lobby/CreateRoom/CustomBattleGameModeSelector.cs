using System;
using System.Linq;
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

        public CustomGameMode SelectedGameMode { get; private set; } = CustomGameMode.TwoVersusTwo;

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextOptionClicked);
            _previousButton.onClick.AddListener(OnPreviousOptionClicked);
            _currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnNextOptionClicked()
        {
            bool isLast = SelectedGameMode == Enum.GetValues(typeof(CustomGameMode)).Cast<CustomGameMode>().Last();
            SelectedGameMode = isLast ? 0 : SelectedGameMode + 1;
            _currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnPreviousOptionClicked()
        {
            bool isFirst = (int)SelectedGameMode <= 0;
            SelectedGameMode = isFirst ? Enum.GetValues(typeof(CustomGameMode)).Cast<CustomGameMode>().Last() : SelectedGameMode - 1;
            _currentGameModeText.text = SelectedGameMode.GetString();
        }
    }

    public enum CustomGameMode
    {
        OneVersusOne,
        TwoVersusTwo,
        Tournament
    }

    public static class CustomGameModeExtension
    {
        public static string GetString(this CustomGameMode gameMode)
        {
            return gameMode switch
            {
                CustomGameMode.OneVersusOne => "1v1",
                CustomGameMode.TwoVersusTwo => "2v2",
                CustomGameMode.Tournament => "Turnaus",
                _ => ""
            };
        }
    }
}
