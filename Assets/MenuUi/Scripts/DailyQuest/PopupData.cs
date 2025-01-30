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

        public void Set(PlayerTasks.PlayerTask task)
        {
            _taskId = task.Id;
            _taskDescription = task.Content;
            _taskAmount = task.Amount;
            _taskPoints = task.Points;
            _taskCoins = task.Coins;
        }
    }
    //private OwnPageData? _ownPage;
    //public OwnPageData? OwnPage { get { return _ownPage; } }

    private PlayerTasks.PlayerTask _ownPage;
    public PlayerTasks.PlayerTask OwnPage { get { return _ownPage; } }

    public PopupData(PopupDataType type)
    {
        _ownPage = null;
        _type = type;
    }

    public PopupData(PlayerTasks.PlayerTask task)
    {
        //_ownPage = new OwnPageData();
        _ownPage = task;
        _type = PopupDataType.OwnTask;

        //SetOwnPageData(task);
    }

    public void SetOwnPageData(PlayerTasks.PlayerTask task)
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
