using System;
using UnityEngine;
using static ChatListener;

/// <summary>
/// Class containing information about individual chat channel. Chat channel can be Global, Clan or Country. This is determined by chat channels ChatChannelType
/// </summary>
[Serializable]
public class ChatChannel
{
    [SerializeField] private string _id;
    [SerializeField] private string _channelName;
    [SerializeField] private ChatChannelType _chatChannelType;
    [SerializeField] private int _firstMsgIndex;
    [SerializeField] private int _lastMsgIndex;

    public string ChannelName { get => _channelName;}
    public string Id { get => _id;}
    public ChatChannelType ChatChannelType { get => _chatChannelType;}
    public int FirstMsgIndex { get => _firstMsgIndex;}
    public int LastMsgIndex { get => _lastMsgIndex;}

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

    public void Reset()
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
