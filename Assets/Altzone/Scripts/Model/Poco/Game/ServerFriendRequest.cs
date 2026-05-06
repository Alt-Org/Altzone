using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerFriendRequest
{
    public string friendship_id { get; set; }
    public string direction { get; set; }
    public ServerFriendPlayer friend { get; set; }
    public string clan_id { get; set; }
    public string clanName { get; set; }
}
