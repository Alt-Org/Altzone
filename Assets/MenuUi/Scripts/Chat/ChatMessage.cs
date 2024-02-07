using System;
using UnityEngine;
using static ChatListener;

/// <summary>
/// Message instance received and sent using Photon Chat.
/// </summary>
[Serializable]
public class ChatMessage : IEquatable<ChatMessage>
{
    [SerializeField] internal int _id;
    [SerializeField] internal string _username;
    [SerializeField] internal string _message;
    [SerializeField] internal ChatChannel _channel;
    [SerializeField] internal Mood _mood;

    internal ChatMessage()
    {
        _id = -1;
        _username = string.Empty;
        _message = string.Empty;
        _channel = new ChatChannel();
        _mood = Mood.None;
    }

    internal ChatMessage(int id, string name, string message, ChatChannel channel, Mood mood)
    {
        _id = id;
        _username = name;
        _message = message;
        _channel = channel;
        _mood = mood;
    }

    public bool Equals(ChatMessage other)
    {
        if (other is null)
            return false;

        return _username == other._username && _message == other._message && _channel._channelName == other._channel._channelName && _channel._chatChannelType == other._channel._chatChannelType && _mood == other._mood;
    }

    public override bool Equals(object obj) => Equals(obj as ChatMessage);
    public override int GetHashCode() => (_username, _message, _channel._channelName, _channel._chatChannelType, _mood).GetHashCode();
}
