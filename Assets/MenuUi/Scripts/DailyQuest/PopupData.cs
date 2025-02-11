using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;

public struct PopupData
{
    public enum PopupDataType
    {
        OwnTask,
        CancelTask,
    }
    private PopupDataType _type;
    public PopupDataType Type { get { return _type; } }

    public static PopupDataType GetType(string type)
    {
        switch (type)
        {
            case "own_task": return PopupDataType.OwnTask;
            case "cancel_task": return PopupDataType.CancelTask;
            default: return PopupDataType.OwnTask;
        }
    }

    private Vector3? _location;
    public Vector3? Location { get { return _location; } }

    public struct OwnPageData
    {
        private int _taskId;
        private string _taskDescription;
        private int _taskAmount;
        private int _taskPoints;
        private int _taskCoins;

        public int TaskId { get { return _taskId; } }
        public string TaskDescription { get { return _taskDescription; } }
        public int TaskAmount { get { return _taskAmount; } }
        public int TaskPoints { get { return _taskPoints; } }
        public int TaskCoins { get { return _taskCoins; } }

        public void Set(PlayerTask task)
        {
            _taskId = task.Id;
            _taskDescription = task.Title;
            _taskAmount = task.Amount;
            _taskPoints = task.Points;
            _taskCoins = task.Coins;
        }
    }
    //private OwnPageData? _ownPage;
    //public OwnPageData? OwnPage { get { return _ownPage; } }

    private PlayerTask _ownPage;
    public PlayerTask OwnPage { get { return _ownPage; } }

    public PopupData(PopupDataType type, Vector3? location)
    {
        _ownPage = null;
        _type = type;
        _location = location;
    }

    public PopupData(PlayerTask task, Vector3? location)
    {
        //_ownPage = new OwnPageData();
        _ownPage = task;
        _type = PopupDataType.OwnTask;
        _location = location;

        //SetOwnPageData(task);
    }

    public void SetOwnPageData(PlayerTask task)
    {
        //if (_ownPage == null)
        //_ownPage = new OwnPageData();

        //OwnPageData tempData = _ownPage.Value;
        //tempData.Set(task);
        //_ownPage = tempData;
        _ownPage = task;
        _type = PopupDataType.OwnTask;
    }
}
