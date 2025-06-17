using System.Collections;
using UnityEngine;
using Altzone.Scripts.Voting;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using System;

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

    [SerializeField] private UnityEngine.UI.Image Background;

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
        float totalDuration = pollData.EndTime - pollData.StartTime;

        while (true)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long secondsLeft = pollData.EndTime - currentTime;

            if (secondsLeft <= 0)
            {
                // Clamp fill amount and timer to zero
                Clock.fillAmount = 1f;
                TimeLeftText.text = "0s";

                // End the poll
                PollManager.EndPoll(pollId);

                // Stop updating after poll ends
                yield break;
            }

            Clock.fillAmount = 1 - (float)(secondsLeft) / totalDuration;

            if (secondsLeft < 60) TimeLeftText.text = secondsLeft + "s";
            else if (secondsLeft < 3600) TimeLeftText.text = (secondsLeft / 60) + "m";
            else TimeLeftText.text = (secondsLeft / 3600) + "h";

            yield return new WaitForSeconds(1);
        }
    }
    private bool PollPassed()
    {
        return pollData.YesVotes.Count > pollData.NoVotes.Count;
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
        pollData = PollManager.GetAnyPollData(pollId);
        if (pollData == null) return;

        SetValues();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < pollData.EndTime)
        {
            StartCoroutine(UpdateValues());
        }
        else
        {
            Clock.fillAmount = 1f;
            SetResultColor();
        }
    }

    private void SetResultColor()
    {
        // Set color based on if the poll passed or not
        if (PollPassed())
        {
            Background.color = new Color(0.4f, 1f, 0.4f, 0.4f);
        }
        else
        {
            Background.color = new Color(1f, 0.4f, 0.4f, 0.4f);
        }
    }

    public void PassPollId()
    {
        VotingActions.PassPollId?.Invoke(pollId);
    }
}
