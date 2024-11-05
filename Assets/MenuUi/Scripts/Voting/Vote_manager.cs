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
    private List<PollObject> pollObjectList = new List<PollObject>();
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

        pollObjectList = clan.Polls;
        Debug.Log(pollObjectList.Count);
    }
}
