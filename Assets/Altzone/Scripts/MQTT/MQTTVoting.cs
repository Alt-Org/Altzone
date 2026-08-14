using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Voting;
using UnityEngine;

namespace Altzone.Scripts.MQTT
{
    [Serializable]
    public abstract class MQTTVotingStartedNotification<T>
    {
        public string status { get; set; }
        public string voting_id { get; set; }
        public string type { get; set; }
        public virtual T entity { get; set; }
        public MQTTPollPlayer organizer { get; set; }
    }

    [Serializable]
    public class MQTTFurnitureNotification : MQTTVotingStartedNotification<PollFurniture>
    {
    }

    [Serializable]
    public class PollFurniture
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string shopItemName { get; set; }
        public string price { get; set; }
    }

    [Serializable]
    public class MQTTVotingUpdatedNotification
    {
        public string status { get; set; }
        public string voting_id { get; set; }
        public MQTTPollPlayer voter { get; set; }
        public List<string> votes { get; set; }
    }

    [Serializable]
    public class MQTTVotingEndedNotification
    {
        public string status { get; set; }
        public string voting_id { get; set; }
        public List<ServerPoll> votes { get; set; }
        public string endedAt { get; set; }
    }

    [Serializable]
    public class MQTTPollPlayer
    {
        public string _id { get; set; }
        public string name { get; set; }
        public ServerAvatar avatar { get; set; }
    }
}
