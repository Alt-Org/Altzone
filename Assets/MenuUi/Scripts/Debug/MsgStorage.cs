using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal interface IReadOnlyMsgObject
    {
        internal int Client { get; }
        internal int Id { get;}
        internal int Time { get; }
        internal string Msg { get; }
        internal messageType Type { get; }
    }
    internal class MsgObject : IReadOnlyMsgObject
    {
        int IReadOnlyMsgObject.Client => Client;
        readonly internal int Client;
        int IReadOnlyMsgObject.Id => Id;
        internal int Id { get; private set; }
        int IReadOnlyMsgObject.Time => Time;
        readonly internal int Time;
        string IReadOnlyMsgObject.Msg => Msg;
        readonly internal string Msg;
        messageType IReadOnlyMsgObject.Type => Type;
        readonly internal messageType Type;

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
        internal int Time { get; }
        internal messageType Type { get; }
        internal IReadOnlyList<IReadOnlyMsgObject> List { get; }
    }

    internal class Timestamp : IReadOnlyTimestamp
    {
        internal readonly int Time;
        internal IReadOnlyList<IReadOnlyMsgObject> List => _list;
        internal messageType Type { get; private set;}
        int IReadOnlyTimestamp.Time => Time;
        messageType IReadOnlyTimestamp.Type => Type;
        IReadOnlyList<IReadOnlyMsgObject> IReadOnlyTimestamp.List => _list;

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
        internal IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client);
        internal IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time);
    }

    public enum messageType
    {
        None,
        Info,
        Warning,
        Error
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

        IReadOnlyList<IReadOnlyMsgObject> IReadOnlyMsgStorage.AllMsgs(int client) => AllMsgs(client);

        internal IReadOnlyList<IReadOnlyMsgObject> AllMsgs(int client) { return _msgList[client]; }

        internal void Add(MsgObject msg)
        {
            //if(msg.Time >= _msgList.FindLast())
            msg.SetId(_msgList.Count);
            _msgList[msg.Client].Add(msg);

            Timestamp stamp = _timelineStorage.AddMessageToTimestamp(msg);

            if(!_timeStampMapList[msg.Client].ContainsKey(msg.Time))
                _timeStampMapList[msg.Client][msg.Time] = stamp;
        }
        IReadOnlyList<IReadOnlyMsgObject> IReadOnlyMsgStorage.GetTime(int client, int time) => GetTime(client, time);
        internal IReadOnlyList<IReadOnlyMsgObject> GetTime(int client, int time)
        {
            return _timeStampMapList[client][time].List;
        }

        private List<List<MsgObject>> _msgList = new();
        private List<Dictionary<int, Timestamp>> _timeStampMapList = new();
        private TimelineStorage _timelineStorage;
    }

    internal interface IReadOnlyTimelineStorage
    {
        internal IReadOnlyTimestamp GetTimestamp(int client, int time);
        internal IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client);
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

        IReadOnlyTimestamp IReadOnlyTimelineStorage.GetTimestamp(int client, int time) => GetTimestamp(client, time);

        internal IReadOnlyTimestamp GetTimestamp(int client, int time)
        {
            return _timelines[client][time];
        }
        IReadOnlyList<IReadOnlyTimestamp> IReadOnlyTimelineStorage.GetTimeline(int client) => GetTimeline(client);
        internal IReadOnlyList<IReadOnlyTimestamp> GetTimeline(int client)
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
