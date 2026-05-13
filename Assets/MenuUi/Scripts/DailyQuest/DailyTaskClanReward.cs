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

        public ClanRewardType Type { get { return _type; } }
        public int Threshold { get { return _threshold; } }
        public Sprite RewardImage { get { return _rewardImage; } }
        public int RewardAmount { get { return _rewardAmount; } }


        /// <summary>
        /// Sets reward opened/closed
        /// </summary>
        /// <param name="open">If the reward should be open</param>
        public void SetOpen(bool open)
        {
            _open = open;
        }

        /// <summary>
        /// Checks if the reward is opened or not
        /// </summary>
        /// <returns>True if open, false if not</returns>
        public bool IsOpen()
        {
            return _open;
        }

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

    public void Set(ClanRewardData data)
    {
        _data = data;

        _unopenedBaseReward.SetActive(!data.IsOpen());
        _openedBaseReward.SetActive(data.IsOpen());

        _unopenedBoxReward.SetActive(data.Type == ClanRewardType.Box);
        _openedBoxReward.SetActive(data.Type == ClanRewardType.Box);

        _unopenedChestReward.SetActive(data.Type == ClanRewardType.Chest);
        _openedChestReward.SetActive(data.Type == ClanRewardType.Chest);

        _rewardThreshold.text = "" + data.Threshold;
    }

    /// <summary>
    /// Updates the opened state of the reward and activates the right game object based on it
    /// </summary>
    /// <param name="open">If the reward should be open</param>
    public void UpdateState(bool open)
    {
        _unopenedBaseReward.SetActive(!open);
        _openedBaseReward.SetActive(open);
        _data.SetOpen(open);
    }

    /// <summary>
    /// Used when user clicks this clan milestone reward.
    /// </summary>
    public void OpenClanRewardPopup()
    {
        PopupData popupData = new(_data, transform.position);
        DailyTaskManager.Instance.ShowPopupAndHandleResponse("", popupData);
    }
}
