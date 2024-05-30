using UnityEngine;
using TMPro;

public class VotingSystem : MonoBehaviour
{
    public TMP_Text voteResultText;
    private int yesVotes = 0;
    private int noVotes = 0;
    private bool votingActive = false;

    void Start()
    {
        // Initialize UI text
        voteResultText.text = "Äänestä Nyt!";
    }

    // Method to handle voting
    public void Vote(bool isYes)
    {
        if (votingActive)
        {
            if (isYes)
            {
                yesVotes++;
            }
            else
            {
                noVotes++;
            }

            // Update UI text
            UpdateVoteResultText();

            // Stop voting after a vote is cast
            StopVoting();
        }
    }

    // Method to start the voting process
    public void StartVoting()
    {
        // Reset votes
        yesVotes = 0;
        noVotes = 0;

        // Set voting active
        votingActive = true;

        // Update UI text
        UpdateVoteResultText();
    }

    // Method to stop the voting process
    private void StopVoting()
    {
        // Set voting inactive
        votingActive = false;

        // Update UI text
        voteResultText.text = "Voting ended. Results: Yes - " + yesVotes + ", No - " + noVotes;
    }

    // Method to update the vote result text
    private void UpdateVoteResultText()
    {
        voteResultText.text = "Yes: " + yesVotes + "\nNo: " + noVotes;
    }
}
