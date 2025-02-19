using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using static DailyTaskClanReward;

public struct PopupData
{
    public enum PopupDataType
    {
        OwnTask,
        CancelTask,
        ClanMilestone,
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
            default: return PopupDataType.OwnTask;
        }
    }

    private Vector3? _location;
    public Vector3? Location { get { return _location; } }

    private PlayerTask _ownPage;
    public PlayerTask OwnPage { get { return _ownPage; } }

    private ClanRewardData? _clanRewardData;
    public ClanRewardData? ClanRewardData { get { return _clanRewardData; } }

    public PopupData(PopupDataType type, Vector3? location)
    {
        _ownPage = null;
        _clanRewardData = null;
        _type = type;
        _location = location;
    }

    public PopupData(PlayerTask task, Vector3? location)
    {
        _ownPage = task;
        _clanRewardData = null;
        _type = PopupDataType.OwnTask;
        _location = location;
    }

    public PopupData(ClanRewardData clanRewardData, Vector3 location)
    {
        _ownPage = null;
        _clanRewardData = clanRewardData;
        _type = PopupDataType.ClanMilestone;
        _location = location;
    }

    public void SetOwnPageData(PlayerTask task)
    {
        _ownPage = task;
        _type = PopupDataType.OwnTask;
    }
}
