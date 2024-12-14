using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Voting;

public class PollObject : MonoBehaviour
{
    private PollData pollData;

    public void SetPollData(PollData newPollData)
    {
        pollData = newPollData;
    }

    public void CreatePollPopup()
    {
        VotingActions.CreatePollPopup?.Invoke(pollData);
    }
}
