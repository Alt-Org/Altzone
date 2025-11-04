using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class ServerOnlinePlayer 
{
    public string _id { get; set; }
    public string name { get; set; }
    public PlayerData PlayerData { get; set; }
    public ClanData ClanData { get; set; }
}
