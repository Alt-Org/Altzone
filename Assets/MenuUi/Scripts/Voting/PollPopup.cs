using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Voting;
using TMPro;
using UnityEngine;

public class PollPopup : MonoBehaviour
{
    private string pollId;

    [SerializeField] private UnityEngine.UI.Image image;
    [SerializeField] private TextMeshProUGUI yesVotesText;
    [SerializeField] private TextMeshProUGUI noVotesText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI votesLeftText;
    [SerializeField] private UnityEngine.UI.Image greenFillAmount;
    [SerializeField] private AddPlayerHeads playerHeads;

    void OnEnable()
    {
        VotingActions.PassPollId += SetPollId;
        VotingActions.PollPopupReady?.Invoke();
    }

    void OnDisable()
    {
        VotingActions.PassPollId -= SetPollId;
    }

    public void SetPollId(string newPollId)
    {
        pollId = newPollId;
        Debug.Log("PollId Set: " + pollId);

        SetValues();
    }

    private void SetValues()
    {
        PollData pollData = PollManager.GetPollData(pollId);

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

        if (pollData is FurniturePollData)
        {
            FurniturePollData furniturePollData = (FurniturePollData)pollData;

            if (valueText != null) valueText.text = "Value: " + furniturePollData.Furniture.Value.ToString();
        }

        playerHeads.InstantiateHeads(pollId);
    }

    public void AddVote(bool answer)
    {
        PollData pollData = PollManager.GetPollData(pollId);
        pollData.AddVote(answer);
        SetValues();
        VotingActions.ReloadPollList?.Invoke();
    }
}
