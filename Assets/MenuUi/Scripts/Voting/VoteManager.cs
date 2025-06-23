using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.UI;

public class VoteManager : MonoBehaviour
{
    [SerializeField] private GameObject VotedListContent;
    [SerializeField] private GameObject NotVotedListContent;
    [SerializeField] private GameObject PollObjectPrefab;
    [SerializeField] private GameObject PollPopup;
    [SerializeField] private GameObject Blocker;
    [SerializeField] private GameObject NoPollsText;

    private List<GameObject> Polls = new List<GameObject>();
    private PlayerData player;

    private void OnEnable()
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            player = data;
            InstantiatePolls();
        });

        VotingActions.ReloadPollList += InstantiatePolls;
        VotingActions.PassPollId += SetPollPopupPollId;
    }

    private void OnDisable()
    {
        VotingActions.ReloadPollList -= InstantiatePolls;
        VotingActions.PassPollId -= SetPollPopupPollId;
    }

    // For checking if the player has already voted
    private bool HasPlayerVoted(PollData pollData, string playerId)
    {
        return !pollData.NotVoted.Contains(playerId);
    }

    public void InstantiatePolls()
    {
        PollManager.LoadClanData();

        // Clear existing polls
        for (int i = 0; i < Polls.Count; i++)
        {
            GameObject obj = Polls[i];
            Destroy(obj);
        }
        Polls.Clear();

        if (player == null)
        {
            Debug.LogWarning("Player data not loaded yet.");
            return;
        }

        // Instantiate new polls
        foreach (var pollData in PollManager.GetPollList())
        {
            GameObject obj = null;

            if (pollData.NotVoted.Contains(player.Id))
                obj = Instantiate(PollObjectPrefab, NotVotedListContent.transform);
            else
                obj = Instantiate(PollObjectPrefab, VotedListContent.transform);

            if (obj != null)
            {
                obj.GetComponent<PollObject>().SetPollId(pollData.Id);
                Polls.Add(obj);

                obj.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!HasPlayerVoted(pollData, player.Id))
                    {
                        PollPopup.SetActive(true);
                        VotingActions.PassPollId?.Invoke(pollData.Id);
                    }
                    else
                    {
                        Debug.Log("You have already voted on this poll.");
                        // Maybe add visual indicator for the player in the future?
                    }
                });
            }

            Canvas.ForceUpdateCanvases();

        }

        if (Polls.Count == 0) NoPollsText.SetActive(true);
        else NoPollsText.SetActive(false);
    }

    public void SetPollPopupPollId(string pollId)
    {
        PollPopup.GetComponent<PollPopup>().SetPollId(pollId);
    }
}
