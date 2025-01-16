using Altzone.Scripts.Model.Poco.Game;

public struct PopupData
{
    public PopupData(PlayerTasks.PlayerTask task, PopupDataType type)
    {
        _ownPage = new OwnPageData();
        _type = type;

        switch (type)
        {
            case PopupDataType.OwnTask: SetOwnPageData(task); break;
            default: SetOwnPageData(task); break;
        }
    }

    public enum PopupDataType
    {
        OwnTask,
    }
    private PopupDataType _type;
    public PopupDataType Type { get { return _type; } }

    public static PopupDataType GetType(string type)
    {
        switch (type)
        {
            case "own_task": return PopupDataType.OwnTask;
            default: return PopupDataType.OwnTask;
        }
    }

    #region OwnPage
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
    private OwnPageData? _ownPage;
    public OwnPageData? OwnPage { get { return _ownPage; } }

    public void SetOwnPageData(PlayerTasks.PlayerTask task)
    {
        if (_ownPage == null)
            _ownPage = new OwnPageData();

        OwnPageData temp = _ownPage.Value;
        temp.Set(task);
        _ownPage = temp;
        _type = PopupDataType.OwnTask;
    }
    #endregion
}
