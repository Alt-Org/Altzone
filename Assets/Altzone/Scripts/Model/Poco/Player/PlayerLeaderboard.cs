using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class PlayerLeaderboard 
{
    private int _points;
    private int _wonBattles;
    private PlayerData _player;

    public PlayerLeaderboard(ServerPlayer player)
    {
        _points = player.points;
        //_wonBattles = player.gameStatistics.wonBattles;
        _player = new(player);
    }
    public PlayerLeaderboard(PlayerData player)
    {
        _points = player.points;
        _wonBattles = player.stats.wonBattles;
        _player = player;
    }

    public int Points { get => _points; }
    public int WonBattles { get => _wonBattles; }
    public PlayerData Player { get => _player;}
}
