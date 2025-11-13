public class ServerSong
{
    public string id;
    public string songId;
    public string playerId;
    public int songDurationSeconds;

    [Newtonsoft.Json.JsonConstructor]
    public ServerSong(string id, string songId, string playerId, int songDurationSeconds)
    {
        this.id = id;
        this.songId = songId;
        this.playerId = playerId;
        this.songDurationSeconds = songDurationSeconds;
    }

    public ServerSong(ServerCurrentSong serverCurrentSong)
    {
        id = serverCurrentSong.id;
        songId = serverCurrentSong.songId;
        playerId = serverCurrentSong.playerId;
        songDurationSeconds = -1;
    }
}

