using System.Collections.Generic;
using UnityEngine;

public class PastPollManager : MonoBehaviour
{
    [SerializeField] private GameObject PastPollListContent;
    [SerializeField] private GameObject PollObjectPrefab;
    [SerializeField] private GameObject NoPollsText;

    private List<GameObject> PastPolls = new List<GameObject>();

    private void OnEnable()
    {
        InstantiatePastPolls();
    }

    public void InstantiatePastPolls()
    {
        foreach (GameObject obj in PastPolls)
        {
            Destroy(obj);
        }
        PastPolls.Clear();

        var pastPollList = PollManager.GetPastPollList();

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
