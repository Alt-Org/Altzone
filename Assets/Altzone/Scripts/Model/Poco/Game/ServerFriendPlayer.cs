using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ServerFriendPlayer
{
    public string _id { get; set; }
    public string name { get; set; }
    public string clan_id { get; set; }
    public ServerAvatar avatar { get; set; }
}
