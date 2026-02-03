using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanPlayerFetcher : MonoBehaviour
{
    private readonly List<PlayerData> _players = new();
    public List<PlayerData> Players { get { return _players; } }
    public bool PlayersLoaded { get; private set; } = false;

    private void Start()
    {
        RefreshClanMembersPlayerData();
    }

    public void RefreshClanMembersPlayerData()
    {
        StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(members =>
        {
            _players.Clear();

            if (members == null)
            {
                Debug.LogError("Failed to load clan members");
            }
            else
            {
                foreach (var member in members)
                {
                    _players.Add(member.GetPlayerData());
                }
            }

            PlayersLoaded = true;

        }));
    }
}
