using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Voting;
using UnityEngine;

public class VoteManager : MonoBehaviour
{
    public GameObject Content;
    public GameObject PollObjectPrefab;
    private List<GameObject> Polls = new List<GameObject>();

    private void OnEnable()
    {
        InstantiatePolls();
    }

    public void InstantiatePolls()
    {
        // Clear existing polls
        for (int i = 0; i < Polls.Count; i++)
        {
            GameObject obj = Polls[i];
            Destroy(obj);
        }
        Polls.Clear();

        // Instantiate new polls
        foreach (var pollData in PollManager.GetPollList())
        {
            GameObject obj = Instantiate(PollObjectPrefab, Content.transform);
            obj.GetComponent<PollObject>().SetPollData(pollData);
            Polls.Add(obj);
        }
    }
}
