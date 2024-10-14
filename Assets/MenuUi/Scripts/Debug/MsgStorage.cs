using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        public int SourceFlag { get; }
        public string Trace { get; }
        public MessageType Type { get; }
        public IReadOnlyList<IReadOnlyMsgObject> MatchList { get; }
        public int ColorGroup { get; }

        public string GetHighlightedMsg(List<Color> colors);
        public bool IsType(MessageTypeOptions typeOptions);
        public bool IsFromSource(int sourceFlags);
    }

    internal interface IReadOnlyTimestamp
    {
        public int Time { get; }
        public MessageType Type { get; }
        public int SourceTypes { get; }
        public IReadOnlyList<IReadOnlyMsgObject> List { get; }
    }

    internal interface IReadOnlyMsgStorage
    {
        public int ClientCount { get; }

        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client);
        public int[] GetTimes();
        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time);
        public IReadOnlyTimelineStorage GetTimelineStorage();
        public int TotalMessages();
        public int GetSourceAllFlags();
        public IReadOnlyList<int> GetSourceFlagList();
        public string GetSourceFlagName(int sourceFlag);
        public IReadOnlyList<int> GetColorSuperGroupList();
    }

    internal interface IReadOnlyTimelineStorage
    {
        public int ClientCount { get; }
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
        public int SourceFlag { get; }
        public string Trace { get; }
        public MessageType Type { get; }
        public IReadOnlyList<IReadOnlyMsgObject> MatchList { get; private set; }
        public int ColorGroup { get; private set; }

        internal MsgObject(int client, int time, string msg, int sourceFlag, string trace, MessageType type)
        {
            Client = client;
            Id = -1;
            Time = time;
            Msg = msg;
            SourceFlag = sourceFlag;
            Trace = trace;
            Type = type;
            MatchList = null;
            ColorGroup = 0;
        }

        public string GetHighlightedMsg(List<Color> colors)
        {
            StringBuilder stringBuilder = new();

            int colorGroupStart;
            int colorGroupLength;
            int colorGroupNumber;
            Color color;

            foreach (Tuple<int, int, int> colorColorGroup in _msgColorGroupList)
            {
                colorGroupStart  = colorColorGroup.Item1;
                colorGroupLength = colorColorGroup.Item2;
                colorGroupNumber = colorColorGroup.Item3;

                if (colorGroupNumber != _fullMatchColorGroup)
                {
                    color = colors[colorGroupNumber];
                    stringBuilder.Append(string.Format("<mark=#{0:x2}{1:x2}{2:x2}>", color.r * 255, color.g * 255, color.b * 255));
                    stringBuilder.Append(Msg, colorGroupStart, colorGroupLength);
                    stringBuilder.Append("</mark>");
                }
                else stringBuilder.Append(Msg, colorGroupStart, colorGroupLength);
            }

            return stringBuilder.ToString();
        }

        public bool IsType(MessageTypeOptions typeOptions)
        {
            return typeOptions.HasFlag((MessageTypeOptions)Type);
        }

        public bool IsFromSource(int sourceFlags)
        {
            return (sourceFlags & SourceFlag) > 0;
        }

        internal void SetId(int id)
        {
            Id = id;
        }

        internal void SetMatchList(IReadOnlyList<IReadOnlyMsgObject> matchList)
        {
            MatchList = matchList;
        }

        internal void SetColorGroup(int colorGroup, int fullMatchColorGroup)
        {
            ColorGroup = colorGroup;
            _fullMatchColorGroup = fullMatchColorGroup;
        }

        internal void SetStringColorGroupList(IReadOnlyList<Tuple<int, int, int>> msgColorGroupList)
        {
            _msgColorGroupList = msgColorGroupList;
        }

        private IReadOnlyList<Tuple<int, int, int>> _msgColorGroupList;
        private int _fullMatchColorGroup;
    }

    internal class Timestamp : IReadOnlyTimestamp
    {
        public int Time { get; }
        public MessageType Type { get; private set; }
        public int SourceTypes { get; private set; }
        public IReadOnlyList<IReadOnlyMsgObject> List => _list;
        public IReadOnlyList<MsgObject> MutableList => _list;

        internal Timestamp(int time)
        {
            Time = time;
            Type = MessageType.None;
            SourceTypes = 0;
            _list = new();
        }

        internal void AddMsg(MsgObject msgObject)
        {
            _list.Add(msgObject);
            if(Type < msgObject.Type) Type = msgObject.Type;
            SourceTypes |= msgObject.SourceFlag;
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
                if (msgObject.IsType(wantedTypes))
                {
                    newList.Add(msgObject);
                }
            }
            return newList;
        }

        public int ClientCount => _msgLists.Length;

        internal MsgStorage(int clientCount)
        {
            _timelineStorage = new(clientCount);
            _msgLists = new List<MsgObject>[clientCount];
            _timeStampMaps = new Dictionary<int, Timestamp>[clientCount];
            _globalTimeSet = new();
            for (int i = 0; i < clientCount; i++)
            {
                _msgLists[i] = new();
                _timeStampMaps[i] = new();
            }
        }

        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client)
        {
            if (!IsValidClient(client)) return null;
            return _msgLists[client];
        }

        public IReadOnlyList<MsgObject> AllMutableMsgs(int client)
        {
            if (!IsValidClient(client)) return null;
            return _msgLists[client];
        }

        internal void Add(MsgObject msg)
        {
            if (IsValidClient(msg.Client))
            {
                msg.SetId(_msgLists[msg.Client].Count);
                _msgLists[msg.Client].Add(msg);

                Timestamp stamp = _timelineStorage.AddMessageToTimestamp(msg);

                if (!_globalTimeSet.Contains(stamp.Time)) _globalTimeSet.Add(stamp.Time);

                if (!_timeStampMaps[msg.Client].ContainsKey(msg.Time))
                    _timeStampMaps[msg.Client][msg.Time] = stamp;
            }
        }

        public int[] GetTimes()
        {
            int[] timeArray = _globalTimeSet.ToArray();
            Array.Sort(timeArray);
            return timeArray;
        }

        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time)
        {
            if (!IsValidClient(client)) return null;
            if (_timeStampMaps[client] == null) return null;
            if (!_timeStampMaps[client].ContainsKey(time)) return null;
            return _timeStampMaps[client][time].List;
        }

        public IReadOnlyList<MsgObject> GetMutableTime(int client, int time)
        {
            if (!IsValidClient(client)) return null;
            if (_timeStampMaps[client] == null) return null;
            if (!_timeStampMaps[client].ContainsKey(time)) return null;
            return _timeStampMaps[client][time].MutableList;
        }

        public IReadOnlyTimelineStorage GetTimelineStorage()
        {
            return _timelineStorage;
        }

        public bool IsValidClient(int client)
        {
            return ClientCount > client && client >= 0;
        }

        public int TotalMessages()
        {
            int total = 0;
            foreach (List<MsgObject> msgList in _msgLists)
            {
                total += msgList.Count;
            }
            return total;
        }

        public int GetSourceFlagOrNew(string source)
        {
            int i = 0;
            for (; i < _sourceFlagNameList.Count; i++)
            {
                if (_sourceFlagNameList[i] == source) return 1 << i;
            }
            int flag = 1 << i;
            _sourceAllFlags |= flag;
            _sourceFlagList.Add(flag);
            _sourceFlagNameList.Add(source);
            return flag;
        }

        public int GetSourceAllFlags()
        {
            return _sourceAllFlags;
        }

        public IReadOnlyList<int> GetSourceFlagList()
        {
            return _sourceFlagList;
        }

        public string GetSourceFlagName(int sourceFlag)
        {
            return _sourceFlagNameList[(int)Math.Log(sourceFlag, 2)];
        }

        public IReadOnlyList<int> GetColorSuperGroupList()
        {
            return _colorSuperGroupList;
        }

        public void SetColorSuperGroupList(IReadOnlyList<int> colorSuperGroupList)
        {
            _colorSuperGroupList = colorSuperGroupList;
        }

        private readonly List<MsgObject>[] _msgLists;
        private readonly Dictionary<int, Timestamp>[] _timeStampMaps;
        private readonly HashSet<int> _globalTimeSet;
        private readonly TimelineStorage _timelineStorage;

        private int _sourceAllFlags = 0;
        private readonly List<int> _sourceFlagList = new();
        private readonly List<string> _sourceFlagNameList = new();

        private IReadOnlyList<int> _colorSuperGroupList;

        private class TimelineStorage : IReadOnlyTimelineStorage
        {
            public int ClientCount => _timelines.Length;
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

            internal Timestamp AddMessageToTimestamp(MsgObject msg)
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
