using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Voting;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;

public class PollObject : MonoBehaviour
{
    private string pollId;

    [SerializeField] private UnityEngine.UI.Image Image;
    [SerializeField] private TextMeshProUGUI UpperText;
    [SerializeField] private TextMeshProUGUI LowerText;

    [SerializeField] private UnityEngine.UI.Image Clock;
    [SerializeField] private TextMeshProUGUI TimeLeftText;

    [SerializeField] private TextMeshProUGUI YesVotesText;
    [SerializeField] private TextMeshProUGUI NoVotesText;
    [SerializeField] private UnityEngine.UI.Image GreenFill;

    [SerializeField] private AddPlayerHeads playerHeads;

    PollData pollData;

    private void Start()
    {
        StartCoroutine(UpdateValues());
    }

    private void OnEnable()
    {
        VotingActions.ReloadPollList += SetValues;
    }

    private void OnDisable()
    {
        VotingActions.ReloadPollList -= SetValues;
    }

    private IEnumerator UpdateValues()
    {
        while (true)
        {
            Clock.fillAmount = 1 - (float)(pollData.EndTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) / (pollData.EndTime - pollData.StartTime);

            int secondsLeft = (int)(pollData.EndTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            int minutesLeft = secondsLeft / 60;
            int hoursLeft = minutesLeft / 60;

            if (secondsLeft < 60) TimeLeftText.text = (secondsLeft.ToString() + "s left");
            else if (minutesLeft < 60) TimeLeftText.text = (minutesLeft.ToString() + "m left");
            else TimeLeftText.text = (hoursLeft.ToString() + "h left");

            yield return new WaitForSeconds(1);
        }
    }

    private void SetValues()
    {
        if (pollData is FurniturePollData)
        { 
            FurniturePollData furniturePollData = (FurniturePollData)pollData;

            Image.sprite = furniturePollData.Sprite;

            UpperText.text = Enum.GetName(typeof(FurniturePollType), furniturePollData.FurniturePollType);

            LowerText.text = furniturePollData.Furniture.Value.ToString();
        }

        if (YesVotesText != null) YesVotesText.text = pollData.YesVotes.Count.ToString();
        if (NoVotesText != null) NoVotesText.text = pollData.NoVotes.Count.ToString();

        if (GreenFill != null)
        {
            if (pollData.YesVotes.Count == 0 && pollData.NoVotes.Count == 0) GreenFill.fillAmount = 0.5f;
            else GreenFill.fillAmount = (float)pollData.YesVotes.Count / (pollData.NoVotes.Count + pollData.YesVotes.Count);
        }

        playerHeads.InstantiateHeads(pollId);
    }

    public void SetPollId(string newPollId)
    {
        pollId = newPollId;
        pollData = PollManager.GetPollData(pollId);
        SetValues();
    }

    public void PassPollId()
    {
        VotingActions.PassPollId?.Invoke(pollId);
    }
}
