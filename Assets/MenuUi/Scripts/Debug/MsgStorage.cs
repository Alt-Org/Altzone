using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{

    #region Enum and Interfaces
    public enum MessageType
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
        public static List<MsgObject> Sort(IReadOnlyList<MsgObject> list, HashSet<MessageType> typeSet)
        {
            return new List<MsgObject>();
        }


        internal MsgStorage()
        {
            /*HashSet<MessageType> set = new()
            {
                MessageType.Info,
                MessageType.Warning,
                MessageType.Error
            };
            Sort(new List<MsgObject>(), set);*/

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

        public IReadOnlyTimelineStorage GetTimelineStorage()
        {
            return _timelineStorage;
        }

        private readonly List<List<MsgObject>> _msgList = new();
        private readonly List<Dictionary<int, Timestamp>> _timeStampMapList = new();
        private readonly TimelineStorage _timelineStorage = new();

        private class TimelineStorage : IReadOnlyTimelineStorage
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

}
