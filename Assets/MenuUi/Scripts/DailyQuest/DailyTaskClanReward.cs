using TMPro;
using UnityEngine;

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

    private DailyTaskManager _dailyTaskManager;

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
        private Sprite _rewardImage;
        private int _rewardAmount;

        public bool Open { get { return _open; } }
        public ClanRewardType Type { get { return _type; } }
        public int Threshold { get { return _threshold; } }
        public Sprite RewardImage { get { return _rewardImage; } }
        public int RewardAmount { get { return _rewardAmount; } }

        public ClanRewardData(bool open, ClanRewardType type, int threshold, Sprite rewardImage, int rewardAmount)
        {
            _open = open;
            _type = type;
            _threshold = threshold;
            _rewardImage = rewardImage;
            _rewardAmount = rewardAmount;
        }
    }

    private ClanRewardData _data;
    public ClanRewardData Data { get { return _data; } }

    public void Set(ClanRewardData data, DailyTaskManager dailyTaskManager)
    {
        _data = data;

        _unopenedBaseReward.SetActive(!data.Open);
        _openedBaseReward.SetActive(data.Open);

        _unopenedBoxReward.SetActive(data.Type == ClanRewardType.Box);
        _openedBoxReward.SetActive(data.Type == ClanRewardType.Box);

        _unopenedChestReward.SetActive(data.Type == ClanRewardType.Chest);
        _openedChestReward.SetActive(data.Type == ClanRewardType.Chest);

        _rewardThreshold.text = "" + data.Threshold;

        _dailyTaskManager = dailyTaskManager;
    }

    public void UpdateState(bool open)
    {
        _unopenedBaseReward.SetActive(!open);
        _openedBaseReward.SetActive(open);
    }

    /// <summary>
    /// Used when user clicks this clan milestone reward.
    /// </summary>
    public void OpenClanRewardPopup()
    {
        PopupData popupData = new(_data, transform.position);
        StartCoroutine(_dailyTaskManager.ShowPopupAndHandleResponse("", popupData));
    }
}
