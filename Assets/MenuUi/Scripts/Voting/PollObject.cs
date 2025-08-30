using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts.Voting;

public class PollObject : MonoBehaviour
{
    private string pollId;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI UpperText;
    [SerializeField] private TextMeshProUGUI LowerText;
    [SerializeField] private TextMeshProUGUI YesVotesText;
    [SerializeField] private TextMeshProUGUI NoVotesText;
    [SerializeField] private TextMeshProUGUI TimeLeftText;

    [Header("Images")]
    [SerializeField] private Image Clock;
    [SerializeField] private Image Image;
    [SerializeField] private Image GreenFill;
    [SerializeField] private Image Background;

    [Header("Buttons")]
    [SerializeField] private Button ClockButton;

    [Header("Debug")]
    [SerializeField] private Button debugButton;

    [Header("PlayerHeads")]
    [SerializeField] private AddPlayerHeads playerHeads;

    private PollData pollData;
    private bool showEndTimeManually = false;
    private Coroutine updateCoroutine;
    private bool pollEnded = false;

    private void Start()
    {
        ClockButton.onClick.AddListener(OnClockButtonClicked);


        if (pollData != null)
        {
            updateCoroutine = StartCoroutine(UpdateValues());
        }
    }

    private void OnEnable()
    {
        VotingActions.ReloadPollList += SetValues;

        if (pollData != null && updateCoroutine == null && !pollEnded)
        {
            updateCoroutine = StartCoroutine(UpdateValues());
        }
    }

    private void OnDisable()
    {
        VotingActions.ReloadPollList -= SetValues;
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
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
                pollEnded = true;
                showEndTimeManually = true;
                ClockButton.interactable = false;

                // Convert poll end time to local time
                DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).ToLocalTime();

                // Format and show local time. Example: "20.6. 13:50"
                TimeLeftText.text = endDateTime.ToString("d.M. HH:mm");

                PollManager.EndPoll(pollId);
                SetResultColor();

                yield break;
            }

            Clock.fillAmount = 1 - (float)(secondsLeft) / totalDuration;
            UpdateClockDisplay(secondsLeft);

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateClockDisplay(long secondsLeft = -1)
    {
        if (pollData == null) return;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (secondsLeft == -1)
        {
            secondsLeft = pollData.EndTime - currentTime;
        }

        if (pollEnded || showEndTimeManually)
        {
            DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(pollData.EndTime).ToLocalTime();
            TimeLeftText.text = endDateTime.ToString("d.M. HH:mm");
        }
        else
        {
            // Display the remaining time, requires the "left" to read properly in-game
            if (secondsLeft < 60) TimeLeftText.text = secondsLeft + "s\nleft";
            else if (secondsLeft < 3600) TimeLeftText.text = (secondsLeft / 60) + "m\nleft";
            else TimeLeftText.text = (secondsLeft / 3600) + "h\nleft";
        }
    }

    private bool PollPassed()
    {
        return pollData.YesVotes.Count > pollData.NoVotes.Count;
    }

    private void SetValues()
    {
        // Update UI for FurniturePollData type polls
        if (pollData is FurniturePollData furniturePollData)
        {
            Image.sprite = furniturePollData.Sprite;
            UpperText.text = Enum.GetName(typeof(FurniturePollType), furniturePollData.FurniturePollType);
            LowerText.text = furniturePollData.Furniture?.Value.ToString() ?? "N/A";
        }

        if (YesVotesText != null) YesVotesText.text = pollData.YesVotes.Count.ToString();
        if (NoVotesText != null) NoVotesText.text = pollData.NoVotes.Count.ToString();

        if (GreenFill != null)
        {
            if (pollData.YesVotes.Count == 0 && pollData.NoVotes.Count == 0)
                GreenFill.fillAmount = 0.5f;
            else
                GreenFill.fillAmount = (float)pollData.YesVotes.Count / (pollData.NoVotes.Count + pollData.YesVotes.Count);
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
            // Start coroutine only if GameObject is active in hierarchy
            if (gameObject.activeInHierarchy)
            {
                if (updateCoroutine != null) StopCoroutine(updateCoroutine);
                pollEnded = false;
                showEndTimeManually = false;
                ClockButton.interactable = true;
                updateCoroutine = StartCoroutine(UpdateValues());
            }
        }
        else
        {
            pollEnded = true;
            showEndTimeManually = true;
            ClockButton.interactable = false;
            Clock.fillAmount = 1f;
            SetResultColor();
            UpdateClockDisplay();
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

    private void OnClockButtonClicked()
    {
        // Don't allow toggling if poll has ended
        if (pollEnded) return;

        showEndTimeManually = !showEndTimeManually;
        UpdateClockDisplay();
    }

    public void ShowPollInfoPopup()
    {
        if (pollData is FurniturePollData furniturePollData && furniturePollData.Furniture != null)
        {
            if (PollInfoPopup.Instance != null)
            {
                PollInfoPopup.Instance.OpenFurniturePopup(furniturePollData.Furniture);
            }
            else
            {
                Debug.LogWarning("PollInfoPopup instance is not found in the scene!");
            }
        }
        else
        {
            Debug.LogWarning("FurniturePollData or Furniture is null, cannot open PollInfoPopup.");
        }
    }


}
