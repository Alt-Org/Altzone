using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class ClanLeaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardPrefab;

    public void LoadClanLeaderboard(ServerClan clan)
    {
        Debug.Log("Loading clan leaderboard");
    }
}
