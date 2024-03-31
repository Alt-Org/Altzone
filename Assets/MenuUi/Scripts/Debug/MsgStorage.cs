using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public enum messageType
    {
        None,
        Info,
        Warning,
        Error
    }

    internal interface IReadOnlyMsgObject
    {
        public int Client { get; }
        public int Id { get;}
        public int Time { get; }
        public string Msg { get; }
        public messageType Type { get; }
    }
    internal class MsgObject : IReadOnlyMsgObject
    {
        public int Client { get; }
        public int Id { get; private set; }
        public int Time { get; }
        public string Msg { get; }
        public messageType Type { get; }

        internal MsgObject(int client, int time, string msg, messageType type)
        {
            Client = client;
            Id = -1;
            Time = time;
            Msg = msg;
            Type = type;
        }

        internal void SetId(int id)
        {
            Id = id;
        }
    }

    internal interface IReadOnlyTimestamp
    {
        public int Time { get; }
        public messageType Type { get; }
        public IReadOnlyList<IReadOnlyMsgObject> List { get; }
    }

    internal class Timestamp : IReadOnlyTimestamp
    {
        public int Time { get; }
        public messageType Type { get; private set; }
        public IReadOnlyList<IReadOnlyMsgObject> List { get; }

        internal Timestamp(int time)
        {
            Time = time;
            Type = messageType.None;
            _list = new();
        }

        internal void AddMsg(MsgObject msgObject)
        {
            _list.Add(msgObject);
            if(Type < msgObject.Type) Type = msgObject.Type;
        }

        private readonly List<MsgObject> _list;
    }

    internal interface IReadOnlyMsgStorage
    {
        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client);
        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time);
    }

    internal class MsgStorage : IReadOnlyMsgStorage
    {
        internal MsgStorage(TimelineStorage timelineStorage)
        {
            _timelineStorage = timelineStorage;
            for (int i = 0; i < 4; i++)
            {
                _msgList.Add(null);
                _timeStampMapList.Add(null);
            }
        }

        public IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client) { return _msgList[client]; }

        internal void Add(MsgObject msg)
        {
            msg.SetId(_msgList.Count);
            _msgList[msg.Client].Add(msg);

            Timestamp stamp = _timelineStorage.AddMessageToTimestamp(msg);

            if(!_timeStampMapList[msg.Client].ContainsKey(msg.Time))
                _timeStampMapList[msg.Client][msg.Time] = stamp;
        }
        public IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time)
        {
            return _timeStampMapList[client][time].List;
        }

        private List<List<MsgObject>> _msgList = new();
        private List<Dictionary<int, Timestamp>> _timeStampMapList = new();
        private TimelineStorage _timelineStorage;
    }

    internal interface IReadOnlyTimelineStorage
    {
        public IReadOnlyTimestamp GetTimestamp(int client, int time);
        public IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client);
    }

    internal class TimelineStorage : IReadOnlyTimelineStorage
    {
        internal TimelineStorage()
        {
            for (int i = 0; i < 4; i++)
            {
                _timelines.Add(null);
            }
        }

        public IReadOnlyTimestamp GetTimestamp(int client, int time)
        {
            return _timelines[client][time];
        }
        public IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client)
        {
            return _timelines[client];
        }
        private Timestamp GetOrNew(int client, int time)
        {
            if (_timelines[client] == null)
            {
                _timelines[client] = new();
            }
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
        internal Timestamp AddMessageToTimestamp(MsgObject msg)
        {
            Timestamp stamp = GetOrNew(msg.Client, msg.Time);
            stamp.AddMsg(msg);
            return stamp;

        }

        private List<List<Timestamp>> _timelines = new();
        }
    }
