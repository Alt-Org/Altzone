using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public class AddPlayerHeads : MonoBehaviour
{
    [SerializeField] private GameObject ContentYes;
    [SerializeField] private GameObject ContentNo;
    [SerializeField] private GameObject HeadPrefab;
    private List<GameObject> YesHeads = new List<GameObject>();
    private List<GameObject> NoHeads = new List<GameObject>();

    private void OnEnable()
    {
        //InstantiatePolls();
        //VotingActions.ReloadPollList += InstantiatePolls;
    }

    private void OnDisable()
    {
        //VotingActions.ReloadPollList -= InstantiatePolls;
    }

    public void InstantiateHeads(string pollId)
    {
        PollData pollData = PollManager.GetPollData(pollId);

        // Clear existing heads
        for (int i = 0; i < YesHeads.Count; i++)
        {
            GameObject obj = YesHeads[i];
            Destroy(obj);
        }

        for (int i = 0; i < NoHeads.Count; i++)
        {
            GameObject obj = NoHeads[i];
            Destroy(obj);
        }

        YesHeads.Clear();
        NoHeads.Clear();

        // Instantiate new heads
        foreach (var vote in pollData.YesVotes)
        {
            GameObject obj = Instantiate(HeadPrefab, ContentYes.transform);
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            YesHeads.Add(obj);
        }

        foreach (var vote in pollData.NoVotes)
        {
            GameObject obj = Instantiate(HeadPrefab, ContentNo.transform);
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            NoHeads.Add(obj);
        }
    }
}
