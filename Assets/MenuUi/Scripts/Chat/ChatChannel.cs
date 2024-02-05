using System;
using UnityEngine;
using static ChatListener;

[Serializable]
public class ChatChannel
{
    [SerializeField] internal string _id;
    [SerializeField] internal string _channelName;
    [SerializeField] internal ChatChannelType _chatChannelType;
    [SerializeField] internal int _firstMsgIndex;
    [SerializeField] internal int _lastMsgIndex;

    internal ChatChannel()
    {
        _id = string.Empty;
        _channelName = string.Empty;
        _chatChannelType = ChatChannelType.None;
        _lastMsgIndex = 0;
        _firstMsgIndex = 1;
    }

    internal ChatChannel(string channelName, ChatChannelType channelType)
    {
        _id = string.Empty;
        _channelName = channelName;
        _chatChannelType = channelType;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }
    internal ChatChannel(string id, string channelName, ChatChannelType channelType)
    {
        _id = id;
        _channelName = channelName;
        _chatChannelType = channelType;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }

    internal void Reset()
    {
        _id = string.Empty;
        _channelName = string.Empty;
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }

    internal void ResetIndexes()
    {
        _lastMsgIndex = 0;
        _firstMsgIndex = 0;
    }
}
