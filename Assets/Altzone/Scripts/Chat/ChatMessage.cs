using System;
using System.Collections.Generic;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using static ServerChatMessage;

namespace Altzone.Scripts.Chat
{
    /// <summary>
    /// Message instance received and sent using Photon Chat.
    /// </summary>
    [Serializable]
    public class ChatMessage : IEquatable<ChatMessage>
    {
        [SerializeField] private string _id;
        private string _senderId;
        [SerializeField] private string _username;
        private AvatarData _avatar;
        [SerializeField] private string _message;
        [SerializeField] private ChatChannel _channel;
        [SerializeField] private Mood _mood;
        private List<ServerReactions> _reactions;
        private DateTime _timestamp;

        public ChatChannel Channel { get => _channel; }
        public string SenderId { get => _senderId; }
        public string Username { get => _username; }
        public AvatarData Avatar { get => _avatar; }
        public string Message { get => _message; }
        public Mood Mood { get => _mood; }
        public string Id { get => _id; }
        public List<ServerReactions> Reactions { get => _reactions; internal set => _reactions = value; }
        public DateTime Timestamp { get => _timestamp; }

        internal ChatMessage()
        {
            _id = "-1";
            _username = string.Empty;
            _message = string.Empty;
            _channel = new ChatChannel();
            _mood = Mood.None;
            _reactions = new();
        }

        internal ChatMessage(string id, string name, string message, ChatChannel channel, Mood mood)
        {
            _id = id;
            _username = name;
            _message = message;
            _channel = channel;
            _mood = mood;
        }

        internal ChatMessage(ServerChatMessage message)
        {
            _id = message._id;
            _senderId = message.sender._id;
            _username = message.sender.name;
            if(message.sender.avatar != null) _avatar = new(message.sender.name, message.sender.avatar);
            _message = message.content;
            _channel = ChatListener.Instance.GetChatChannel(message.type);
            if(message.feeling != null)_mood = (Mood)Enum.Parse(typeof(Mood), message.feeling);
            _reactions = message.reactions;
            _timestamp = DateTime.Parse(message.createdAt);
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
}
