using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;

namespace Altzone.Scripts.MQTT
{
    [Serializable]
    public class MQTTMatchRoomData
    {
        public string id { get; set; }
        public MatchType matchType { get; set; }
        public InviteStatus status { get; set; }
        public string ownerPlayerId { get; set; }
        public string clanId { get; set; }
        public List<MQTTMatchPlayers> players { get; set; }
        public List<MQTTBots> bots { get; set; }
        public int teamSize { get; set; }
        public bool allowBots { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string readyAt { get; set; }
    }

    [Serializable]
    public class MQTTMatchInvite
    {
        public string id { get; set; }
        public MatchType matchType { get; set; }
        public InviteStatus status { get; set; }
        public MQTTMatchPlayers ownerPlayer{ get; set; }
        public MQTTMatchPlayers senderPlayer { get; set; }
        public string clanId { get; set; }
        public List<MQTTMatchPlayers> players { get; set; }
        public int teamSize { get; set; }
        public bool allowBots { get; set; }
        public string sentAt { get; set; }
    }

    [Serializable]
    public class MqttMatchSet
    {
        public string id { get; set; }
        public MatchType matchType { get; set; }
        public MatchStatus status { get; set; }
        public int teamSize { get; set; }
        public List<MQTTMatchTeams> teams { get; set; }
        public string startedAt { get; set; }
        public string finishedAt { get; set; }
        public MatchResult winningSide { get; set; }
    }

    public enum MatchType
    {
        RANDOM,
        CLAN,
        CUSTOM
    }

    public enum InviteStatus
    {
        OPEN,
        READY,
        QUEUED,
        MATCHED,
        CANCELLED
    }

    public enum MatchStatus
    {
        ACTIVE,
        FINISHED
    }

    [Serializable]
    public class MQTTMatchTeams
    {
        public string side { get; set; }
        public string clanId { get; set; }
        public List<MQTTMatchPlayers> players { get; set; }
        public List<MQTTBots> bots { get; set; }
    }

    [Serializable]
    public class MQTTMatchPlayers
    {
        public string playerId { get; set; }
        public string name { get; set; }
        public ServerAvatar avatar { get; set; }
        public bool isBot { get; set; }
    }

    [Serializable]
    public class MQTTBots
    {
        public string botId { get; set; }
        public string displayName { get; set; }
        public bool isBot { get; set; }
    }

    [Serializable]
    public class MatchResult
    {
        public string winningSide { get; set; }
    }
}
