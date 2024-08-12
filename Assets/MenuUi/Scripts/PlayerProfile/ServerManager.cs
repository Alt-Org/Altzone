using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Altzone.Scripts.Model.Poco.Player;
using System.Globalization;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.GA;
using Photon.Realtime;

/// <summary>
/// ServerManager acts as an interface between the server and the game.
/// Stores information about the logged in Player, Clan, Stock, Items etc.
/// </summary>
public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }

    private ServerPlayer _player;               // Player info from server
    private ServerClan _clan;                   // Clan info from server
    private ServerStock _stock;                 // Stock info from server

    [SerializeField] private bool _automaticallyLogIn = false;
    private int _accessTokenExpiration;
    public bool isLoggedIn = false;
    public static string ADDRESS = "https://altzone.fi/api/";

    #region Delegates & Events

    public delegate void LogInStatusChanged(bool isLoggedIn);
    public static event LogInStatusChanged OnLogInStatusChanged;

    public delegate void LogInFailed();
    public static event LogInFailed OnLogInFailed;

    public delegate void ClanChanged(ServerClan clan);
    public static event ClanChanged OnClanChanged;

    public delegate void ClanInventoryChanged();
    public static event ClanInventoryChanged OnClanInventoryChanged;

    #endregion

    #region Getters & Setters

    public string AccessToken { get => PlayerPrefs.GetString("accessToken", string.Empty); set => PlayerPrefs.SetString("accessToken", value); }
    public int AccessTokenExpiration { get => _accessTokenExpiration; set => _accessTokenExpiration = value; }
    public ServerPlayer Player { get => _player; set => _player = value; }
    public ServerClan Clan
    {
        get => _clan; set
        {
            _clan = value;

            if (Player != null && Clan != null)
                Player.clan_id = Clan._id;
        }
    }
    public ServerStock Stock { get => _stock; set => _stock = value; }

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        if(_automaticallyLogIn) StartCoroutine(LogIn());
    }

    public void Reset()
    {
        Player = null;
        Clan = null;
        Stock = null;

        PlayerPrefs.SetString("accessToken", string.Empty);
        PlayerPrefs.SetString("playerId", string.Empty);
        PlayerPrefs.SetString("profileId", string.Empty);
    }

    /// <summary>
    /// Raises OnClanChanged event when clan has changed.
    /// </summary>
    public void RaiseClanChangedEvent()
    {
        if (OnClanChanged != null)
            OnClanChanged(Clan);
    }

    /// <summary>
    /// Raises ClanInventoryChanged event when clan stock has changed.
    /// </summary>
    public void RaiseClanInventoryChangedEvent()
    {
        if (OnClanInventoryChanged != null)
            OnClanInventoryChanged();
    }

    /// <summary>
    /// Tries to log in player if access token is found.
    /// </summary>
    public IEnumerator LogIn()
    {
        if (Player != null || AccessToken == string.Empty)
        {
            OnLogInFailed();
            yield break;
        }
        else
        {
            // Checks if we can get Player & player Clan from the server
            yield return StartCoroutine(GetPlayerFromServer(player =>
            {
                if (player == null)
                {
                    OnLogInFailed();
                    return;
                }

                SetPlayerValues(player);

                if (Clan == null)
                {
                    StartCoroutine(GetClanFromServer(clan =>
                    {
                        if (clan == null)
                        {
                            return;
                        }

                        RaiseClanChangedEvent();
                        RaiseClanInventoryChangedEvent();
                    }));
                }

            }));
        }
    }

    /// <summary>
    /// Logs player out.
    /// </summary>
    public void LogOut()
    {
        Reset();

        var playerSettings = GameConfig.Get().PlayerSettings;

        // 12345 is the DemoPlayer player in DataStorage
        // If in the future we force log in, this default player is not necessary.
        playerSettings.PlayerGuid = "12345";
        isLoggedIn = false;

        if (OnLogInStatusChanged != null)
            OnLogInStatusChanged(false);

        if (OnClanChanged != null)
            OnClanChanged(null);
    }

    /// <summary>
    /// Sets values related to "Profile" received from server
    /// </summary>
    /// <param name="profileJSON">JSON object containing Profile info from server</param>
    /// <remarks>
    /// Profile and Player are not the same as Profile might hold personal information!
    /// Player contains exclusively data related to in game Player.
    /// </remarks>
    public void SetProfileValues(JObject profileJSON)
    {
        AccessToken = profileJSON["accessToken"].ToString();
        AccessTokenExpiration = int.Parse(profileJSON["tokenExpires"].ToString());
        PlayerPrefs.SetString("playerId", profileJSON["Player"]["_id"].ToString());

        //StartCoroutine(LogIn());
    }

    /// <summary>
    /// Sets Player values from server and saves it to DataStorage.
    /// </summary>
    /// <param name="player">ServerPlayer from server containing the most up to date player data.</param>
    public void SetPlayerValues(ServerPlayer player)
    {
        string clanId = player.clan_id;

        // 12345 is DemoClan in DataStore
        if (clanId == null)
            clanId = "12345";

        // Check if the customplayer index is in DataStorage
        var storefront = Storefront.Get();
        PlayerData playerData = null;

        storefront.GetPlayerData(player.uniqueIdentifier, p => playerData = p);

        int currentCustomCharacterId = playerData == null ? 1 : playerData.SelectedCharacterId;
        int[] currentBattleCharacterIds = playerData == null ? new int[5] : playerData.SelectedCharacterIds;

        PlayerData newPlayerData = null;
        newPlayerData = new PlayerData(player._id, player.clan_id, currentCustomCharacterId, currentBattleCharacterIds, player.name, player.backpackCapacity, player.uniqueIdentifier);

        PlayerPrefs.SetString("profileId", player.profile_id);

        Storefront.Get().SavePlayerData(newPlayerData, null);
        var playerSettings = GameConfig.Get().PlayerSettings;

        playerSettings.PlayerGuid = player.uniqueIdentifier;

        isLoggedIn = true;

        if (OnLogInStatusChanged != null)
            OnLogInStatusChanged(true);
    }

    /// <summary>
    /// Fetches Clan, Stock and Items data from server and saves it to DataStorage.
    /// </summary>
    /// <param name="clan">Clan to be saved.</param>
    /// <returns></returns>
    public IEnumerator SaveClanFromServerToDataStorage(ServerClan clan)
    {
        PlayerData playerData = null;
        ClanData clanData = null;

        var gameConfig = GameConfig.Get();
        var playerSettings = gameConfig.PlayerSettings;
        var playerGuid = playerSettings.PlayerGuid;
        var store = Storefront.Get();
        //yield return null;
        // Checks that the player is found in DataStorage
        store.GetPlayerData(playerGuid, playerDataFromStorage =>
        {
            if (playerDataFromStorage == null)
            {
                return;
            }

            playerData = playerDataFromStorage;

            // Changes player clan and saves it to DataStorage.
            playerData.ClanId = clan._id;
            store.SavePlayerData(playerData, null);

            // Checks if the clan is found in DataStorage or if we have to create new one.
            store.GetClanData(playerData.ClanId, clanDataFromStorage =>
            {
                if (clanDataFromStorage == null)
                {
                    clanData = new ClanData(clan._id, clan.name, clan.tag, clan.gameCoins);
                    return;
                }
                else
                {
                    clanData = clanDataFromStorage;
                }

            });
        });

        // Creates or fetches the most up to date clan Stock before saving.
        if (Stock == null)
        {
            yield return StartCoroutine(GetStockFromServer(clan, stock =>
            {
                if (stock == null)
                {
                    StartCoroutine(PostStockToServer(stock =>
                    {
                        if (stock != null)
                        {
                            Stock = stock;
                        }
                    }));
                }
            }));
        }

        // Get Clan Items
        yield return StartCoroutine(GetStockItemsFromServer(Stock, new List<ServerItem>(), null, 0, items =>
        {
            if (items != null)
            {
                ClanInventory inventory = new ClanInventory();
                List<ClanFurniture> clanFurniture = new List<ClanFurniture>();

                /*foreach (ServerItem item in items)
                {
                    //Debug.LogWarning($"Id: {item._id}, Name: {item.name}");
                    if (item._id == null || item.name == null) continue;
                    clanFurniture.Add(new ClanFurniture(item._id, item.name.Trim().ToLower(CultureInfo.GetCultureInfo("en-US")).Replace(" ", ".")));
                }*/

                if(clanFurniture.Count == 0)
                {
                    int i = 0;
                    while (i < 2)
                    {
                        clanFurniture.Add(new ClanFurniture((10000 + 100 + i).ToString(), "Sofa_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 200 + i).ToString(), "Mirror_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 300 + i).ToString(), "Floorlamp_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 400 + i).ToString(), "Toilet_Schrodinger"));
                        clanFurniture.Add(new ClanFurniture((10000 + 500 + i).ToString(), "Sink_Schrodinger"));
                        clanFurniture.Add(new ClanFurniture((10000 + 600 + i).ToString(), "Closet_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 700 + i).ToString(), "CoffeeTable_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 800 + i).ToString(), "SideTable_Taakka"));
                        clanFurniture.Add(new ClanFurniture((10000 + 900 + i).ToString(), "ArmChair_Taakka"));
                        i++;
                    }

                    for (i = 0; i < Clan.playerCount; i++)
                    {
                        int slotRows = 8;
                        int slotColumn = 20;

                        int furniture1X = UnityEngine.Random.Range(1, slotColumn - 1);
                        int furniture1Y = UnityEngine.Random.Range(1, slotRows);
                        int furniture2X;
                        int furniture2Y;
                        while (true)
                        {
                            furniture2X = UnityEngine.Random.Range(0, slotColumn - 7);
                            furniture2Y = UnityEngine.Random.Range(1, slotRows);
                            if ((furniture2X >= furniture1X - 7 && furniture2X <= furniture1X + 1 && furniture2Y >= furniture1Y - 1 && furniture2Y <= furniture1Y + 2)) continue;
                            else break;
                        }

                        clanFurniture.Add(new ClanFurniture((10000 + 300 + 3 + i).ToString(), "Floorlamp_Taakka", furniture1X, furniture1Y, i, false));
                        clanFurniture.Add(new ClanFurniture((10000 + 100 + 3 + i).ToString(), "Sofa_Taakka", furniture2X, furniture2Y, i, false));

                    }
                }

                inventory.Furniture = clanFurniture;
                clanData.Inventory = inventory;
            }
        }));

        // Saves clan data including its items.
        store.SaveClanData(clanData, null);
    }

    #region Server

    #region Player
    public IEnumerator GetPlayerFromServer(Action<ServerPlayer> callback)
    {
        if (Player != null)
            Debug.LogWarning("Player already exists. Consider using ServerManager.Instance.Player if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(ADDRESS + "player/" + PlayerPrefs.GetString("playerId", string.Empty), AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerPlayer player = result["data"]["Player"].ToObject<ServerPlayer>();
                Player = player;
                Debug.LogWarning(player.clan_id);
                Debug.LogWarning(player.uniqueIdentifier);

                if (callback != null)
                    callback(player);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    #endregion

    #region Clan
    public IEnumerator GetClanFromServer(Action<ServerClan> callback)
    {
        if (Clan != null)
            Debug.LogWarning("Clan already exists. Consider using ServerManager.Instance.Clan if the most up to data data from server is not needed.");

        if (Player.clan_id == null)
            yield break;


        yield return StartCoroutine(WebRequests.Get(ADDRESS + "clan/" + Player.clan_id, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerClan clan = result["data"]["Clan"].ToObject<ServerClan>();
                Clan = clan;

                // Saves clan data to DataStorage
                StartCoroutine(SaveClanFromServerToDataStorage(Clan));

                if (callback != null)
                    callback(Clan);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator GetAllClans(int page, Action<List<ServerClan>, PaginationData> callback)
    {
        string query = ADDRESS + "clan?page=" + page + "&limit=5";

        StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray clans = (JArray)result["data"]["Clan"];

                PaginationData paginationData = result["paginationData"].ToObject<PaginationData>();

                if (callback != null)
                    callback(clans.ToObject<List<ServerClan>>(), paginationData);
            }
            else
            {
                if (callback != null)
                    callback(null, null);
            }
        }));

        yield break;
    }

    public IEnumerator JoinClan(ServerClan clanToJoin, Action<ServerClan> callback)
    {
        string body = @$"{{""clan_id"":""{clanToJoin.id}"",""player_id"":""{Player._id}""}}";

        StartCoroutine(WebRequests.Post(ADDRESS + "clan/join", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                string clanId = result["data"]["Join"]["clan_id"].ToString();
                GameAnalyticsManager.Instance.ClanChange(clanId);
                StartCoroutine(WebRequests.Get(ADDRESS + "clan/" + clanId, AccessToken, request =>
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        JObject result = JObject.Parse(request.downloadHandler.text);
                        ServerClan clan = result["data"]["Clan"].ToObject<ServerClan>();
                        Clan = clan;

                        StartCoroutine(SaveClanFromServerToDataStorage(Clan));

                        if (callback != null)
                            callback(Clan);
                    }
                    else
                    {
                        if (callback != null)
                            callback(null);
                    }
                }));
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));

        yield break;
    }
    public IEnumerator LeaveClan(Action<bool> callback)
    {
        StartCoroutine(WebRequests.Delete(ADDRESS + "clan/join/" + Clan._id, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Clan = null;
                Player.clan_id = null;
                Stock = null;

                PlayerData playerData = null;
                var storefront = Storefront.Get();

                storefront.GetPlayerData(Player.uniqueIdentifier, data => playerData = data);

                if(playerData != null)
                {
                    playerData.ClanId = "12345";                    //Demo-clan for not logged in players
                    storefront.SavePlayerData(playerData, null);
                }

                if (callback != null)
                    callback(true);
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        }));

        yield break;
    }

    public IEnumerator PostClanToServer(string name, string tag, int coins, bool isOpen, Action<ServerClan> callback)
    {
        string body = @$"{{""name"":""{name}"",""tag"":""{tag}"",""gameCoins"":{coins},""isOpen"":{isOpen.ToString().ToLower()}}}";

        yield return StartCoroutine(WebRequests.Post(ADDRESS + "clan", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerClan clan = result["data"]["Clan"].ToObject<ServerClan>();
                Clan = clan;
                Player.clan_id = Clan._id;

                ClanData clanData = null;
                clanData = new ClanData(Clan._id, Clan.name, Clan.tag, Clan.gameCoins);
                Storefront.Get().SaveClanData(clanData, null);

                if (callback != null)
                {
                    callback(Clan);
                    RaiseClanChangedEvent();
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(null);
                }
            }
        }));

        if (Clan != null && Clan.stockCount == 0)
        {
            yield return StartCoroutine(PostStockToServer(stock =>
            {
                if (callback != null)
                {
                    Stock = stock;
                }
            }));
        }
    }

    #endregion


    #region Stock
    public IEnumerator GetStockFromServer(ServerClan clan, Action<ServerStock> callback)
    {
        if (Stock != null)
            Debug.LogWarning("Stock already exists. Consider using ServerManager.Instance.Stock if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(ADDRESS + "stock?search=clan_id=\"" + clan._id + "\"", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Stock = result["data"]["Stock"][0].ToObject<ServerStock>();     // Clan can have multiple stock but for now we get only the first

                if (callback != null)
                {
                    callback(Stock);
                }
                else
                {
                    if (callback != null)
                    {
                        callback(null);
                    }
                }
            }
        }));
    }
    public IEnumerator GetStockItemsFromServer(ServerStock stock, List<ServerItem> serverItems, PaginationData paginationData, int pageCount, Action<List<ServerItem>> callback)
    {
        if (stock == null)
            yield break;

        bool lastPage = true;
        string query = string.Empty;

        if (paginationData == null)
            query = ADDRESS + "item?limit=10&search=stock_id=\"" + stock._id + "\"";
        else
            query = ADDRESS + "item?page=" + ++paginationData.currentPage + "&limit=10&search=stock_id=\"" + stock._id + "\"";

        yield return StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                List<ServerItem> requestItems = new List<ServerItem>();
                JObject jObject = JObject.Parse(request.downloadHandler.text);
                JArray array = (JArray)jObject["data"]["Item"];
                requestItems = array.ToObject<List<ServerItem>>();

                foreach (var item in requestItems)
                    serverItems.Add(item);

                paginationData = jObject["paginationData"].ToObject<PaginationData>();

                if (paginationData.pageCount != 0)
                    pageCount = paginationData.pageCount;

                if (paginationData != null && paginationData.currentPage < pageCount)
                    lastPage = false;
            }
            else
            {
                string debugString = "Could not fetch items from stock!";

                if (request.responseCode == 404)
                {
                    debugString += " The stock might not have any items.";
                }

                Debug.Log(debugString);

                return;
            }
        }));

        if (!lastPage)
            yield return StartCoroutine(GetStockItemsFromServer(stock, serverItems, paginationData, pageCount, null));

        if (callback != null)
        {
            callback(serverItems);
        }
        else
        {
            if (callback != null)
            {
                callback(null);
            }
        }
    }
    public IEnumerator PostStockToServer(Action<ServerStock> callback)
    {
        if (Stock != null)
            yield break;

        string body = string.Empty;

        if (Clan.stockCount == 0)
        {
            body = @$"{{""type"":0,""rowCount"":5,""columnCount"":10,""clan_id"":""{Clan._id}""}}";

            StartCoroutine(WebRequests.Post(ADDRESS + "stock", body, AccessToken, request =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    JObject result = JObject.Parse(request.downloadHandler.text);
                    Stock = result["data"]["Stock"].ToObject<ServerStock>();

                    if (callback != null)
                        callback(Stock);
                }
                else
                {
                    if (callback != null)
                        callback(null);
                }
            }));
        }

        yield break;
    }

    #endregion

    #endregion
}
