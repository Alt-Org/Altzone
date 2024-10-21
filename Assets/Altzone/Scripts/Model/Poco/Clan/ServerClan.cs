using System.Collections.Generic;

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
        public List<string> admin_ids { get; set; }
        public int playerCount { get; set; }
        public int itemCount { get; set; }
        public int stockCount { get; set; }
        public ClanAge clanAge { get; set; }
        public Language language { get; set; }
        public Goals goals { get; set; }
        public bool isOpen { get; set; }
        public int? furnitureCount { get; set; }
        public int? raidRoomCount { get; set; }

        // Lista Voting-objekteja ��nestyst� varten
        public List<Voting> VoteList { get; set; }

        public ServerClan()
        {
            // Alusta ��nestyslista konstruktorissa
            VoteList = new List<Voting>();
        }
    }
}

// Luokka, joka sis�lt�� yksitt�isen ��nestyksen tiedot
public class Voting
{
    public string itemName { get; set; }
    public int votes { get; set; }

    public Voting(string name)
    {
        itemName = name;
        votes = 0;
    }
}
