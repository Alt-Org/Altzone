using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public class PollManagerData : MonoBehaviour
{
    public List<PollData> pollDataList = new List<PollData>();
    public List<PollData> pastPollDataList = new List<PollData>();

    public static PollManagerData Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
