using Altzone.Scripts.MQTT;

public class ServerCurrentSong
{
    public string id;
    public string songId;
    public string playerId;
    public long startedAt;

    [Newtonsoft.Json.JsonConstructor]
    public ServerCurrentSong(string id, string songId, string playerId, long startedAt)
    {
        this.id = id;
        this.songId = songId;
        this.playerId = playerId;
        this.startedAt = startedAt;
    }

    public ServerCurrentSong(MQTTJukeBoxSong song)
    {
        id = song.id;
        songId = song.songId;
        playerId = song.playerId;
        startedAt = song.startedAt;
    }

    public ServerCurrentSong(ServerSong songFromList, MQTTCurrentSong newsong)
    {
        id = songFromList.id;
        songId = songFromList.songId;
        playerId = songFromList.playerId;
        startedAt = newsong.startedAt;
    }

    public ServerCurrentSong(MQTTCurrentSong song)
    {
        id = null;
        songId = song.songId;
        playerId = null;
        startedAt = song.startedAt;
    }
}
