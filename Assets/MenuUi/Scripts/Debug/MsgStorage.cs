using System;
using System.Collections.Generic;

namespace DebugUi.Scripts.BattleAnalyzer
{
    #region Enum and Interfaces

    public enum MessageType
    {
        None = 0,
        Info = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3
    }

    [Flags]
    public enum MessageTypeOptions
    {
        None = 0,
        Info = MessageType.Info,
        Warning = MessageType.Warning,
        Error = MessageType.Error
    }

    internal interface IReadOnlyMsgObject
    {
        public int Client { get; }
        public int Id { get;}
        public int Time { get; }
        public string Msg { get; }
        public MessageType Type { get; }
    }

    internal interface IReadOnlyTimestamp
    {
        public int Time { get; }
        public MessageType Type { get; }
        public IReadOnlyList<IReadOnlyMsgObject> List { get; }
    }

    internal interface IReadOnlyMsgStorage
    {
        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client);
        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time);
        public IReadOnlyTimelineStorage GetTimelineStorage();
        public int TotalMessages();
    }

    internal interface IReadOnlyTimelineStorage
    {
        public IReadOnlyTimestamp GetTimestamp(int client, int time);
        public IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client);
    }

    #endregion

    internal class MsgObject : IReadOnlyMsgObject
    {
        public int Client { get; }
        public int Id { get; private set; }
        public int Time { get; }
        public string Msg { get; }
        public string Trace { get; }
        public MessageType Type { get; }

        internal MsgObject(int client, int time, string msg, string trace, MessageType type)
        {
            Client = client;
            Id = -1;
            Time = time;
            Msg = msg;
            Trace = trace;
            Type = type;
        }

        internal void SetId(int id)
        {
            Id = id;
        }
    }

    internal class Timestamp : IReadOnlyTimestamp
    {
        public int Time { get; }
        public MessageType Type { get; private set; }
        public IReadOnlyList<IReadOnlyMsgObject> List => _list;

        internal Timestamp(int time)
        {
            Time = time;
            Type = MessageType.None;
            _list = new();
        }

        internal void AddMsg(MsgObject msgObject)
        {
            _list.Add(msgObject);
            if(Type < msgObject.Type) Type = msgObject.Type;
        }

        private readonly List<MsgObject> _list;
    }

    internal class MsgStorage : IReadOnlyMsgStorage
    {
        public static IReadOnlyList<IReadOnlyMsgObject> GetSubList(IReadOnlyList<IReadOnlyMsgObject> list, MessageTypeOptions wantedTypes)
        {
            if (list == null) return null;
            List<IReadOnlyMsgObject> newList = new();
            if (list.Count == 0) return newList;

            foreach (IReadOnlyMsgObject msgObject in list)
            {
                if (wantedTypes.HasFlag((MessageTypeOptions)msgObject.Type))
                {
                    newList.Add(msgObject);
                }
            }
            return newList;
        }

        internal MsgStorage(int clientCount)
        {
            _timelineStorage = new(clientCount);
            _msgList = new List<MsgObject>[clientCount];
            _timeStampMapList = new Dictionary<int, IReadOnlyTimestamp>[clientCount];
            for (int i = 0; i < clientCount; i++)
            {
                _msgList[i] = new();
                _timeStampMapList[i] = new();
            }
        }

        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client)
        {
            if (!IsValidClient(client)) return null;
            return _msgList[client];
        }

        internal void Add(MsgObject msg)
        {
            if (IsValidClient(msg.Client))
            {
                msg.SetId(_msgList[msg.Client].Count);
                _msgList[msg.Client].Add(msg);

                IReadOnlyTimestamp stamp = _timelineStorage.AddMessageToTimestamp(msg);

                if (!_timeStampMapList[msg.Client].ContainsKey(msg.Time))
                    _timeStampMapList[msg.Client][msg.Time] = stamp;
            }
        }

        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time)
        {
            if (!IsValidClient(client)) return null;
            if (_timeStampMapList[client] == null) return null;
            if (!_timeStampMapList[client].ContainsKey(time)) return null;
            return _timeStampMapList[client][time].List;
        }

        public IReadOnlyTimelineStorage GetTimelineStorage()
        {
            return _timelineStorage;
        }

        public bool IsValidClient(int client)
        {
            return _msgList.Length > client && client >= 0;
        }

        public int TotalMessages()
        {
            int total = 0;
            foreach (List<MsgObject> msgList in _msgList)
            {
                total += msgList.Count;
            }
            return total;
        }

        private readonly List<MsgObject>[] _msgList;
        private readonly Dictionary<int, IReadOnlyTimestamp>[] _timeStampMapList;
        private readonly TimelineStorage _timelineStorage;

        private class TimelineStorage : IReadOnlyTimelineStorage
        {
            internal TimelineStorage(int clientCount)
            {
                _timelines = new List<Timestamp>[clientCount];
                for (int i = 0; i < clientCount; i++)
                {
                    _timelines[i] = new();
                }
            }

            public IReadOnlyTimestamp GetTimestamp(int client, int time)
            {
                if (!IsValidClient(client)) return null;
                return _timelines[client][time];
            }

            public IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client)
            {
                if (!IsValidClient(client)) return null;
                return _timelines[client];
            }

            private Timestamp GetOrNew(int client, int time)
            {
                int timelineSize = _timelines[client].Count;
                if (timelineSize <= time)
                {
                    for (int i = timelineSize; i <= time; i++)
                    {
                        _timelines[client].Add(new(i));
                    }
                }
                return _timelines[client][time];
            }

            internal IReadOnlyTimestamp AddMessageToTimestamp(MsgObject msg)
            {
                Timestamp stamp = GetOrNew(msg.Client, msg.Time);
                stamp.AddMsg(msg);
                return stamp;

            }

            private bool IsValidClient(int client)
            {
                return _timelines.Length > client && client >= 0;
            }

            private readonly List<Timestamp>[] _timelines;
        }

        /*
         * +------------------------------------------------------------+
         * |                         MsgStorage                         |
         * +------------------------------------------------------------+
         * |                                                            |
         * |  _msgList: List<MsgObject>[]                               |
         * |    [client0]: List<MsgObject>                              |                                                                     +-----------------------------+
         * |      [Id0]: MsgObject --------------------------------------------------------------------------------------------+------------->|          MsgObject          |
         * |      [Id1]: MsgObject --------------------------------------------------------------------------------------------|-+---------+  +-----------------------------+
         * |      [Id2]: MsgObject --------------------------------------------------------------------------------------------|-|-+-----+ |  |  Client: 0 (int)            |
         * |      .                                                     |                                                      | | |     | |  |  Id: 0 (int)                |
         * |      .                                                     |                                                      | | |     | |  |  Time: 1 (int)              |
         * |    [client1]: List<MsgObject>                              |                                                      | | |     | |  |  Msg: string                |
         * |      [Id0]: MsgObject --------------------------------------------------------------------------------------------|-|-|--+  | |  |  Type: MessageType.Info     |
         * |      .                                                     |                                                      | | |  |  | |  +-----------------------------+
         * |      .                                                     |                                                      | | |  |  | |
         * |    .                                                       |                     +-----------------------------+  | | |  |  | |  +-----------------------------+
         * |    .                                                       |  +----------------->|          Timestamp          |  | | |  |  | +->|          MsgObject          |
         * |                                                            |  | +-------------+  +-----------------------------+  | | |  |  |    +-----------------------------+
         * |  _timeStampMapList: Dictionary<int, IReadOnlyTimestamp>[]  |  | | +---------+ |  |  Time: 0 (int)              |  | | |  |  |    |  Client: 0 (int)            |
         * |    [client0]: Dictionary<int, IReadOnlyTimestamp>          |  | | | +-----+ | |  |  Type: MessageType.None     |  | | |  |  |    |  Id: 1 (int)                |
         * |      [Time1]: IReadOnlyTimestamp -----------------------------|-+ | |     | | |  |                             |  | | |  |  |    |  Time: 1 (int)              |
         * |      [Time3]: IReadOnlyTimestamp -----------------------------|-|-|-+     | | |  |  _list: List<MsgObject>     |  | | |  |  |    |  Msg: string                |
         * |      .                                                     |  | | | |     | | |  |    empty                    |  | | |  |  |    |  Type: MessageType.Warning  |
         * |      .                                                     |  | | | |     | | |  |                             |  | | |  |  |    +-----------------------------+
         * |    [client1]: Dictionary<int, IReadOnlyTimestamp>          |  | | | |     | | |  +-----------------------------+  | | |  |  |
         * |      [Time0]: IReadOnlyTimestamp  ----------------------------|-|-|-|--+  | | |                                   | | |  |  |    +-----------------------------+
         * |      .                                                     |  | | | |  |  | | |  +-----------------------------+  | | |  |  +--->|          MsgObject          |
         * |      .                                                     |  | | | |  |  | | +->|          Timestamp          |  | | |  |       +-----------------------------+
         * |    .                                                       |  | | | |  |  | |    +-----------------------------+  | | |  |       |  Client: 0 (int)            |
         * |    .                                                       |  | | | |  |  | |    |  Time: 1 (int)              |  | | |  |       |  Id: 2 (int)                |
         * |                                                            |  | | | |  |  | |    |  Type: MessageType.Warning  |  | | |  |       |  Time: 3 (int)              |
         * |  +-------------------------------------+                   |  | | | |  |  | |    |                             |  | | |  |       |  Msg: string                |
         * |  |           TimelineStorage           |                   |  | | | |  |  | |    |  _list: List<MsgObject>     |  | | |  |       |  Type: MessageType.Info     |
         * |  +-------------------------------------+                   |  | | | |  |  | |    |    [0]: MsgObject -------------+ | |  |       +-----------------------------+
         * |  |                                     |                   |  | | | |  |  | |    |    [1]: MsgObject ---------------+ |  |
         * |  |  _timelines: List<List<Timestamp>>  |                   |  | | | |  |  | |    |                             |      |  |
         * |  |   [client0]: <List<Timestamp>       |                   |  | | | |  |  | |    +-----------------------------+      |  |       +-----------------------------+
         * |  |     [Time0]: Timestamp ------------------------------------+ | | |  |  | |                                         |  +------>|          MsgObject          |
         * |  |     [Time1]: Timestamp --------------------------------------+ | |  |  | |    +-----------------------------+      |  |       +-----------------------------+
         * |  |     [Time2]: Timestamp ----------------------------------------+ |  |  | +--->|          Timestamp          |      |  |       |  Client: 1 (int)            |
         * |  |     [Time3]: Timestamp ------------------------------------------+  |  |      +-----------------------------+      |  |       |  Id: 0 (int)                |
         * |  |     .                               |                   |           |  |      |  Time: 2 (int)              |      |  |       |  Time: 0 (int)              |
         * |  |     .                               |                   |           |  |      |  Type: MessageType.None     |      |  |       |  Msg: string                |
         * |  |   [client1]: <List<Timestamp>       |                   |           |  |      |                             |      |  |       |  Type: MessageType.Info     |
         * |  |     [Time0]: Timestamp ---------------------------------------------+  |      |  _list: List<MsgObject>     |      |  |       +-----------------------------+
         * |  |     .                               |                   |           |  |      |    empty                    |      |  |
         * |  |     .                               |                   |           |  |      |                             |      |  |
         * |  |   .                                 |                   |           |  |      +-----------------------------+      |  |
         * |  |   .                                 |                   |           |  |                                           |  |
         * |  |                                     |                   |           |  |      +-----------------------------+      |  |
         * |  +-------------------------------------+                   |           |  +----->|          Timestamp          |      |  |
         * |                                                            |           |         +-----------------------------+      |  |
         * +------------------------------------------------------------+           |         |  Time: 3 (int)              |      |  |
         *                                                                          |         |  Type: MessageType.Info     |      |  |
         *                                                                          |         |                             |      |  |
         *                                                                          |         |  _list: List<MsgObject>     |      |  |
         *                                                                          |         |    [0]: MsgObject -----------------+  |
         *                                                                          |         |                             |         |
         *                                                                          |         +-----------------------------+         |
         *                                                                          |                                                 |
         *                                                                          |                                                 |
         *                                                                          |         +-----------------------------+         |
         *                                                                          +-------->|          Timestamp          |         |
         *                                                                                    +-----------------------------+         |
         *                                                                                    |  Time: 0 (int)              |         |
         *                                                                                    |  Type: MessageType.Info     |         |
         *                                                                                    |                             |         |
         *                                                                                    |  _list: List<MsgObject>     |         |
         *                                                                                    |    [0]: MsgObject --------------------+
         *                                                                                    |                             |
         *                                                                                    +-----------------------------+
         */
    }

}
