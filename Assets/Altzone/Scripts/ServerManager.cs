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
using Altzone.Scripts.Settings;
using UnityEngine.SceneManagement;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Voting;
using System.Linq;
using Altzone.Scripts.Model.Poco;
using Altzone.Scripts.Store;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Audio;

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
    private List<ServerOnlinePlayer> _onlinePlayers;
    private bool _firstJoin = true;

    [SerializeField] private bool _automaticallyLogIn = false;
    private int _accessTokenExpiration;
    private string _accessToken;
    public bool isLoggedIn = false;
    [SerializeField] private bool _skipServerFurniture = false;
    private static string ADDRESS = "https://altzone.fi/api/";
    private static string LATESTDEVBUILDADDRESS = "https://devapi.altzone.fi/latest-release/";
    private static string DEVADDRESS = "https://devapi.altzone.fi/";

    public static string SERVERADDRESS { get
        {
            if(AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild) return DEVADDRESS;
            else return DEVADDRESS;//LATESTDEVBUILDADDRESS;
        }
    }


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

    public delegate void ClanPollsChanged();
    public static event ClanPollsChanged OnClanPollsChanged;

    public delegate void OnlinePlayersChanged(List<ServerOnlinePlayer> onlinePlayers);
    public static event OnlinePlayersChanged OnOnlinePlayersChanged;

    #endregion

    #region Getters & Setters

    public string AccessToken
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_accessToken)) return _accessToken;
            else return PlayerPrefs.GetInt("AutomaticLogin", 0) == 1 ? PlayerPrefs.GetString("accessToken", string.Empty) : string.Empty;
        }
        set
        {
            _accessToken = value;
            PlayerPrefs.SetString("accessToken", value);
        }
    }
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
    public List<ServerOnlinePlayer> OnlinePlayers { get => _onlinePlayers;}
    public bool FirstJoin { get => _firstJoin; set => _firstJoin = value; }

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        OnClanChanged?.Invoke(Clan);
        _firstJoin = false;
    }

    /// <summary>
    /// Raises ClanInventoryChanged event when clan stock has changed.
    /// </summary>
    public void RaiseClanInventoryChangedEvent()
    {
        OnClanInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Raises ClanPollsChanged event when clan polls have changed.
    /// </summary>
    public void RaiseClanPollsChangedEvent()
    {
        OnClanPollsChanged?.Invoke();
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
            bool gettingPlayer = true;
            bool gettingCharacter = true;
            bool gettingTasks = true;
            List<CustomCharacter> characters = null;
            // Checks if we can get Player & player Clan from the server
            yield return StartCoroutine(GetOwnPlayerFromServer(player =>
            {
                if (player == null)
                {
                    OnLogInFailed?.Invoke();
                }
                gettingPlayer = false;
            }));
            yield return new WaitUntil(() => gettingPlayer == false);
            if (Player == null) yield break;
            yield return StartCoroutine(GetCustomCharactersFromServer(characterList =>
            {
                if (characterList == null)
                {
                    Debug.LogError("Failed to fetch Custom Characters.");
                    gettingCharacter = false;
                    characters = null;
                }
                else
                {
                    gettingCharacter = false;
                    characters = characterList;
                }
            }));
            yield return new WaitUntil(() => gettingCharacter == false);


            bool callFinished = false;
            bool characterAdded = false;
            List<CharacterID> newCharacters = new List<CharacterID>();

            DataStore storefront = Storefront.Get();
            PlayerData playerData = null;

            storefront.GetPlayerData(Player.uniqueIdentifier, p => playerData = p);
            if (playerData != null && playerData.SelectedCharacterId != 0 && playerData.SelectedCharacterId != -1)
            {
                var charIds = Enum.GetValues(typeof(CharacterID));
                foreach (CharacterID id in charIds)
                {
                    if (characters == null || characters.FirstOrDefault(x => x.Id == id) == null)
                        if (!CustomCharacter.IsTestCharacter(id) && id != CharacterID.None)
                            newCharacters.Add(id);
                }
                foreach (var character in newCharacters)
                {
                    callFinished = false;
                    StartCoroutine(AddCustomCharactersToServer(character, callback =>
                    {
                        if (callback != null)
                        {
                            Debug.Log("CustomCharacter added: " + character);
                            characterAdded = true;
                        }
                        else
                        {
                            Debug.Log("CustomCharacter adding failed.");
                        }
                        callFinished = true;
                    }));
                    yield return new WaitUntil(() => callFinished == true);
                }
                if (characterAdded)
                {
                    callFinished = false;
                    StartCoroutine(UpdateCustomCharacters((c, characterList) => { callFinished = c; characters = characterList; }));
                }
                new WaitUntil(() => callFinished == true);

                yield return StartCoroutine(GetPlayerTasksFromServer(tasks =>
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
                yield return new WaitUntil(() => gettingTasks == false);
            }
            SetPlayerValues(Player, characters);

            OnLogInStatusChanged?.Invoke(true);
            StartCoroutine(ServiceHeartBeat());

            if (Clan == null)
            {
                StartCoroutine(GetClanFromServer(clan =>
                {
                    OnClanFetchFinished?.Invoke();
                    if (clan == null)
                    {
                        _firstJoin = true;
                        return;
                    }

                    RaiseClanChangedEvent();
                    RaiseClanInventoryChangedEvent();
                }));
            }
        }
    }

    /// <summary>
    /// Logs player out.
    /// </summary>
    public void LogOut()
    {
        Reset();

        PlayerSettings playerSettings = GameConfig.Get().PlayerSettings;

        // 12345 is the DemoPlayer player in DataStorage
        // If in the future we force log in, this default player is not necessary.
        playerSettings.PlayerGuid = "12345";
        isLoggedIn = false;
        _accessToken = null;

        OnLogInStatusChanged?.Invoke(false);

        OnClanChanged?.Invoke(null);
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
        JToken accessToken = profileJSON["accessToken"];
        Assert.IsNotNull(accessToken);
        AccessToken = (string)accessToken;

        JToken tokenExpires = profileJSON["tokenExpires"];
        Assert.IsNotNull(tokenExpires);
        AccessTokenExpiration = tokenExpires.Value<int>();

        JToken player = profileJSON["Player"];
        Assert.IsNotNull(player);
        PlayerPrefs.SetString("playerId", (string)player["_id"] ?? string.Empty);

        //StartCoroutine(LogIn());
    }

    /// <summary>
    /// Sets Player values from server and saves it to DataStorage.
    /// </summary>
    /// <param name="player">ServerPlayer from server containing the most up to date player data.</param>
    public void SetPlayerValues(ServerPlayer player, List<CustomCharacter> characters)
    {
        string clanId = player.clan_id;

        // 12345 is DemoClan in DataStore
        if (clanId == null)
            clanId = "12345";

        // Check if the customplayer index is in DataStorage
        DataStore storefront = Storefront.Get();
        PlayerData playerData = null;

        storefront.GetPlayerData(player.uniqueIdentifier, p => playerData = p);

        if (playerData == null) {
            int currentCustomCharacterId = (int)(player?.currentAvatarId == null ? (int)CharacterID.None : player.currentAvatarId);

            int none = (int)CharacterID.None;
            string noneStr = none.ToString();
            string[] currentBattleCharacterIds = (player?.battleCharacter_ids == null || player.battleCharacter_ids.Length < 3) ? new string[3] { noneStr, noneStr, noneStr } : player.battleCharacter_ids;

            if (characters == null)
            {
                ReadOnlyCollection<CustomCharacter> customCharacters = null;
                storefront.GetAllDefaultCharacterYield(c => customCharacters = c);
                characters = new();
                foreach (CustomCharacter characterItem in customCharacters)
                {
                    characters.Add(characterItem);
                }
            }

            playerData = new PlayerData(player._id, player.clan_id, currentCustomCharacterId, currentBattleCharacterIds, player.name, player.backpackCapacity, player.uniqueIdentifier, characters);
        }
        else
        {
            if (characters == null)
            {
                ReadOnlyCollection<CustomCharacter> customCharacters = null;
                storefront.GetAllDefaultCharacterYield(c => customCharacters = c);
                characters = new();
                foreach (CustomCharacter characterItem in customCharacters)
                {
                    characters.Add(characterItem);
                }
                playerData.BuildCharacterLists(characters);
            }
            else
            {
                foreach (CustomCharacter character in characters)
                {
                    if(!character.AreStatsValid())
                        StartUpdatingCustomCharacterToServer(character);
                }

                playerData.BuildCharacterLists(characters);
            }
            playerData.UpdatePlayerData(player);
        }
        PlayerPrefs.SetString("profileId", player.profile_id);

        Storefront.Get().SavePlayerData(playerData, null);
        PlayerSettings playerSettings = GameConfig.Get().PlayerSettings;

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
        PlayerSettings playerSettings = gameConfig.PlayerSettings;
        string playerGuid = playerSettings.PlayerGuid;
        DataStore store = Storefront.Get();
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

            //clanData = new ClanData(clan);

            // Checks if the clan is found in DataStorage or if we have to create new one.

            store.GetClanData(playerData.ClanId, clanDataFromStorage =>
            {
                if (clanDataFromStorage == null)
                {
                    clanData = new ClanData(clan);
                }
                else
                {
                    clanData = clanDataFromStorage;
                    clanData.UpdateClanData(clan);
                }

            }); 
        });
        yield return new WaitUntil(() => clanData != null);

        yield return StartCoroutine(GetClanPlayers(members =>
        {
            if (members != null) clanData.Members = members;
        }));

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
        bool furnitureFetchFinished = true;
        // Get Clan Items
        yield return StartCoroutine(GetStockItemsFromServer(Stock, new List<ServerItem>(), null, 0, items =>
        {
            furnitureFetchFinished = false;
            if (items != null)
            {
                ClanInventory inventory = new();
                List<ClanFurniture> clanFurniture = new();
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
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        ReadOnlyCollection<GameFurniture> baseFurniture = null;
                        store.GetAllGameFurnitureYield(result => baseFurniture = result);
                        clanFurniture = CreateDefaultModels.CreateDefaultDebugFurniture(baseFurniture);
                    }
                }
                if (clanFurniture.Count != 0)
                {
                    inventory.Furniture = clanFurniture;
                    clanData.Inventory = inventory;
                }
            }
            furnitureFetchFinished = true;
        }));

        yield return new WaitUntil(() => furnitureFetchFinished);

        bool stallFurnitureFetchFinished = true;
        if (!_skipServerFurniture)
        {
            stallFurnitureFetchFinished = false;
            yield return StartCoroutine(GetOwnFleaMarketItemsFromServer(callback =>
            {
                foreach (var item in callback)
                {
                    clanData.Inventory.Furniture.Add(item);
                }
                stallFurnitureFetchFinished = true;
            }));
        }

        yield return new WaitUntil(() => stallFurnitureFetchFinished);

        yield return StartCoroutine(GetClanStall(clan._id, stall =>
        {
            if (stall == null) Debug.LogWarning("Failed to get stall data.");
            else
            {
                clanData.AdData = stall;
            }
        }));

        yield return StartCoroutine(GetClanVoteListFromServer(polls =>
        {
            if (polls != null)
            {
                clanData.Polls.Clear();
                foreach (ServerPoll poll in polls)
                {

                    if (poll.type == "flea_market_sell_item" || poll.type == "shop_buy_item")
                    {
                        FurniturePollData pollData = new FurniturePollData(poll, clanData);
                        clanData.Polls.Add(pollData);
                    }
                }
                RaiseClanPollsChangedEvent();
            }
        }));

        // Saves clan data including its items.
        store.SaveClanData(clanData, null);
        RaiseClanChangedEvent();
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
                    callback(new());
            }));
        }
    }

    public IEnumerator UpdateCustomCharacters(Action<bool, List<CustomCharacter>> callback, bool setCharacters = false)
    {
        if (Player == null) { callback(false, null); yield break; }
        List<CustomCharacter> characters = null;
        bool gettingCharacter = true;
        yield return StartCoroutine(GetCustomCharactersFromServer(characterList =>
        {
            if (characterList == null)
            {
                Debug.LogError("Failed to fetch Custom Characters.");
                gettingCharacter = false;
                characters = null;
            }
            else
            {
                gettingCharacter = false;
                characters = characterList;
            }
        }));
        new WaitUntil(() => gettingCharacter == false);

        DataStore storefront = Storefront.Get();
        PlayerData playerData = null;

        storefront.GetPlayerData(Player.uniqueIdentifier, p => playerData = p);

        playerData.BuildCharacterLists(characters);
        if (setCharacters) { 
            playerData.SelectedCharacterIds = new CustomCharacterListObject[3] {
            new(serverId : characters.FirstOrDefault(x=> x.Id == CharacterID.Booksmart).ServerID, Id: CharacterID.Booksmart),
            new(serverId : characters.FirstOrDefault(x=> x.Id == CharacterID.Artist).ServerID,Id: CharacterID.Artist),
            new(serverId : characters.FirstOrDefault(x=> x.Id == CharacterID.Soulsisters).ServerID,Id: CharacterID.Soulsisters) };
        }
        storefront.SavePlayerData(playerData, null);
        if (characters == null) callback(false, characters);
        else callback(true, characters);
    }

    public IEnumerator ServiceHeartBeat()
    {
        float timeCurrent = 0f;
        float timeToNext = 120f;

        while (true)
        {
            timeCurrent = 0f;
            bool? heartBeat = null;
            yield return HeartbeatToServer(callback =>
            {
                if(callback == false)
                {
                    timeToNext = 5f;
                }
                else
                {
                    timeToNext = 5f;
                }
                heartBeat = callback;
            });
            yield return new WaitUntil(() => heartBeat != null);
            bool? list = null;
            if (heartBeat == true)
            {
                yield return GetOnlinePlayersFromServer(callback =>
                {
                    if (callback != null)
                    {
                        list = true;
                        _onlinePlayers = callback;
                        OnOnlinePlayersChanged?.Invoke(_onlinePlayers);
                        timeToNext = 120f;
                    }
                    else
                    {
                        list = false;
                        timeToNext = 5f;
                    }
                });
            }
            yield return new WaitUntil(() => list != null);
            while (timeCurrent < timeToNext)
            {
                yield return null;
                timeCurrent += Time.deltaTime;
            }
        }
    }

    #region Server

    #region Player
    public IEnumerator GetOwnPlayerFromServer(Action<ServerPlayer> callback)
    {
        if (Player != null)
            Debug.LogWarning("Player already exists. Consider using ServerManager.Instance.Player if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "player/" + PlayerPrefs.GetString("playerId", string.Empty) + "?with=DailyTask", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
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

    public IEnumerator GetOtherPlayerFromServer(string id, Action<ServerPlayer> callback, bool dailyTask = false)
    {
        string withDailyTask = "";
        if (dailyTask)withDailyTask= "?with=DailyTask";

        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "player/" + id + withDailyTask, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerPlayer player = result["data"]["Player"].ToObject<ServerPlayer>();

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

    public IEnumerator GetPlayerFromServer(string playerId, Action<ServerPlayer> callback)
    {
        if (Player != null)
            Debug.LogWarning("Player already exists. Consider using ServerManager.Instance.Player if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "player/" + playerId, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerPlayer player = result["data"]["Player"].ToObject<ServerPlayer>();

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

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "player/", player, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(player);
                //Debug.LogWarning(result);
                ServerPlayer playerInfo = result.ToObject<ServerPlayer>();

                if (playerInfo.name != null) Player.name = playerInfo.name;
                //if (playerInfo.uniqueIdentifier != null) Player.uniqueIdentifier = playerInfo.uniqueIdentifier;
                if (playerInfo.clan_id != null) Player.clan_id = playerInfo.clan_id;
                if (playerInfo.currentAvatarId != null) Player.currentAvatarId = playerInfo.currentAvatarId;
                if (playerInfo.battleCharacter_ids != null) Player.battleCharacter_ids = playerInfo.battleCharacter_ids;
                if (playerInfo.above13 != null) Player.above13 = playerInfo.above13;
                if (playerInfo.parentalAuth != null) Player.parentalAuth = playerInfo.parentalAuth;
                if (playerInfo.avatar != null) Player.avatar = playerInfo.avatar;
                if (playerInfo.gameStatistics != null) Player.gameStatistics = playerInfo.gameStatistics;
                if (playerInfo.DailyTask != null ) Player.DailyTask = playerInfo.DailyTask;
                if (playerInfo.clanRole_id != null) Player.clanRole_id = playerInfo.clanRole_id;
                if (playerInfo.clanLogo != null) Player.clanLogo = playerInfo.clanLogo;


                //Player = playerInfo;

                if (callback != null)
                    callback(Player);
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


        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "clan/" + Player.clan_id, AccessToken, request =>
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
        string query = SERVERADDRESS + "clan";//?page=" + page + "&limit=5";

        StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                JArray clans = (JArray)result["data"]["Clan"];

                PaginationData paginationData = new();//result["paginationData"].ToObject<PaginationData>();

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


        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "clan/" + Player.clan_id + "?with=Player", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                List<ClanMember> members = new();
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray middleresult = result["data"]["Clan"]["Player"] as JArray;
                foreach (JToken value in middleresult)
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

        StartCoroutine(WebRequests.Post(SERVERADDRESS + "clan/join", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                string clanId = result["data"]["Clan"]["_id"].ToString();
                GameAnalyticsManager.Instance.ClanChange(clanId);
                StartCoroutine(WebRequests.Get(SERVERADDRESS + "clan/" + clanId, AccessToken, request =>
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

        StartCoroutine(WebRequests.Post(SERVERADDRESS + "clan/leave", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Clan = null;
                Player.clan_id = null;
                Stock = null;

                PlayerData playerData = null;
                DataStore storefront = Storefront.Get();

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

        yield return StartCoroutine(WebRequests.Post(SERVERADDRESS + "clan", body, AccessToken, request =>
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
        ClanLogo logo = new ClanLogo();
        logo.logoType = ClanLogoType.Heart;
        logo.pieceColors = new();
        List<string> serverValues = new();

        foreach (var piece in data.ClanHeartPieces)
        {
            logo.pieceColors.Add(ColorUtility.ToHtmlStringRGB(piece.pieceColor));
        }

        foreach (var value in data.Values)
        {
            string valueString = ClanDataTypeConverter.ClanValuesToString(value);
            serverValues.Add(valueString);
        }

        string body = JObject.FromObject(
            new
            {
                _id = data.Id,
                name = data.Name,
                tag = data.Tag,
                isOpen = Clan.isOpen,
                labels = serverValues,
                ageRange = data.ClanAge,
                goal = data.Goals,
                phrase = data.Phrase,
                language = data.Language,
                clanLogo = logo
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return UpdateClanToServer(body, callback);
        Storefront.Get().SaveClanData(data, null);
    }

    public IEnumerator UpdateClanToServer(string body, Action<bool> callback)
    {

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "clan", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerClan clan = result["data"]["Clan"].ToObject<ServerClan>();
                Clan = clan;

                StartCoroutine(SaveClanFromServerToDataStorage(Clan));

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

    #region Roles
    public IEnumerator CreateClanRoleToServer(ClanRoles role, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                name = role.name,
                rights = role.rights,

            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
            ).ToString();

        yield return StartCoroutine(WebRequests.Post(SERVERADDRESS + "clan/role", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
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

    public IEnumerator UpdateClanRoleToServer(ClanRoles role, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                id = role._id,
                name = role.name,
                rights = role.rights,

            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
            ).ToString();

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "clan/role", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
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

    public IEnumerator SetMemberRoleInClanToServer(string player, string role, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                player_id = player,
                role_id = role
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
            ).ToString();

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "clan/role/set", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
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

    #endregion

    #region Heartbeat

    public IEnumerator GetOnlinePlayersFromServer(Action<List<ServerOnlinePlayer>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "online-players", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                //ServerPlayer player = result["data"]["Object"].ToObject<ServerPlayer>();
                List<ServerOnlinePlayer> player = result["data"]["Object"].ToObject<List<ServerOnlinePlayer>>();

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

    public IEnumerator HeartbeatToServer(Action<bool> callback)
    {
        if (Player == null)
        {
            Debug.LogError("Cannot find Player.");
            yield break;
        }

        yield return StartCoroutine(WebRequests.Post(SERVERADDRESS + "online-players/ping", "", AccessToken, request =>
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

    #region DailyTasks

    public IEnumerator GetPlayerTasksFromServer(Action<ClanTasks> callback)
    {
        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "dailyTasks", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                List<ServerPlayerTask> serverTasks = ((JArray)result["data"]["DailyTask"]).ToObject<List<ServerPlayerTask>>();
                //Clan = clan;
                if (serverTasks.Count < 1) { callback(null); return; }

                List<PlayerTask> tasks = new();
                foreach (ServerPlayerTask task in serverTasks)
                {
                    tasks.Add(new(task));
                    if (task._id == Player.DailyTask?._id) Player.DailyTask = task;
                }

                if(Player.DailyTask?._id != null && Player.DailyTask.title == null){
                    StartCoroutine(GetPlayerTaskFromServer(Player.DailyTask._id, task =>
                    {
                        Player.DailyTask = task;
                    }));
                }

                ClanTasks clanTasks;
                if(GameConfig.Get().GameVersionType is VersionType.Standard) clanTasks = new(TaskVersionType.Normal,tasks);
                else clanTasks = new(TaskVersionType.Education, tasks);

                if (callback != null)
                    callback(clanTasks);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator GetPlayerTaskFromServer(string taskId, Action<ServerPlayerTask> callback)
    {
        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "dailyTasks/" + taskId, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerPlayerTask serverTask = result["data"]["DailyTask"].ToObject<ServerPlayerTask>();
                //Clan = clan;

                if (callback != null)
                    callback(serverTask);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator ReservePlayerTaskFromServer(string taskId, Action<PlayerTask> callback)
    {
        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "dailyTasks/reserve/" + taskId, "", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerPlayerTask task = result["data"]["Object"].ToObject<ServerPlayerTask>();
                //Clan = clan;

                if (callback != null)
                    callback(new(task));
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator UnreservePlayerTaskFromServer(Action<bool> callback)
    {
        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "dailyTasks/unreserve/", "", AccessToken, request =>
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

    public IEnumerator ProgressPlayerTaskToServer(int amount, Action<bool> callback)
    {
        string body = JObject.FromObject(new { amount = amount }).ToString();

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "dailyTasks/uiDailyTask/", body, AccessToken, request =>
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

    public IEnumerator GetMessageHistory(ChatChannelType channel, Action<List<ServerChatMessage>> callback)
    {
        yield return StartCoroutine(WebRequests.Get($"{DEVADDRESS}chat/history?type={channel.ToString().ToLower()}", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                List<ServerChatMessage> messageList = ((JArray)result["data"]["ChatMessage"]).ToObject<List<ServerChatMessage>>();



                if (callback != null)
                    callback(messageList);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    #region BattleCharacter

    public IEnumerator GetCustomCharactersFromServer(Action<List<CustomCharacter>> callback)
    {
        if (Player == null)
            Debug.LogWarning("Cannot find ServerPlayer data. Fetch player data before trying to get CustomCharacters.");

        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "customCharacter/", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
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

    /// <summary>
    /// Starts a coroutine for updating custom character to server, since PlayerData where it's called isn't inherited from MonoBehaviour.
    /// </summary>
    /// <param name="character">The CustomCharacter which to save to server.</param>
    public void StartUpdatingCustomCharacterToServer(CustomCharacter character)
    {
        StartCoroutine(UpdateCustomCharactersToServer(character, success =>
        {
            if (!success) Debug.LogError("Failed to save custom character to server!");
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

        //Debug.LogWarning(body);

        yield return StartCoroutine(WebRequests.Put(SERVERADDRESS + "customCharacter/", body, AccessToken, request =>
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

    public IEnumerator AddCustomCharactersToServer(CharacterID id, Action<ServerCharacter> callback)
    {
        if (id.Equals(CharacterID.None))
        {
            Debug.LogError("Cannot find Player.");
            callback(null);
            yield break;
        }
        if (CustomCharacter.IsTestCharacter(id)) { callback(null); yield break; }

        ReadOnlyCollection<BaseCharacter> allItems = null;
        Storefront.Get().GetAllBaseCharacterYield(result => allItems = result);

        BaseCharacter character = allItems.FirstOrDefault(x => x.Id == id);

        if (character == null) { callback(null); yield break; }

        ServerCharacter serverCharacter = new(id);

        string body = JObject.FromObject(serverCharacter).ToString();

        //Debug.LogWarning(body);

        yield return StartCoroutine(WebRequests.Post(SERVERADDRESS + "customCharacter/", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                ServerCharacter serverCharacter = result["data"]["CustomCharacter"].ToObject<ServerCharacter>();


                if (callback != null)
                    callback(serverCharacter);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    #endregion

    #region Stock
    public IEnumerator GetStockFromServer(ServerClan clan, Action<ServerStock> callback)
    {
        if (Stock != null)
            Debug.LogWarning("Stock already exists. Consider using ServerManager.Instance.Stock if the most up to data data from server is not needed.");

        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "stock?search=clan_id=\"" + clan._id + "\"", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
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
            query = SERVERADDRESS + "stock/" + stock._id + "?limit=10&with=Item";
        else
            query = SERVERADDRESS + "stock/" + stock._id + "?page=" + ++paginationData.currentPage + "&limit=10&with=Item";

        yield return StartCoroutine(WebRequests.Get(query, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                List<ServerItem> requestItems = new();
                JObject jObject = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(jObject);
                JArray array = (JArray)jObject["data"]["Stock"]["Item"];
                requestItems = array.ToObject<List<ServerItem>>();

                foreach (ServerItem item in requestItems)
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

            StartCoroutine(WebRequests.Post(SERVERADDRESS + "stock", body, AccessToken, request =>
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
        StartCoroutine(WebRequests.Post(SERVERADDRESS + "gameAnalytics/logfile/", formData, AccessToken, secretKey, id, callback));
    }

    #endregion

    #region Voting

    public IEnumerator GetClanVoteListFromServer(Action<List<ServerPoll>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "voting/", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                //ServerPlayer player = result["data"]["Object"].ToObject<ServerPlayer>();
                JArray middleresult = (JArray)result["data"]["Voting"];

                List<ServerPoll> pollList = new();
                foreach (var item in middleresult)
                {
                    pollList.Add(item.ToObject<ServerPoll>());
                }

                if (callback != null)
                    callback(pollList);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public void SendClanVoteToServer(string voteid, bool answer, Action<ServerPoll> callback) => StartCoroutine(SendClanVoteToServerCoroutine(voteid, answer, callback));

    public IEnumerator SendClanVoteToServerCoroutine(string voteid, bool answer, Action<ServerPoll> callback)
    {
        string body = JObject.FromObject(new { voting_id = voteid, choice = answer?"accept":"reject" }).ToString();

        yield return StartCoroutine(WebRequests.Put(DEVADDRESS + "voting/", body, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);

                ServerPoll poll = new();
                poll = result["data"]["Voting"].ToObject<ServerPoll>();

                if (callback != null)
                    callback(poll);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    #endregion

    #region Shop
    public IEnumerator GetClanShopListFromServer(Action<List<GameFurniture>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "clan-shop/items/", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                //Debug.LogWarning(result);
                //ServerPlayer player = result["data"]["Object"].ToObject<ServerPlayer>();
                JArray middleresult = (JArray)result["data"]["Item"];

                List<GameFurniture> furniturelist = new();
                foreach(var item in middleresult)
                {
                    string name = item["name"].ToString();
                    GameFurniture furniture = StorageFurnitureReference.Instance.GetGameFurniture(name);
                    if (furniture != null)
                    furniturelist.Add(furniture);
                }

                if (callback != null)
                    callback(furniturelist);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }
    public void BuyShopItem(string itemName, Action<bool> callback) => StartCoroutine(BuyShopItemToServer(itemName, callback));
    public IEnumerator BuyShopItemToServer(string itemName, Action<bool> callback)
    {
        string body = JObject.FromObject(new { itemName = itemName }).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "clan-shop/buy", body, AccessToken, request =>
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

    public IEnumerator GetOwnFleaMarketItemsFromServer(Action<List<ClanFurniture>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "fleamarket", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                //ServerPlayer player = result["data"]["Object"].ToObject<ServerPlayer>();
                JArray middleresult = (JArray)result["data"]["FleaMarketItem"];

                List<ClanFurniture> furniturelist = new();
                foreach (var item in middleresult)
                {
                    if (item["clan_id"].ToString() == Clan._id)
                    {
                        Debug.LogWarning("FleaMarketFetch");
                        string id = item["_id"].ToString();
                        string name = item["name"].ToString();
                        ClanFurniture furniture = new(id, name);
                        if (furniture != null)
                            furniturelist.Add(furniture);
                    }
                }

                if (callback != null)
                    callback(furniturelist);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator GetFleaMarketItemsFromServer(Action<List<GameFurniture>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "fleamarket", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                //ServerPlayer player = result["data"]["Object"].ToObject<ServerPlayer>();
                JArray middleresult = (JArray)result["data"]["FleaMarketItem"];

                List<GameFurniture> furniturelist = new();
                foreach (var item in middleresult)
                {
                    string name = item["name"].ToString();
                    GameFurniture furniture = StorageFurnitureReference.Instance.GetGameFurniture(name);
                    if (furniture != null)
                        furniturelist.Add(furniture);
                }

                if (callback != null)
                    callback(furniturelist);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public IEnumerator AddCoinsToClan(int amount, Action<bool> callback)
    {
        string body = JObject.FromObject(new { amount = amount }).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "shop/clanCoins", body, AccessToken, request =>
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

    #region Stall
    public IEnumerator GetClanStall(string clan_id, Action<AdStoreObject> callback)
    {
        yield return StartCoroutine(WebRequests.Get(DEVADDRESS + "stall/"+clan_id, AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                ServerStall stall = result["data"]["Stall"].ToObject<ServerStall>();

                AdStoreObject adObject = new AdStoreObject(stall.adPoster.border, stall.adPoster.colour);



                if (callback != null)
                    callback(adObject);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));
    }

    public void SellItemOnStall(string item_id, int price, Action<bool> callback) => StartCoroutine(SellItemOnStallToServer(item_id, price, callback));

    public IEnumerator SellItemOnStallToServer(string item_id, int price, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                item_id = item_id,
                price = price
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "fleaMarket/sell/", body, AccessToken, request =>
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

    public IEnumerator BuyItemFromStall(string item_id, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                item_id = item_id
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "fleaMarket/buy/", body, AccessToken, request =>
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

    public IEnumerator PutItemOnStall(string item_id, bool toStall, Action<bool> callback)
    {
        string status = toStall?"available":"shipping";
        string body = JObject.FromObject(
            new
            {
                item_id = item_id,
                status = status
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "fleaMarket/change-item-status/", body, AccessToken, request =>
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

    public IEnumerator ChangeItemPrice(string item_id, int price, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                item_id = item_id,
                price = price
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "fleaMarket/change-item-price/", body, AccessToken, request =>
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

    public IEnumerator ReturnItemToStock(string item_id, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                item_id = item_id
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Post(DEVADDRESS + "fleaMarket/move-to-stock/"+item_id, body, AccessToken, request =>
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

    public IEnumerator UpdateClanAdPoster(AdStoreObject ad, Action<bool> callback)
    {
        string body = JObject.FromObject(
            new
            {
                border = ad.BorderFrame,
                colour = ad.BackgroundColour,
                //tag = data.Tag,
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        yield return StartCoroutine(WebRequests.Patch(DEVADDRESS + "stall/adPoster", body, AccessToken, request =>
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

    #region Leaderboard
    public IEnumerator GetClanLeaderboardFromServer(Action<List<ClanLeaderboard>> callback)
    {
        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "leaderboard/clan", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                JArray jArray = (JArray)result["data"]["Clan"];
                //Debug.LogWarning(result);
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
        yield return StartCoroutine(WebRequests.Get(SERVERADDRESS + "leaderboard/player", AccessToken, request =>
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

    #region Jukebox
    public IEnumerator GetJukeboxClanPlaylist(Action<ServerPlaylist> callback)
    {
        StartCoroutine(WebRequests.Get(SERVERADDRESS + "jukebox", AccessToken, request =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject result = JObject.Parse(request.downloadHandler.text);
                Debug.LogWarning(result);
                ServerPlaylist playlist = result["data"]["Jukebox"].ToObject<ServerPlaylist>();

                if (callback != null)
                    callback(playlist);
            }
            else
            {
                if (callback != null)
                    callback(null);
            }
        }));

        yield break;
    }

    public IEnumerator AddJukeboxClanMusicTrack(Action<bool> callback, MusicTrack musicTrack)
    {
        string body = JObject.FromObject(
            new
            {
                songId = musicTrack.Id,
                songDurationSeconds = (int)musicTrack.Music.length
            },
            JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })
        ).ToString();

        StartCoroutine(WebRequests.Post(SERVERADDRESS + "jukebox", body, AccessToken, request =>
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

        yield break;
    }

    public IEnumerator DeleteJukeboxClanMusicTrack(Action<bool> callback, string musicTrackUniqueId)
    {
        StartCoroutine(WebRequests.Delete(SERVERADDRESS + "jukebox/" + musicTrackUniqueId, AccessToken, request =>
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

        yield break;
    }
    #endregion

    #endregion
}
