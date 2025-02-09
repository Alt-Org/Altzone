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
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.Assertions;
using Altzone.Scripts.Model;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    [SerializeField] private bool _skipServerFurniture = false;
    public static string ADDRESS = "https://altzone.fi/api/";
    public static string DEVADDRESS = "https://devapi.altzone.fi/";


    #region Delegates & Events

    public delegate void LogInStatusChanged(bool isLoggedIn);
    public static event LogInStatusChanged OnLogInStatusChanged;

    public delegate void LogInFailed();
    public static event LogInFailed OnLogInFailed;

    public delegate void ClanFetchFinished();
    public static event ClanFetchFinished OnClanFetchFinished;

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
        if (_automaticallyLogIn) StartCoroutine(LogIn());
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
            OnLogInFailed?.Invoke();
            yield break;
        }
        else
        {
            // Checks if we can get Player & player Clan from the server
            yield return StartCoroutine(GetPlayerFromServer(player =>
            {
                if (player == null)
                {
                    OnLogInFailed?.Invoke();
                    return;
                }
                bool gettingTasks = true;
                StartCoroutine(GetPlayerTasksFromServer(tasks =>
                {
                    if (tasks == null)
                    {
                        Debug.LogError("Failed to fetch task data.");
                        gettingTasks = false;
                    }
                    else
                    {
                        Storefront.Get().SavePlayerTasks(tasks, tasks =>
                        {
                            gettingTasks = false;
                        });
                    }
                }));
                new WaitUntil(() => gettingTasks == false);
                SetPlayerValues(player);

                if (OnLogInStatusChanged != null)
                    OnLogInStatusChanged(true);

                if (Clan == null)
                {
                    StartCoroutine(GetClanFromServer(clan =>
                    {
                        if (OnClanFetchFinished != null)
                            OnClanFetchFinished();
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
    /// Sets values related to player "Profile" received from server
    /// </summary>
    /// <param name="profileJSON">JSON object containing Profile info from server</param>
    /// <remarks>
    /// Profile and Player (<c>ServerPlayer</c>) are not the same as Profile might hold personal information!<br />
    /// Player contains exclusively data related to in game Player.
    /// </remarks>
    public void SetProfileValues(JObject profileJSON)
    {
        var accessToken = profileJSON["accessToken"];
        Assert.IsNotNull(accessToken);
        AccessToken = (string)accessToken;

        var tokenExpires = profileJSON["tokenExpires"];
        Assert.IsNotNull(tokenExpires);
        AccessTokenExpiration = tokenExpires.Value<int>();

        var player = profileJSON["Player"];
        Assert.IsNotNull(player);
        PlayerPrefs.SetString("playerId", (string)player["_id"] ?? string.Empty);

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

        int currentCustomCharacterId = (int)(player?.currentAvatarId == null ? (playerData == null? 0:playerData.SelectedCharacterId) : player.currentAvatarId);
        int[] currentBattleCharacterIds = player?.battleCharacter_ids == null ? (playerData == null ? new int[3] : playerData.SelectedCharacterIds) : Array.ConvertAll(player.battleCharacter_ids, s => int.Parse(s));

        PlayerData newPlayerData = null;
        newPlayerData = new PlayerData(player._id, player.clan_id, currentCustomCharacterId, currentBattleCharacterIds, player.name, player.backpackCapacity, player.uniqueIdentifier);

        PlayerPrefs.SetString("profileId", player.profile_id);

        Storefront.Get().SavePlayerData(newPlayerData, null);
        var playerSettings = GameConfig.Get().PlayerSettings;

        playerSettings.PlayerGuid = player.uniqueIdentifier;

        isLoggedIn = true;

        //if (OnLogInStatusChanged != null)
        //    OnLogInStatusChanged(true);
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

            clanData = new ClanData(clan);

            // Checks if the clan is found in DataStorage or if we have to create new one.
            /*
            store.GetClanData(playerData.ClanId, clanDataFromStorage =>
            {
                if (clanDataFromStorage == null)
                {
                    clanData = new ClanData(clan);
                    return;
                }
                else
                {
                    clanData = clanDataFromStorage;
                }

            }); */
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
                if (!_skipServerFurniture)
                {
                    foreach (ServerItem item in items)
                    {
                        //Debug.LogWarning($"Id: {item._id}, Name: {item.name}");
                        if (item._id == null || item.name == null) continue;
                        clanFurniture.Add(new ClanFurniture(item._id, item.name/*.Trim().ToLower(CultureInfo.GetCultureInfo("en-US")).Replace(" ", ".")*/));
                    }
                }
                else
                {
                    ReadOnlyCollection<GameFurniture> baseFurniture = null;
                    store.GetAllGameFurnitureYield(result => baseFurniture = result);
                    clanFurniture = CreateDefaultModels.CreateDefaultDebugFurniture(baseFurniture);
                }

                inventory.Furniture = clanFurniture;
                clanData.Inventory = inventory;
            }
        }));

        yield return StartCoroutine(GetClanPlayers(members =>
        {
            clanData.Members = members;
        }));

        // Saves clan data including its items.
        store.SaveClanData(clanData, null);
    }

    public IEnumerator GetClanPlayers(Action<List<ClanMember>> callback)
    {
        if (Clan == null)
        {
            Debug.LogWarning("Local Clan data not found. Likely reason is that the person is not a member of a clan.");
            yield break;
        }
        else
        {
            yield return StartCoroutine(GetClanMembersFromServer(members =>
            {
                if (members != null)
                {
                    foreach (ClanMember player in members)
                    {
                        //Debug.Log(player.Name);
                    }
                    callback(members);
                }
                else
                    callback(null);
            }));
        }
    }

    #region Server

    #region Player
    public IEnumerator GetPlayerFromServer(Action<ServerPlayer> callback)
    {
        if (Player != null)
            Debug.LogWarning("Player already exists. Consider using ServerManager.Instance.Player if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "player/" + PlayerPrefs.GetString("playerId", string.Empty), AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                ServerPlayer player = result["data"]["Player"].ToObject<ServerPlayer>();
                Player = player;

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

    public IEnumerator UpdatePlayerToServer(string player, Action<ServerPlayer> callback)
    {
        if (Player == null)
        {
            Debug.LogError("Cannot find Player.");
            yield break;
        }

        //JObject body = JObject.FromObject(player);

        //Debug.Log(player);

        yield return StartCoroutine(WebRequests.Put(DEVADDRESS + "player/", player, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                ServerPlayer playerInfo = Player;


                //Player = playerInfo;

                if (callback != null)
                    callback(playerInfo);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator GetPlayerTasksFromServer(Action<PlayerTasks> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "latest-release/playerTasks?period=month", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerPlayerTasks tasks = result["data"]["PlayerTask"].ToObject<ServerPlayerTasks>();
                //Clan = clan;

                if (callback != null)
                    callback(new(tasks));
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
        {
            if (callback != null)
                callback(Clan);
            yield break;
        }


        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "clan/" + Player.clan_id, AccessToken, request =>
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
        string query = DEVADDRESS + "clan?page=" + page + "&limit=5";

        StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
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

    public IEnumerator GetClanMembersFromServer(Action<List<ClanMember>> callback)
    {
        /*if (Clan != null)
            Debug.LogWarning("Clan already exists. Consider using ServerManager.Instance.Clan if the most up to data data from server is not needed.");

        if (Player.clan_id == null)
            yield break;*/


        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "clan/" + Player.clan_id + "?with=Player", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                List<ClanMember> members = new List<ClanMember>();
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray middleresult = result["data"]["Clan"]["Player"] as JArray;
                foreach (var value in middleresult)
                {
                    members.Add(new(value.ToObject<ServerPlayer>()));
                }

                // Saves clan data to DataStorage
                //StartCoroutine(SaveClanFromServerToDataStorage(Clan));

                if (callback != null)
                    callback(members);
            }
            else
            {
                Debug.LogWarning("Failed to get players from clan.");
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator JoinClan(ServerClan clanToJoin, Action<ServerClan> callback)
    {
        string body = JObject.FromObject(new{clan_id=clanToJoin._id,player_id=Player._id}).ToString();

        StartCoroutine(WebRequests.Post(DEVADDRESS + "clan/join", body, AccessToken, request =>
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
        string body = @$"{{""player_id"":""{Player._id}""}}";

        StartCoroutine(WebRequests.Post(DEVADDRESS + "clan/leave", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Clan = null;
                Player.clan_id = null;
                Stock = null;

                PlayerData playerData = null;
                var storefront = Storefront.Get();

                storefront.GetPlayerData(Player.uniqueIdentifier, data => playerData = data);

                if (playerData != null)
                {
                    playerData.ClanId = "12345";                    //Demo-clan for not logged in players
                    storefront.SavePlayerData(playerData, null);
                }
                else
                {
                    playerData.ClanId = "0";
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

    public IEnumerator PostClanToServer(ServerClan clan, Action<ServerClan> callback)
    {
        string body = JObject.FromObject(clan, JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "clan", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerClan clan = result["data"]["Clan"].ToObject<ServerClan>();
                Clan = clan;
                Player.clan_id = Clan._id;

                ClanData clanData = null;
                clanData = new ClanData(Clan);
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

    public IEnumerator UpdateClanToServer(ClanData data, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new {
                _id=data.Id,
                name=data.Name,
                tag=data.Tag,
                isOpen=Clan.isOpen,
                labels = data.Labels,
                ageRange=data.ClanAge,
                goal=data.Goals,
                phrase=data.Phrase,
                language=data.Language
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Put(DEVADDRESS + "clan", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Storefront.Get().SaveClanData(data, null);

                if (callback != null)
                {
                    callback(true);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(false);
                }
            }
        }));
    }

    #endregion

    #region BattleCharacter

    public IEnumerator GetCustomCharactersFromServer(Action<List<CustomCharacter>> callback)
    {
        if (Player != null)
            Debug.LogWarning("Player already exists. Consider using ServerManager.Instance.Player if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "player/" + PlayerPrefs.GetString("playerId", string.Empty), AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                List<ServerCharacter> serverCharacterList = ((JArray)result["data"]["CustomCharacter"]).ToObject<List<ServerCharacter>>();

                List<CustomCharacter> characterList = new();

                foreach (ServerCharacter character in serverCharacterList)
                {
                    characterList.Add(new(character));
                }

                if (callback != null)
                    callback(characterList);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator UpdateCustomCharactersToServer(CustomCharacter character, Action<bool> callback)
    {
        if (character == null)
        {
            Debug.LogError("Cannot find Player.");
            yield break;
        }

        ServerCharacter serverCharacter = new(character);

        string body = JObject.FromObject(serverCharacter).ToString();

        //Debug.Log(player);

        yield return StartCoroutine(WebRequests.Put(DEVADDRESS + "customCharacter/", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (callback != null)
                    callback(true);
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        }));
    }

    public IEnumerator AddCustomCharactersToServer(CharacterID id, Action<bool> callback)
    {
        if (id.Equals(CharacterID.None))
        {
            Debug.LogError("Cannot find Player.");
            yield break;
        }

        ServerCharacter serverCharacter = new(id);

        string body = JObject.FromObject(serverCharacter).ToString();

        //Debug.Log(player);

        yield return StartCoroutine(WebRequests.Put(DEVADDRESS + "customCharacter/", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (callback != null)
                    callback(true);
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        }));
    }

    #endregion

    #region Stock
    public IEnumerator GetStockFromServer(ServerClan clan, Action<ServerStock> callback)
    {
        if (Stock != null)
            Debug.LogWarning("Stock already exists. Consider using ServerManager.Instance.Stock if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "stock?search=clan_id=\"" + clan._id + "\"", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
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
        {
            Debug.LogWarning("Cannot get furniture form server: Cannot find stock");
            yield break;
        }

        bool lastPage = true;
        string query = string.Empty;

        if (paginationData == null)
            query = DEVADDRESS + "stock/" + stock._id + "?limit=10&with=Item";
        else
            query = DEVADDRESS + "stock/" + stock._id + "?page=" + ++paginationData.currentPage + "&limit=10&with=Item";

        yield return StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                List<ServerItem> requestItems = new List<ServerItem>();
                JObject jObject = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(jObject);
                JArray array = (JArray)jObject["data"]["Stock"]["Item"];
                requestItems = array.ToObject<List<ServerItem>>();

                foreach (var item in requestItems)
                    serverItems.Add(item);

                paginationData = jObject["paginationData"]?.ToObject<PaginationData>();

                if (paginationData != null && paginationData.pageCount != 0)
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

    #region Battle

    public void SendDebugLogFile(List<IMultipartFormSection> formData, string secretKey, string id, Action<UnityWebRequest> callback)
    {
        StartCoroutine(WebRequests.Post(DEVADDRESS + "gameAnalytics/logfile/", formData, AccessToken, secretKey, id, callback));
    }

    #endregion

    #region Leaderboard
    public IEnumerator GetClanLeaderboardFromServer(Action<List<ClanLeaderboard>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "latest-release/leaderboard/clan", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray jArray = (JArray)result["data"]["Clan"];
                Debug.LogWarning(result);
                List<ServerClan> clans = jArray.ToObject<List<ServerClan>>();
                List<ClanLeaderboard> clansLeaderBoard = new();
                foreach (ServerClan clan in clans)
                {
                    clansLeaderBoard.Add(new(clan));
                }
                //Clan = clan;

                if (callback != null)
                    callback(clansLeaderBoard);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator GetPlayerLeaderboardFromServer(Action<List<PlayerLeaderboard>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "latest-release/leaderboard/player", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray jArray = (JArray)result["data"]["Player"];
                List<ServerPlayer> players = jArray.ToObject<List<ServerPlayer>>();
                List<PlayerLeaderboard> playersLeaderBoard = new();
                foreach (ServerPlayer player in players)
                {
                    playersLeaderBoard.Add(new(player));
                }
                //Clan = clan;

                if (callback != null)
                    callback(playersLeaderBoard);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }
    #endregion

    #endregion
}
