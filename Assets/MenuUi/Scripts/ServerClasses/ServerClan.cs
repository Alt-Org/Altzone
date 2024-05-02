using System.Collections.Generic;
using UnityEngine;

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

    // Luokka, joka sis‰lt‰‰ yksitt‰isen ‰‰nestyksen tiedot
    public class Voting
    {
        public string ItemName { get; set; }
        public int Votes { get; set; }

        public Voting(string itemName)
        {
            ItemName = itemName;
            Votes = 0;
        }
    }

    // Luokka, joka edustaa serverilt‰ saapuvaa Clan-oliota


    // Lista Voting-objekteja ‰‰nestyst‰ varten
    public List<Voting> VoteList { get; private set; }

    void Awake()
    {
        // Alusta ‰‰nestyslista
        VoteList = new List<Voting>();
    }

    // Metodi, jolla voi lis‰t‰ uuden ‰‰nestyksen listalle
    public void AddVoting(string itemName)
    {
        VoteList.Add(new Voting(itemName));
    }
}

// Luokka, joka hallinnoi serverin tietoja
public class ServerManager : MonoBehaviour
{
    // Singleton-instanssi ServerManager-luokasta
    public static ServerManager ManagerInstance { get; private set; }

    // ServerClan-instanssi
    public ServerClan Clan { get; private set; }

    void Awake()
    {
        // Varmista, ett‰ on vain yksi instanssi olemassa
        if (ManagerInstance == null)
        {
            ManagerInstance = this;
            DontDestroyOnLoad(gameObject);
            GetComponentInChildren<ServerClan>(); // Haetaan ServerClan-instanssi t‰m‰n GameObjectin alihierarkiasta
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

// Luokka, joka sis‰lt‰‰ yksitt‰isen ‰‰nestyksen tiedot
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
