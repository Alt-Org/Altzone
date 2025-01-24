using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Game;

public class DailyTaskClanPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ranking;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _taskName;
    [SerializeField] private GameObject _taskCoinRewardIcon;
    [SerializeField] private GameObject _taskPointRewardIcon;
    [SerializeField] private TextMeshProUGUI _taskRewardValue;
    [SerializeField] private TextMeshProUGUI _playerTotalPoints;

    private PlayerData _playerData;
    public PlayerData PlayerData {  get { return _playerData; } }

    private PlayerTasks.PlayerTask _playerTask;
    public PlayerTasks.PlayerTask PlayerTask { get { return _playerTask; } }

    private int _rank;
    public int Rank { get { return _rank; } }

    public void Set(int rank, PlayerData player, PlayerTasks.PlayerTask task)
    {
        _rank = rank;
        _playerData = player;
        _playerTask = task;

        _ranking.text = "" + rank;
        _playerName.text = (player != null ? player.Name : "Player" + rank);
        _taskName.text = (task != null ? task.Title : "No task");

        _taskCoinRewardIcon.SetActive((task != null ? task.Coins > 0 : true));
        _taskPointRewardIcon.SetActive((task != null ? task.Points > 0 : false));

        _taskRewardValue.text = "" + (task != null ? (task.Coins > 0 ?  task.Coins : task.Points) : "9999");
        _playerTotalPoints.text = "" + (player != null ? player.points : 1000000 - rank);
    }

}
