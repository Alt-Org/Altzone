using System.Collections.Generic;

public class ServerPlaylist
{
    public ServerCurrentSong currentSong { get; set; }
    public List<ServerSong> songQueue { get; set; }
}
