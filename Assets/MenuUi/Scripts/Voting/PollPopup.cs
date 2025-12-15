using System.Collections.Generic;
using Altzone.Scripts.Voting;
using TMPro;
using UnityEngine;
using System;

public class PollPopup : MonoBehaviour // Controls the popup display for polls
{
    private string pollId;
    private PollData pollData;
    private List<string> objectInfo = new List<string>();

    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private UnityEngine.UI.Image image;
    [SerializeField] private TextMeshProUGUI yesVotesText;
    [SerializeField] private TextMeshProUGUI noVotesText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI votesLeftText;
    [SerializeField] private UnityEngine.UI.Image greenFillAmount;
    [SerializeField] private AddPlayerHeads playerHeads;

    public void SetPollId(string newPollId)
    {
        pollId = newPollId;
        pollData = PollManager.GetPollData(pollId);
        if (pollData == null) return;

        SetValues();
    }

    private void SetValues()
    {
        // Populate based on the furniture info
        if (pollData is FurniturePollData)
        {
            FurniturePollData furniturePollData = (FurniturePollData)pollData;
            if (headerText != null) headerText.text = Enum.GetName(typeof(FurniturePollType), furniturePollData.FurniturePollType);

            objectInfo.Clear();

            objectInfo.Add(furniturePollData.Furniture.FurnitureInfo.VisibleName.ToString());
            objectInfo.Add("Value: " + furniturePollData.Furniture.Value.ToString());
            objectInfo.Add("Set Name: " + furniturePollData.Furniture.FurnitureInfo.SetName.ToString());

            infoText.text = string.Join("\n", objectInfo);
        }

        if (image != null) image.sprite = pollData.Sprite;
        if (yesVotesText != null) yesVotesText.text = pollData.YesVotes.Count.ToString();
        if (noVotesText != null) noVotesText.text = pollData.NoVotes.Count.ToString();

        if (votesLeftText != null) votesLeftText.text = (pollData.YesVotes.Count + pollData.NoVotes.Count).ToString()
                                                        + "/" +
                                                        (pollData.YesVotes.Count + pollData.NoVotes.Count + pollData.NotVoted.Count).ToString();

        if (greenFillAmount != null)
        {
            if (pollData.YesVotes.Count == 0 && pollData.NoVotes.Count == 0) greenFillAmount.fillAmount = 0.5f;
            else greenFillAmount.fillAmount = (float)pollData.YesVotes.Count / (pollData.NoVotes.Count + pollData.YesVotes.Count);
        }
    }

    public void AddVote(bool answer)
    {
        pollData.AddVote(answer, null);
        SetValues();
        VotingActions.ReloadPollList?.Invoke();

        gameObject.SetActive(false);

        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }
}
