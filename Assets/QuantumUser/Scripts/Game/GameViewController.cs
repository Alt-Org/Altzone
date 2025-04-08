using Quantum;
using QuantumUser.Scripts.UI;
using UnityEngine;
using Altzone.Scripts.BattleUiShared;

using Button = UnityEngine.UI.Button;
using PlayerType = QuantumUser.Scripts.UI.Views.GameUiPlayerInfoHandler.PlayerType;

namespace QuantumUser.Scripts
{
    public class GameViewController : QuantumCallbacks
    {
        [SerializeField] private GridViewController _gridViewController;
        // References to UIviews
        [SerializeField] private GameUiController _gameUiController;
        [SerializeField] private ScreenEffectViewController _screenEffectViewController;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventViewInit>(this, OnViewInit);
            QuantumEvent.Subscribe<EventChangeEmotionState>(this, OnChangeEmotionState);
            QuantumEvent.Subscribe<EventUpdateDebugStatsOverlay>(this, OnUpdateDebugStatsOverlay);
        }

        private void OnViewInit(EventViewInit e)
        {
            if (_gridViewController != null)
            {
                _gridViewController.SetGrid();
            }

            if (_gameUiController.DiamondsHandler != null)
            {
                _gameUiController.DiamondsHandler.SetDiamondsText(0);
            }

            // Commented out code to hide the ui elements which shouldn't be shown at this point, but the code will be used later

            /*
            if (_gameUiController.GiveUpButtonHandler != null)
            {
                _gameUiController.GiveUpButtonHandler.GiveUpButton.onClick.AddListener(OnGiveUpButtonPressed);
                _gameUiController.GiveUpButtonHandler.SetShow(true);
            }

            if (_gameUiController.PlayerInfoHandler != null)
            {
                _gameUiController.PlayerInfoHandler.SetInfo(PlayerType.LocalPlayer, "Minä", new int[3] { 101, 201, 301 });

                Button[] characterButtons = _gameUiController.PlayerInfoHandler.GetLocalPlayerCharacterButtons();
                for (int i = 0; i < characterButtons.Length; i++)
                {
                    int buttonIdx = i;
                    characterButtons[i].onClick.AddListener(()=> OnCharacterButtonPressed(buttonIdx));
                }

                _gameUiController.PlayerInfoHandler.SetInfo(PlayerType.LocalPlayerTeammate, "Tiimiläinen", new int[3] { 401, 501, 601 });
            }
            */
        }

        private void OnChangeEmotionState(EventChangeEmotionState e)
        {
            if (!_screenEffectViewController.IsActive) _screenEffectViewController.SetActive(true);
            _screenEffectViewController.ChangeColor((int)e.Emotion);
        }

        private void OnUpdateDebugStatsOverlay(EventUpdateDebugStatsOverlay e)
        {
            _gameUiController.DebugStatsOverlay.SetShow(true);
            _gameUiController.DebugStatsOverlay.SetStats(e.Character);
        }

        private void OnGiveUpButtonPressed()
        {
            Debug.Log("Give up button pressed!");
        }

        private void OnCharacterButtonPressed(int buttonIdx)
        {
            Debug.Log($"Character button {buttonIdx} pressed!");
        }

        // Handles UI updates based on the game's state and countdown
        private void Update()
        {
            // Try to get the current Quantum frame
            if (Utils.TryGetQuantumFrame(out Frame frame))
            {
                // Try to retrieve the singleton entity reference for the GameSession
                if (frame.TryGetSingletonEntityRef<GameSession>(out var entity) == false)
                {
                    // If the GameSession singleton is not found, display an error message
                    Debug.LogError("GameSession singleton not found -- BattleUIHandler");
                    return;
                }

                // Retrieve the GameSession singleton from the Quantum frame
                GameSession gameSession = frame.GetSingleton<GameSession>();

                // Convert the countdown time to an integer for display
                int countDown = (int)gameSession.TimeUntilStart;

                // Handle different game states to update the UI
                switch (gameSession.state)
                {
                    case GameState.Countdown:
                        // If the game is in the countdown state, display the countdown timer
                        _gameUiController.AnnouncementHandler.SetCountDownNumber(countDown);
                        break;

                    case GameState.Playing:

                        // Clear the countdown text when the countdown is negative
                        _gameUiController.AnnouncementHandler.ClearAnnouncerTextField();
                        _gameUiController.TimerHandler.StartTimer(frame);

                        break;
                    case GameState.GetReadyToPlay:
                        // Display "GO!" when the countdown reaches zero
                        _gameUiController.AnnouncementHandler.ShowEndOfCountDownText();
                        break;

                    case GameState.GameOver:
                        _gameUiController.GiveUpButtonHandler.SetShow(false);
                        _gameUiController.TimerHandler.StopTimer();
                        // If the game is over, display "Game Over!" and show the Game Over UI
                        _gameUiController.GameOverHandler.SetShow(true);
                        break;
                }
            }
        }

    }
}
