using System;
using UnityEngine;
using static ChatListener;

/// <summary>
/// Message instance received and sent using Photon Chat.
/// </summary>
[Serializable]
public class ChatMessage : IEquatable<ChatMessage>
{
    [SerializeField] private int _id;
    [SerializeField] private string _username;
    [SerializeField] private string _message;
    [SerializeField] private ChatChannel _channel;
    [SerializeField] private Mood _mood;

    public ChatChannel Channel { get => _channel;}
    public string Username { get => _username; }
    public string Message { get => _message;}
    public Mood Mood { get => _mood;}
    public int Id { get => _id;}

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

        return _username == other._username && _message == other._message && _channel.ChannelName == other._channel.ChannelName && _channel.ChatChannelType == other._channel.ChatChannelType && _mood == other._mood;
    }

    public override bool Equals(object obj) => Equals(obj as ChatMessage);
    public override int GetHashCode() => (_username, _message, _channel.ChannelName, _channel.ChatChannelType, _mood).GetHashCode();
}
