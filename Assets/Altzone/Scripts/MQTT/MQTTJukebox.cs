using System;
using System.Collections.Generic;

namespace Altzone.Scripts.MQTT
{
    [Serializable]
    public class MQTTJukeBoxPlaylist
    {
        public string clanId { get; set; }
        public MQTTJukeBoxSong currentSong { get; set; }
        public List<MQTTJukeBoxQueue> songQueue { get; set; }
    }

    [Serializable]
    public class MQTTJukeBoxSong
    {
        public string id { get; set; }
        public string songId { get; set; }
        public int songDurationSeconds { get; set; }
        public string playerId { get; set; }
        public long startedAt { get; set; }
    }

    [Serializable]
    public class MQTTJukeBoxQueue
    {
        public string id { get; set; }
        public string songId { get; set; }
        public int songDurationSeconds { get; set; }
        public string playerId { get; set; }
    }

    [Serializable]
    public class MQTTCurrentSong
    {
        public string songId { get; set; }
        public long startedAt { get; set; }
    }
}
