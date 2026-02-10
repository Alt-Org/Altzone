using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ServerChatMessage
{
    public string _id { get; set; }
    public string clan_id { get; set; }
    public MessageSender sender { get; set; }
    public string recipientPlayer_id { get; set; }
    public string content { get; set; }
    public string type { get; set; }
    public string feeling { get; set; }
    public List<ServerReactions> reactions { get; set; }
    public string createdAt { get; set; }

    public class MessageSender
    {
        public string _id { get; set; }
        public string name { get; set; }
        public ServerAvatar avatar { get; set; }
    }

    public class ServerReactions
    {
        public string _id { get; set; }
        public string playerName { get; set; }
        public string emoji { get; set; }
    }
}
