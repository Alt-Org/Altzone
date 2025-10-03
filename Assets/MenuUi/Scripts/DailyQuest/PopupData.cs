using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public struct PopupData
{
    public enum PopupDataType
    {
        OwnTask,
        CancelTask,
        ClanMilestone,
        MultipleChoice,
    }
    private PopupDataType _type;
    public PopupDataType Type { get { return _type; } }

    public static PopupDataType GetType(string type)
    {
        switch (type)
        {
            case "own_task": return PopupDataType.OwnTask;
            case "cancel_task": return PopupDataType.CancelTask;
            case "clan_milestone": return PopupDataType.ClanMilestone;
            case "multiple_choice": return PopupDataType.MultipleChoice;
            default: return PopupDataType.OwnTask;
        }
    }

    private Vector3? _location;
    public Vector3? Location { get { return _location; } }

    private PlayerTask _ownPage;
    public PlayerTask OwnPage { get { return _ownPage; } }

    private int? _dailyTaskCardIndex;
    public int? DailyTaskCardIndex { get { return _dailyTaskCardIndex; } }

    private DailyTaskClanReward.ClanRewardData? _clanRewardData;
    public DailyTaskClanReward.ClanRewardData? ClanRewardData { get { return _clanRewardData; } }

    /// <summary>
    /// Used for showing task cancel window.
    /// </summary>
    /// <param name="type"></param>
    public PopupData(PopupDataType type)
    {
        _ownPage = null;
        _clanRewardData = null;
        _type = type;
        _location = null;
        _dailyTaskCardIndex = null;
    }

    /// <summary>
    /// Used for showing task accept window.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="location"></param>
    public PopupData(PlayerTask task, Vector3 location, int index)
    {
        _ownPage = task;
        _clanRewardData = null;
        _type = PopupDataType.OwnTask;
        _location = location;
        _dailyTaskCardIndex = index;
    }

    /// <summary>
    /// Used for showing clan milestone info window.
    /// </summary>
    /// <param name="clanRewardData"></param>
    /// <param name="location"></param>
    public PopupData(DailyTaskClanReward.ClanRewardData clanRewardData, Vector3 location)
    {
        _ownPage = null;
        _clanRewardData = clanRewardData;
        _type = PopupDataType.ClanMilestone;
        _location = location;
        _dailyTaskCardIndex = null;
    }

    //public void SetOwnPageData(PlayerTask task)
    //{
    //    _ownPage = task;
    //    _type = PopupDataType.OwnTask;
    //}

    public PopupData(PlayerTask task)
    {
        _ownPage = task;
        _clanRewardData = null;
        _type = PopupDataType.MultipleChoice;
        _location = null;
        _dailyTaskCardIndex = null;
    }
}
