using System;
using System.Collections.Generic;
using Altzone.Scripts.Voting;

/// <summary>
/// Clan object received from the server
/// </summary>
namespace Altzone.Scripts.Model.Poco.Clan
{
    public class ServerClan
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string tag { get; set; }
        public string phrase { get; set; }
        public List<string> labels { get; set; }
        public int gameCoins { get; set; }
        public int points { get; set; }
        public List<string> admin_ids { get; set; }
        public int playerCount { get; set; }
        public int itemCount { get; set; }
        public int stockCount { get; set; }
        public ClanAge ageRange { get; set; }
        public Language language { get; set; }
        public Goals goal { get; set; }
        public bool isOpen { get; set; }
        public int? furnitureCount { get; set; }
        public int? raidRoomCount { get; set; }
        public List<PollData> polls { get; set; }
        public ClanLogo clanLogo { get; set; }
    }

    public enum ClanLogoType
    {
        None,
        Heart
    }
    [Serializable]
    public class ClanLogo
    {
        public ClanLogoType logoType { get; set; }
        public List<string> pieceColors { get; set; }
    }
}
