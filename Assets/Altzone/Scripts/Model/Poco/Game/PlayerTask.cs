using System.Collections.Generic;

namespace Altzone.Scripts.Model.Poco.Game
{
    public enum TaskType
    {
        Undefined,
        Test,
        PlayBattle,
        WinBattle,
        WriteChatMessage,
        StartBattleDifferentCharacter,
        Vote,

    }
    public class PlayerTasks
    {
        private List<PlayerTask> _daily;
        private List<PlayerTask> _week;
        private List<PlayerTask> _month;

        public PlayerTasks(ServerPlayerTasks tasks)
        {
            _daily = new();
            foreach (ServerPlayerTask task in tasks.daily)
            {
                _daily.Add(new(task));
            }
            _week = new();
            foreach (ServerPlayerTask task in tasks.weekly)
            {
                _week.Add(new(task));
            }
            _month = new();
            foreach (ServerPlayerTask task in tasks.monthly)
            {
                _month.Add(new(task));
            }
        }

        public List<PlayerTask> Daily { get => _daily; }
        public List<PlayerTask> Week { get => _week; }
        public List<PlayerTask> Month { get => _month; }
    }
    public class PlayerTask
    {
        private string _id;
        private TaskTitle _title;
        //private TaskContent _content;
        private int _amount;
        private int _amountLeft;
        private TaskType _type;
        private int _coins;
        private int _points;
        private int _taskProgress;
        private string _playerId = "";
        private string _startedAt;

        public string Id { get => _id;}
        public int Amount { get => _amount;}
        public TaskType Type { get => _type;}
        public int Coins { get => _coins;}
        public int Points { get => _points;}
        public string Title {
            get
            {
                return _title.Fi;
            }
        }
        //public string Content { get => _content.Fi;}
        public int TaskProgress { get => _taskProgress;}
        public string PlayerId { get => _playerId; }
        public int AmountLeft { get => _amountLeft; }
        public string StartedAt { get => _startedAt; }

        public PlayerTask(ServerPlayerTask task)
        {
            _id = task._id;
            _title = new(task.title);
            //_content = new(task.content);
            _amount = task.amount;
            _amountLeft = task.amountLeft;
            _coins = task.coins;
            _points = task.points;
            _type = GetTypeEnum(task.type);
            _playerId = string.IsNullOrWhiteSpace(task.player_id) ? "" : task.player_id;
            _startedAt = task.startedAt;
    }

        public int Progress(int amount)
        {
            if (amount <= 0) return _taskProgress;
            _taskProgress =- amount;
            if(_taskProgress < 0) _taskProgress = 0;
            return _taskProgress;
        }

        private TaskType GetTypeEnum(string type)
        {
            switch (type)
            {
                case "play_battle":
                    {
                        return TaskType.PlayBattle;
                    }
                case "win_battle":
                    {
                        return TaskType.WinBattle;
                    }
                case "write_chat_message":
                    {
                        return TaskType.WriteChatMessage;
                    }
                case "start_battle_with_new_character":
                    {
                        return TaskType.StartBattleDifferentCharacter;
                    }
                case "vote":
                    {
                        return TaskType.Vote;
                    }
                default:
                    {
                        return TaskType.Undefined;
                    }
            }
        }

        public class TaskTitle
        {
            private readonly string _fi;

            public string Fi { get => _fi;}

            public TaskTitle(ServerPlayerTask.TaskTitle title)
            {
                _fi = title.fi;
            }
        }
        public class TaskContent
        {
            private readonly string _fi;

            public string Fi { get => _fi;}
            public TaskContent(ServerPlayerTask.TaskContent content)
            {
                _fi = content.fi;
            }
        }

        #region Delegates, Events & Invoke Functions

        public delegate void TaskSelected();
        public event TaskSelected OnTaskSelected;
        public void InvokeOnTaskSelected()
        {
            OnTaskSelected.Invoke();
        }

        public delegate void TaskDeselected();
        public event TaskDeselected OnTaskDeselected;
        public void InvokeOnTaskDeselected()
        {
            OnTaskDeselected.Invoke();
        }

        public delegate void TaskUpdated();
        public event TaskUpdated OnTaskUpdated;
        public void InvokeOnTaskUpdated()
        {
            OnTaskUpdated.Invoke();
        }

        #endregion

        #region Add & clear from outside.

        public void AddProgress(int value)
        {
            _taskProgress += value;
        }

        public void ClearProgress()
        {
            _taskProgress = 0;
        }

        public void AddPlayerId(string id)
        {
            _playerId = id;
        }

        public void ClearPlayerId()
        {
            _playerId = "";
        }

        #endregion
    }

    public class ServerPlayerTasks
    {
        public List<ServerPlayerTask> daily;
        public List<ServerPlayerTask> weekly;
        public List<ServerPlayerTask> monthly;
    }

    public class ServerPlayerTask
    {
        public string _id;
        public TaskTitle title;
        //public TaskContent content;
        public int amount;
        public int amountLeft;
        public string type;
        public int coins;
        public int points;
        public string player_id;
        public string startedAt;

        public class TaskTitle
        {
            public string fi;
        }
        public class TaskContent
        {
            public string fi;
        }
    }
}
