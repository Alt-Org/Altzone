using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts;
using Altzone.Scripts.Voting;
using UnityEngine;
using UnityEngine.UI;

public class Vote_manager : MonoBehaviour
{
    public GameObject Content;
    public GameObject PollObjectPrefab;
    private List<PollData> pollDataList = new List<PollData>();
    private List<GameObject> Polls = new List<GameObject>();

    private void OnEnable()
    {
        LoadPollList();
    }

    public void LoadPollList()
    {
        DataStore store = Storefront.Get();
        PlayerData player = null;
        ClanData clan = null;

        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data => player = data);
        store.GetClanData(player.ClanId, data => clan = data);

        pollDataList = clan.Polls;
        InstantiatePolls();

        Debug.Log("Polls: " + pollDataList.Count);
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
        foreach (var pollData in pollDataList)
        {
            GameObject obj = Instantiate(PollObjectPrefab, Content.transform);
            Polls.Add(obj);
        }
    }
}
