using System.Data;
using TMPro;
using UnityEngine;
using static UnityEditor.LightingExplorerTableColumn;

public class DailyTaskClanReward : MonoBehaviour
{
    [Header("Un&Opened Bases")]
    [SerializeField] private GameObject _unopenedBaseReward;
    [SerializeField] private GameObject _openedBaseReward;

    [Header("Box")]
    [SerializeField] private GameObject _unopenedBoxReward;
    [SerializeField] private GameObject _openedBoxReward;

    [Header("Chest")]
    [SerializeField] private GameObject _unopenedChestReward;
    [SerializeField] private GameObject _openedChestReward;

    [Header("Values")]
    [SerializeField] private TextMeshProUGUI _rewardThreshold;

    public enum ClanRewardType
    {
        Box,
        Chest
    }

    //May be replaced if server side comes up with something else.
    public struct ClanRewardData
    {
        private bool _open;
        private ClanRewardType _type;
        private int _threshold;

        public bool Open { get { return _open; } }
        public ClanRewardType Type { get { return _type; } }
        public int Threshold { get { return _threshold; } }

        public ClanRewardData(bool open, ClanRewardType type, int threshold)
        {
            _open = open;
            _type = type;
            _threshold = threshold;
        }
    }

    private ClanRewardData _data;
    public ClanRewardData Data { get { return _data; } }

    public void Set(ClanRewardData data)
    {
        _data = data;

        _unopenedBaseReward.SetActive(!data.Open);
        _openedBaseReward.SetActive(data.Open);

        _unopenedBoxReward.SetActive(data.Type == ClanRewardType.Box);
        _openedBoxReward.SetActive(data.Type == ClanRewardType.Box);

        _unopenedChestReward.SetActive(data.Type == ClanRewardType.Chest);
        _openedChestReward.SetActive(data.Type == ClanRewardType.Chest);

        _rewardThreshold.text = "" + data.Threshold;
    }

    public void UpdateState(bool open)
    {
        _unopenedBaseReward.SetActive(!open);
        _openedBaseReward.SetActive(open);
    }
}
