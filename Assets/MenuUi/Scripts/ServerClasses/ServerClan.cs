using System.Collections.Generic;

/// <summary>
/// Clan object received from the server
/// </summary>
public class ServerClan
{
    public string _id { get; set; }
    public string name { get; set; }
    public string tag { get; set; }
    public int gameCoins { get; set; }
    public List<string> admin_ids { get; set; }
    public int playerCount { get; set; }
    public int itemCount { get; set; }
    public int stockCount { get; set; }
    public bool isOpen { get; set; }
    public string id { get; set; }
    public int? furnitureCount { get; set; }
    public int? raidRoomCount { get; set; }
}
