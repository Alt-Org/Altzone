using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vote_manager : MonoBehaviour
{
    //kesken
    //public GameObject aVote;
    //public Transform voteList;
    //public int maxVotes = 10;

    //private List<GameObject> votes = new List<GameObject>();


    private List<VotingObject> votingItemsList = new List<VotingObject>();

    private SettingsCarrier settingsCarrier;

    // Start is called before the first frame update
    void Start()
    {
        settingsCarrier = SettingsCarrier.Instance;

        if (settingsCarrier.ItemVotingStarted())
        {
            votingItemsList = settingsCarrier.GetVotingObjects();
        }
       PopulateVoteList();
    }

    //void PopulateVoteList()
    //{
    //foreach (var entry in votes)
    //{
    //Destroy(entry.gameObject);
    //}
    //votes.Clear();

    // for (int i = 0; i < Mathf.Min(maxVotes, VoteDataList.Count); i++)
    // {
    // GameObject entry = Instantiate(aVote, voteList);
    //
    //     VoteData voteData = VoteDataList[i];
    //   }

    void PopulateVoteList()
    {
        foreach (VotingObject item in votingItemsList)
        {
            //Debug.Log("new voting items: " + item.id); // Assuming VotingObject has a proper ToString() method or override
            //Debug.Log("new voting items: " + item.votableName);
        }
    }

    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
