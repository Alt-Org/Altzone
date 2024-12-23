using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Voting;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using System;

public class PollObject : MonoBehaviour
{
    private bool selected;

    private string pollId;
    [SerializeField] private UnityEngine.UI.Image Image;
    [SerializeField] private UnityEngine.UI.Image Clock;
    [SerializeField] private TextMeshProUGUI YesVotesText;
    [SerializeField] private TextMeshProUGUI NoVotesText;

    public void SetPollId(string newPollId)
    {
        pollId = newPollId;
        SetValues();
    }

    private void Update()
    {
        PollData pollData = PollManager.GetPollData(pollId);

        //Debug.Log(1 - (float)(pollData.EndTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) / (pollData.EndTime - pollData.StartTime));
        Clock.fillAmount = 1 - (float)(pollData.EndTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) / (pollData.EndTime - pollData.StartTime);
    }

    private void SetValues()
    {
        PollData pollData = PollManager.GetPollData(pollId);

        Image.sprite = pollData.Sprite;
        YesVotesText.text = pollData.YesVotes.Count.ToString();
        NoVotesText.text = pollData.NoVotes.Count.ToString();
    }

    public void SelectPollObject()
    {
        selected = true;
        VotingActions.PollPopupReady += PassPollId;
    }

    public void PassPollId()
    {
        if (selected)
        {
            VotingActions.PassPollId?.Invoke(pollId);
            selected = false;
            VotingActions.PollPopupReady -= PassPollId;
        }
    }
}
