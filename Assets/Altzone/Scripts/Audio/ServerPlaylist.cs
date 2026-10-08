using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.MQTT;
using Newtonsoft.Json;

public class ServerPlaylist
{
    public ServerCurrentSong currentSong { get; set; }
    public List<ServerSong> songQueue { get; set; }

    [Newtonsoft.Json.JsonConstructor]
    public ServerPlaylist(ServerCurrentSong song, List<ServerSong> queue)
    {
        currentSong = song;
        songQueue = queue;
    }

    public ServerPlaylist(MQTTJukeBoxPlaylist mqttPlaylist)
    {
        currentSong = mqttPlaylist.currentSong != null ? new(mqttPlaylist.currentSong) : null;

        songQueue = mqttPlaylist.currentSong != null ? mqttPlaylist.songQueue.Select(x => new ServerSong(x)).ToList() : null;
    }
}
