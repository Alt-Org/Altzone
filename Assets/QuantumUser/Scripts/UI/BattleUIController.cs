using GameAnalyticsSDK.Setup;
using Quantum;
using QuantumUser.Scripts.UI;
using QuantumUser.Scripts.UI.Views;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuantumUser.Scripts
{
    public class BattleUIController : MonoBehaviour
    {
        // Reference to the TextMeshProUGUI component that displays the countdown text


        // References to UIviews
        public GameViewController gameViewController;



        // Start is called before the first frame update
        // Currently not used but can be used for initialization if needed
        void Start()
        {
        }


        // Handles UI updates based on the game's state and countdown
        void Update()
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
                        gameViewController.uiGameAnnouncementHandler.SetCountDownNumber(countDown);
                        break;

                    case GameState.Playing:

                        // Clear the countdown text when the countdown is negative
                        gameViewController.uiGameAnnouncementHandler.ClearAnnouncerTextField();


                        break;
                    case GameState.GetReadyToPlay:
                        // Display "GO!" when the countdown reaches zero
                        gameViewController.uiGameAnnouncementHandler.ShowEndOfCountDownText();
                        break;

                    case GameState.GameOver:
                        // If the game is over, display "Game Over!" and show the Game Over UI
                        gameViewController.GameOverView.SetActive(true);
                        break;
                }
            }
        }

    }
}
