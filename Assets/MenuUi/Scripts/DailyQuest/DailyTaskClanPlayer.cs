using UnityEngine;
using TMPro;

public class DailyTaskClanPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ranking;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _taskName;
    [SerializeField] private GameObject _taskCoinRewardIcon;
    [SerializeField] private GameObject _taskPointRewardIcon;
    [SerializeField] private TextMeshProUGUI _taskRewardValue;
    [SerializeField] private TextMeshProUGUI _playerTotalPoints;

    private int _rank;
    public int Rank { get { return _rank; } }

    public void Set(int rank, string name, int points)
    {
        _rank = rank;
        _ranking.text = "" + rank;
        _playerName.text = name;
        _playerTotalPoints.text = $"{points} p";
    }
}
