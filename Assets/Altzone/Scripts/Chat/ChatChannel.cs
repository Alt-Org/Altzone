using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ServerChatMessage;

namespace Altzone.Scripts.Chat
{
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

        private List<ChatMessage> _chatMessages;

        public string ChannelName { get => _channelName; }
        public string Id { get => _id; }
        public ChatChannelType ChatChannelType { get => _chatChannelType; }
        public int FirstMsgIndex { get => _firstMsgIndex; }
        public int LastMsgIndex { get => _lastMsgIndex; }
        public List<ChatMessage> ChatMessages { get => _chatMessages;}

        public delegate void MessageReceived(ChatChannelType chatChannelType, ChatMessage message);
        public static event MessageReceived OnMessageReceived;

        public delegate void ReactionReceived(ChatChannelType chatChannelType, ChatMessage message);
        public static event ReactionReceived OnReactionReceived;

        public delegate void MessageHistoryReceived(ChatChannelType chatChannelType);
        public static event MessageHistoryReceived OnMessageHistoryReceived;

        public delegate void MessageDeleted(ChatChannelType chatChannelType, string id);
        public static event MessageDeleted OnMessageDeleted;

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

        public void SetChatHistory(List<ChatMessage> messageList)
        {
            _chatMessages = messageList;
            OnMessageHistoryReceived?.Invoke(_chatChannelType);
        }

        public void AddNewMessage(ChatMessage message)
        {
            if (!_chatMessages.Contains(message))
            {
                _chatMessages.Add(message);
                OnMessageReceived?.Invoke(_chatChannelType, message);
            }
        }

        public void UpdateReactions(string id, List<ServerReactions> reactions)
        {
            ChatMessage message = _chatMessages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                message.Reactions = reactions;
                OnReactionReceived?.Invoke(_chatChannelType, message);
            }
        }

        public void DeleteMessage(string id)
        {
            ChatMessage message = _chatMessages.FirstOrDefault(m=> m.Id == id);
            if (message != null)
            {
                _chatMessages.Remove(message);
                OnMessageDeleted?.Invoke(_chatChannelType, id);
            }
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
}
