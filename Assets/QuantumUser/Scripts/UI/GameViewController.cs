using Quantum;
using QuantumUser.Scripts.UI;
using UnityEngine;

namespace QuantumUser.Scripts
{
    public class GameViewController : QuantumCallbacks
    {
        // References to UIviews
        [SerializeField] private GameUiController _gameUiController;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventUpdateDebugStatsOverlay>(this, OnUpdateDebugStatsOverlay);
        }

        private void OnUpdateDebugStatsOverlay(EventUpdateDebugStatsOverlay e)
        {
            _gameUiController.DebugStatsOverlay.SetShow(true);
            _gameUiController.DebugStatsOverlay.SetStats(e.Character);
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


                        break;
                    case GameState.GetReadyToPlay:
                        // Display "GO!" when the countdown reaches zero
                        _gameUiController.AnnouncementHandler.ShowEndOfCountDownText();
                        break;

                    case GameState.GameOver:
                        // If the game is over, display "Game Over!" and show the Game Over UI
                        _gameUiController.GameOverHandler.SetShow(true);
                        break;
                }
            }
        }

    }
}
