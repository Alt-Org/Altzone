using System;
using System.Collections.Generic;
using UnityEngine;

public class PastPollManager : MonoBehaviour // Handles the Ui display for past polls
{
    [SerializeField] private GameObject PastPollListContent;
    [SerializeField] private GameObject PollObjectPrefab;
    [SerializeField] private GameObject NoPollsText;

    public static Action OnPastPollsChanged;

    private List<GameObject> PastPolls = new List<GameObject>();

    private void OnEnable()
    {
        InstantiatePastPolls();
        OnPastPollsChanged += InstantiatePastPolls;
    }

    private void OnDisable()
    {
        OnPastPollsChanged -= InstantiatePastPolls;
    }

    public void InstantiatePastPolls()
    {
        foreach (GameObject obj in PastPolls)
        {
            Destroy(obj);
        }
        PastPolls.Clear();

        // Retrieve the list of past polls from pollmanager
        var pastPollList = PollManager.GetPastPollList();

        // Instantiate PollObject for every poll
        foreach (var pollData in pastPollList)
        {
            GameObject obj = Instantiate(PollObjectPrefab, PastPollListContent.transform);
            obj.GetComponent<PollObject>().SetPollId(pollData.Id);
            PastPolls.Add(obj);

            // Disable interaction
            obj.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        NoPollsText.SetActive(PastPolls.Count == 0);
    }
}
