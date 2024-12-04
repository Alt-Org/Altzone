using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Quantum;

public class InGameUIHandler : MonoBehaviour
{
    // Reference to the TextMeshProUGUI component that displays the countdown text
    [SerializeField] private TextMeshProUGUI countDownText;

    // Reference to the Game Over UI view to be displayed when the game ends
    public GameObject GameOverView;

    // Start is called before the first frame update
    // Currently not used but can be used for initialization if needed
    void Start()
    {

    }

    // Update is called once per frame
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
                countDownText.text = "GameSession singleton not found";
                return;
            }

            // Retrieve the GameSession singleton from the Quantum frame
            var gameSession = frame.GetSingleton<GameSession>();

            // Convert the countdown time to an integer for display
            int countDown = (int)gameSession.TimeUntilStart;

            // Handle different game states to update the UI
            switch (gameSession.state)
            {
                case GameState.Countdown:
                    // If the game is in the countdown state, display the countdown timer
                    countDownText.text = $"{countDown}";
                    break;

                case GameState.Playing:
                    // If the game is in the playing state:
                    if (countDown == 0)
                    {
                        // Display "GO!" when the countdown reaches zero
                        countDownText.text = "GO!";
                        break;
                    }

                    if (countDown < 0)
                    {
                        // Clear the countdown text when the countdown is negative
                        countDownText.text = "";
                    }

                    break;

                case GameState.GameOver:
                    // If the game is over, display "Game Over!" and show the Game Over UI
                    countDownText.text = "Game Over!";
                    GameOverView.SetActive(true);

                    break;
            }
        }
    }
}
