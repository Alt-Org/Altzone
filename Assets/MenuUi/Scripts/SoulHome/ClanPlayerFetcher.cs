using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanPlayerFetcher : MonoBehaviour
{
    public readonly List<PlayerData> _players;

    public void RefreshClanMembersPlayerData()
    {
        StartCoroutine(ServerManager.Instance.GetClanMembersFromServer(members =>
        {
            if (members != null)
            {
                Debug.LogError("Failed to load clan members");
            }

            int i = 0;

            foreach (var member in members)
            {
                _players.Add(member.GetPlayerData());
                ++i;
                Debug.LogError(i);
            }
        }));
    }
}
