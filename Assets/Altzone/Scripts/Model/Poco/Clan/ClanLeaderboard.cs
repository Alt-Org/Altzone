using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class ClanLeaderboard
{
    private int _points;
    private ClanData _clan;

    public ClanLeaderboard(ServerClan clan)
    {
        _points = clan.points;
        _clan = new(clan);
    }
    public ClanLeaderboard(ClanData clan)
    {
        _points = clan.Points;
        _clan = clan;
    }

    public int Points { get => _points; }
    public ClanData Clan { get => _clan; }
}
