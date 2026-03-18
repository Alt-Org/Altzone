using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ClanPlayerFetcher : MonoBehaviour
{
    private readonly List<PlayerData> _players = new();
    public IReadOnlyList<PlayerData> Players { get { return _players; } }
    public bool PlayersLoaded { get; private set; } = false;

    public event Action<PlayerData> OnLocalAvatarUpdated;

    private void Start()
    {
        RefreshClanMembersPlayerData();
    }

    private void OnEnable()
    {
        UpdateLocalAvatar();
    }

    public void RefreshClanMembersPlayerData()
    {
        _players.Clear();

        string clanId = ServerManager.Instance.Player?.clan_id;
        Storefront.Get().GetClanData(clanId, clanData =>
        {
            if (clanData != null)
            {
                foreach (var member in clanData.Members)
                {
                    PlayerData playerData = member.GetPlayerData();
                    _players.Add(playerData);
                }
            }

            PlayersLoaded = true;
        });
    }

    private void UpdateLocalAvatar()
    {
        PlayerData data = new(ServerManager.Instance.Player, true);

        if (data == null)
        {
            return;
        }

        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Id != data.Id)
            {
                continue;
            }

            _players[i] = data;
            OnLocalAvatarUpdated(data);
            break;
        }
    }
}
