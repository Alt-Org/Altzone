using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public class CreatePollPopup : MonoBehaviour
{
    public GameObject PollPopupPrefab;

    void OnEnable()
    {
        VotingActions.CreatePollPopup += CreatePopup;
    }

    void OnDisable()
    {

        VotingActions.CreatePollPopup -= CreatePopup;
    }

    public void CreatePopup(PollData pollData)
    {
        GameObject popup = Instantiate(PollPopupPrefab, transform);
    }
}
