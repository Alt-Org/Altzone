using System;
using System.Collections;
using System.Collections.Generic;
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
        public MessageType Type { get; }

        internal MsgObject(int client, int time, string msg, MessageType type)
        {
            Client = client;
            Id = -1;
            Time = time;
            Msg = msg;
            Type = type == MessageType.None ? MessageType.Info : type;
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
                if (wantedTypes.HasFlag(msgObject.Type))
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
            if(_timeStampMapList[client] == null) return null;
            if(!_timeStampMapList[client].ContainsKey(time)) return null;
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

        private readonly List<MsgObject>[] _msgList;
        private readonly Dictionary<int, IReadOnlyTimestamp>[] _timeStampMapList;
        private readonly TimelineStorage _timelineStorage;

        private class TimelineStorage : IReadOnlyTimelineStorage
        {
            internal TimelineStorage(int clientCount)
            {
                for (int i = 0; i < clientCount; i++)
                {
                    _timelines.Add(new());
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
                    for (int i = timelineSize; i < time; i++)
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
                return _timelines.Count > client && client >= 0;
            }

            private List<List<Timestamp>> _timelines = new();
        }

    }

}
