using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Photon.Client;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class RaidMatchmakingController : MonoBehaviour, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback
{
    [SerializeField] private float lobbyCountdownSeconds = 10f;
    [SerializeField] private float retryDelaySeconds = 1.25f;
    [SerializeField] private int minInventoryRows = 6;
    [SerializeField] private int maxInventoryRowsExclusive = 12;
    [SerializeField] private float fallbackClanWeightLimit = 200f;
    [SerializeField] private RaidMatchmakingViews viewsPrefab;

    private readonly Dictionary<string, RoomInfo> _knownRooms = new();
    private readonly HashSet<string> _rejectedRoomNames = new(StringComparer.Ordinal);
    private readonly HashSet<int> _lootedSlots = new();

    private Raid_InventoryHandler _inventoryHandler;
    private Raid_InventoryPage _inventoryPage;
    private Raid_LootTracking _lootTracking;
    private Raid_Timer _raidTimer;
    private ExitRaid _exitRaid;

    private RaidMatchmakingViews _views;
    private Canvas _overlayCanvas;
    private GameObject _matchmakingPanel;
    private GameObject _lobbyPanel;
    private TextMeshProUGUI _matchmakingTitleText;
    private TextMeshProUGUI _matchmakingStatusText;
    private TextMeshProUGUI _matchmakingDetailText;
    private TextMeshProUGUI _lobbyCountdownText;
    private Transform _participantListRoot;
    private RaidLobbyClanListItem _clanListItemTemplate;

    private string _localPlayerName = "Player";
    private string _localClanId = string.Empty;
    private string _localClanName = string.Empty;

    private bool _joiningOrCreatingRoom;
    private bool _waitingForRetryLeave;
    private bool _surrendering;
    private bool _inventoryInitialized;
    private bool _gameplayReleased;
    private bool _callbacksRegistered;

    public static RaidMatchmakingController Instance { get; private set; }

    public bool ControlsInventorySetup => true;
    public bool HasReleasedGameplay => _gameplayReleased;
    public bool IsSharedRaidActive => _gameplayReleased && IsCurrentRoomRaid();
    public string LocalClanId => _localClanId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResolveReferences();
        CreateOverlayUi();
        RegisterPhotonCallbacks();
    }

    private void Start()
    {
        ShowMatchmaking("Preparing Raid matchmaking", "Loading clan data...", string.Empty);
        StartCoroutine(StartRaidMatchmakingFlow());
    }

    private void Update()
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        int state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);

        if (state == RaidPhotonRoom.StateMatchmaking)
        {
            UpdateMatchmakingStatus();
            return;
        }

        if (state == RaidPhotonRoom.StateLobby)
        {
            ConfigureRaidFromRoomIfReady();
            ShowLobby();
            UpdateLobbyCountdown();
            return;
        }

        if (state == RaidPhotonRoom.StateStarted)
        {
            ConfigureRaidFromRoomIfReady();
            BeginGameplay();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnregisterPhotonCallbacks();
    }

    private IEnumerator StartRaidMatchmakingFlow()
    {
        yield return LoadLocalClanData();

        if (string.IsNullOrWhiteSpace(_localClanId))
        {
            ShowMatchmaking("Raid requires a clan", "Join a clan before entering Raid matchmaking.", string.Empty);
            yield break;
        }

        if (PhotonRealtimeClient.InRoom)
        {
            if (IsCurrentRoomRaid())
            {
                HandleJoinedRaidRoom();
                yield break;
            }

            ShowMatchmaking("Preparing Raid matchmaking", "Leaving the previous room...", string.Empty);
            PhotonRealtimeClient.LeaveRoom(false);
            yield return new WaitUntil(() => !PhotonRealtimeClient.InRoom);
        }

        yield return WaitForPhotonReady();

        if (!CanUsePhotonMatchmaking())
        {
            ShowMatchmaking("Raid matchmaking unavailable", "Photon is not ready for room matchmaking.", PhotonRealtimeClient.NetworkClientState.ToString());
            yield break;
        }

        JoinOrCreateRaidRoom();
    }

    private IEnumerator LoadLocalClanData()
    {
        PlayerData playerData = null;
        ClanData clanData = null;

        if (ServerManager.Instance != null)
        {
            _localPlayerName = ServerManager.Instance.Player?.name ?? _localPlayerName;
            _localClanId = ServerManager.Instance.Player?.clan_id ?? string.Empty;
            _localClanName = ServerManager.Instance.Clan?.name ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_localClanId))
        {
            bool playerLoaded = false;
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
            {
                playerData = data;
                playerLoaded = true;
            });

            yield return new WaitUntil(() => playerLoaded);

            if (playerData != null)
            {
                _localPlayerName = string.IsNullOrWhiteSpace(playerData.Name) ? _localPlayerName : playerData.Name;
                _localClanId = playerData.ClanId ?? string.Empty;
            }
        }

        if (!string.IsNullOrWhiteSpace(_localClanId) && string.IsNullOrWhiteSpace(_localClanName))
        {
            bool clanLoaded = false;
            Storefront.Get().GetClanData(_localClanId, data =>
            {
                clanData = data;
                clanLoaded = true;
            });

            yield return new WaitUntil(() => clanLoaded);
            _localClanName = clanData?.Name ?? _localClanId;
        }

        if (string.IsNullOrWhiteSpace(_localPlayerName))
        {
            _localPlayerName = PhotonRealtimeClient.NickName;
        }

        if (!string.IsNullOrWhiteSpace(_localPlayerName) && PhotonRealtimeClient.Client != null)
        {
            PhotonRealtimeClient.NickName = _localPlayerName;
        }
    }

    private IEnumerator WaitForPhotonReady()
    {
        if (PhotonRealtimeClient.Client != null && !PhotonRealtimeClient.IsConnected && PhotonRealtimeClient.CanConnect)
        {
            ShowMatchmaking("Connecting to Raid matchmaking", "Connecting to Photon...", string.Empty);
            PhotonRealtimeClient.Connect(_localPlayerName);
        }

        float timeout = Time.time + 20f;
        while (!CanUsePhotonMatchmaking() && Time.time < timeout)
        {
            ShowMatchmaking("Connecting to Raid matchmaking", "Waiting for Photon master server...", PhotonRealtimeClient.NetworkClientState.ToString());
            yield return new WaitForSeconds(0.25f);
        }
    }

    private bool CanUsePhotonMatchmaking()
    {
        return PhotonRealtimeClient.Client != null
            && PhotonRealtimeClient.Client.IsConnectedAndReady
            && PhotonRealtimeClient.Client.Server == ServerConnection.MasterServer;
    }

    private void JoinOrCreateRaidRoom()
    {
        if (_joiningOrCreatingRoom || !CanUsePhotonMatchmaking())
        {
            return;
        }

        _joiningOrCreatingRoom = true;
        ShowMatchmaking("Finding Raid players", "Searching for a Raid room...", "Waiting for 4 clan players");

        RoomInfo room = FindJoinableRaidRoom();
        if (room != null)
        {
            EnterRoomArgs joinArgs = new()
            {
                RoomName = room.Name,
                Lobby = PhotonRealtimeClient.Client.InLobby ? PhotonRealtimeClient.Client.CurrentLobby : null
            };

            if (PhotonRealtimeClient.Client.OpJoinRoom(joinArgs))
            {
                return;
            }
        }

        bool operationStarted = _knownRooms.Count > 0 || _rejectedRoomNames.Count > 0
            ? CreateRaidRoom()
            : JoinRandomOrCreateRaidRoom();

        if (!operationStarted)
        {
            _joiningOrCreatingRoom = false;
            StartCoroutine(RetryMatchmakingAfterDelay());
        }
    }

    private bool JoinRandomOrCreateRaidRoom()
    {
        RoomOptions roomOptions = CreateRaidRoomOptions();
        EnterRoomArgs createArgs = new()
        {
            RoomName = $"Raid_{Guid.NewGuid():N}",
            RoomOptions = roomOptions,
            Lobby = PhotonRealtimeClient.Client.InLobby ? PhotonRealtimeClient.Client.CurrentLobby : null
        };

        JoinRandomRoomArgs joinRandomArgs = new()
        {
            ExpectedCustomRoomProperties = new PhotonHashtable
            {
                { PhotonBattleRoom.GameTypeKey, (int)GameType.Raid },
                { RaidPhotonRoom.RaidMatchmakingKey, true },
                { RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking },
                { RaidPhotonRoom.RaidSetupReadyKey, false }
            },
            ExpectedMaxPlayers = RaidPhotonRoom.RoomCapacity,
            Lobby = createArgs.Lobby
        };

        return PhotonRealtimeClient.Client.OpJoinRandomOrCreateRoom(joinRandomArgs, createArgs);
    }

    private bool CreateRaidRoom()
    {
        EnterRoomArgs createArgs = new()
        {
            RoomName = $"Raid_{Guid.NewGuid():N}",
            RoomOptions = CreateRaidRoomOptions(),
            Lobby = PhotonRealtimeClient.Client.InLobby ? PhotonRealtimeClient.Client.CurrentLobby : null
        };

        return PhotonRealtimeClient.Client.OpCreateRoom(createArgs);
    }

    private RoomOptions CreateRaidRoomOptions()
    {
        PhotonHashtable customRoomProperties = new()
        {
            { PhotonBattleRoom.GameTypeKey, (int)GameType.Raid },
            { RaidPhotonRoom.RaidMatchmakingKey, true },
            { RaidPhotonRoom.RaidClanCountsKey, string.Empty },
            { RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking },
            { RaidPhotonRoom.RaidSetupReadyKey, false }
        };

        return new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = RaidPhotonRoom.RoomCapacity,
            PlayerTtl = 0,
            EmptyRoomTtl = 10000,
            PublishUserId = true,
            CustomRoomProperties = customRoomProperties,
            CustomRoomPropertiesForLobby = new[]
            {
                PhotonBattleRoom.GameTypeKey,
                RaidPhotonRoom.RaidMatchmakingKey,
                RaidPhotonRoom.RaidClanCountsKey,
                RaidPhotonRoom.RaidStateKey,
                RaidPhotonRoom.RaidSetupReadyKey
            }
        };
    }

    private RoomInfo FindJoinableRaidRoom()
    {
        return _knownRooms.Values
            .Where(room => IsJoinableRaidRoomForLocalClan(room))
            .OrderByDescending(room => room.PlayerCount)
            .FirstOrDefault();
    }

    private bool IsJoinableRaidRoomForLocalClan(RoomInfo room)
    {
        if (room == null || room.RemovedFromList || !room.IsOpen || room.PlayerCount >= RaidPhotonRoom.RoomCapacity)
        {
            return false;
        }

        if (_rejectedRoomNames.Contains(room.Name))
        {
            return false;
        }

        if (GetRoomInfoProperty(room, PhotonBattleRoom.GameTypeKey, (int)GameType.None) != (int)GameType.Raid)
        {
            return false;
        }

        if (!GetRoomInfoProperty(room, RaidPhotonRoom.RaidMatchmakingKey, false))
        {
            return false;
        }

        if (GetRoomInfoProperty(room, RaidPhotonRoom.RaidSetupReadyKey, false))
        {
            return false;
        }

        int state = GetRoomInfoProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);
        if (state != RaidPhotonRoom.StateMatchmaking)
        {
            return false;
        }

        RaidPhotonRoom.ClanEntry[] clans = RaidPhotonRoom.DecodeClanCounts(GetRoomInfoProperty(room, RaidPhotonRoom.RaidClanCountsKey, string.Empty));
        RaidPhotonRoom.ClanEntry existing = clans.FirstOrDefault(clan => clan.ClanId == _localClanId);
        return string.IsNullOrWhiteSpace(existing.ClanId) || existing.Count < RaidPhotonRoom.MaxPlayersPerClan;
    }

    private void HandleJoinedRaidRoom()
    {
        _joiningOrCreatingRoom = false;
        _waitingForRetryLeave = false;

        if (!IsCurrentRoomRaid())
        {
            return;
        }

        SetLocalPlayerRaidProperties();
        ConfigureRaidFromRoomIfReady();
        RefreshParticipantList();
        UpdateMatchmakingStatus();

        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }

        StartCoroutine(ValidateLocalRoomMembership());
    }

    private void SetLocalPlayerRaidProperties()
    {
        if (PhotonRealtimeClient.LocalPlayer == null || string.IsNullOrWhiteSpace(_localClanId))
        {
            return;
        }

        PhotonRealtimeClient.LocalPlayer.SetCustomProperties(new PhotonHashtable
        {
            { RaidPhotonRoom.PlayerClanIdKey, _localClanId },
            { RaidPhotonRoom.PlayerClanNameKey, _localClanName }
        });
    }

    private IEnumerator ValidateLocalRoomMembership()
    {
        yield return new WaitForSeconds(0.5f);

        if (!IsCurrentRoomRaid())
        {
            yield break;
        }

        int state = GetRoomProperty(PhotonRealtimeClient.CurrentRoom, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);
        if (state != RaidPhotonRoom.StateMatchmaking)
        {
            yield break;
        }

        int localClanPlayers = CountPlayersInClan(_localClanId);
        if (localClanPlayers <= RaidPhotonRoom.MaxPlayersPerClan)
        {
            yield break;
        }

        _rejectedRoomNames.Add(PhotonRealtimeClient.CurrentRoom.Name);
        _waitingForRetryLeave = true;
        ShowMatchmaking("Finding Raid players", "Your clan already has two players in that Raid room.", "Trying another room...");
        PhotonRealtimeClient.LeaveRoom(false);
    }

    private void RefreshMasterRoomState()
    {
        if (!IsCurrentRoomRaid() || !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        int state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);
        if (state != RaidPhotonRoom.StateMatchmaking)
        {
            return;
        }

        List<Player> validPlayers = GetRaidPlayersWithClans();
        RaidPhotonRoom.ClanEntry[] clanEntries = BuildClanEntries(validPlayers);
        room.SetCustomProperties(new PhotonHashtable
        {
            { RaidPhotonRoom.RaidClanCountsKey, RaidPhotonRoom.EncodeClanCounts(clanEntries) }
        });

        bool validClanFormation = validPlayers.Count >= RaidPhotonRoom.RequiredPlayers
            && validPlayers.Count <= RaidPhotonRoom.RoomCapacity
            && clanEntries.All(clan => clan.Count <= RaidPhotonRoom.MaxPlayersPerClan);

        room.IsOpen = !validClanFormation;
        room.IsVisible = !validClanFormation;

        if (validClanFormation)
        {
            StartLobbyCountdown(validPlayers, clanEntries);
        }
    }

    private void StartLobbyCountdown(List<Player> validPlayers, RaidPhotonRoom.ClanEntry[] clanEntries)
    {
        Room room = PhotonRealtimeClient.CurrentRoom;
        if (GetRoomProperty(room, RaidPhotonRoom.RaidSetupReadyKey, false))
        {
            return;
        }

        int seed = Guid.NewGuid().GetHashCode();
        System.Random rng = new(seed);
        int inventoryRows = rng.Next(minInventoryRows, Mathf.Max(minInventoryRows + 1, maxInventoryRowsExclusive));
        int inventorySize = inventoryRows * 4;
        RaidPhotonRoom.TrapData[] traps = _inventoryPage != null
            ? _inventoryPage.BuildDeterministicTrapData(inventorySize, seed + 17)
            : Array.Empty<RaidPhotonRoom.TrapData>();

        float clanMaxWeight = GetDefaultClanMaxWeight();
        RaidPhotonRoom.ClanWeightLimit[] clanLimits = clanEntries
            .Select(clan => new RaidPhotonRoom.ClanWeightLimit(clan.ClanId, clanMaxWeight))
            .ToArray();

        long startTimeMs = DateTimeOffset.UtcNow.AddSeconds(lobbyCountdownSeconds).ToUnixTimeMilliseconds();
        room.SetCustomProperties(new PhotonHashtable
        {
            { RaidPhotonRoom.RaidClanCountsKey, RaidPhotonRoom.EncodeClanCounts(clanEntries) },
            { RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateLobby },
            { RaidPhotonRoom.RaidSetupReadyKey, true },
            { RaidPhotonRoom.RaidStartTimeKey, startTimeMs.ToString() },
            { RaidPhotonRoom.RaidInventorySizeKey, inventorySize },
            { RaidPhotonRoom.RaidInventorySeedKey, seed },
            { RaidPhotonRoom.RaidTrapSlotsKey, RaidPhotonRoom.EncodeTraps(traps) },
            { RaidPhotonRoom.RaidClanWeightLimitsKey, RaidPhotonRoom.EncodeClanWeightLimits(clanLimits) }
        });

        room.IsOpen = false;
        room.IsVisible = false;
        RefreshParticipantList(validPlayers);
    }

    private float GetDefaultClanMaxWeight()
    {
        if (_lootTracking != null && _lootTracking.MaxLootWeight > 0f)
        {
            return _lootTracking.MaxLootWeight;
        }

        return fallbackClanWeightLimit;
    }

    private void ConfigureRaidFromRoomIfReady()
    {
        if (_inventoryInitialized || !IsCurrentRoomRaid())
        {
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        if (!GetRoomProperty(room, RaidPhotonRoom.RaidSetupReadyKey, false))
        {
            return;
        }

        int inventorySize = GetRoomProperty(room, RaidPhotonRoom.RaidInventorySizeKey, 0);
        int seed = GetRoomProperty(room, RaidPhotonRoom.RaidInventorySeedKey, 0);
        string trapPayload = GetRoomProperty(room, RaidPhotonRoom.RaidTrapSlotsKey, string.Empty);
        string limitPayload = GetRoomProperty(room, RaidPhotonRoom.RaidClanWeightLimitsKey, string.Empty);

        if (inventorySize <= 0)
        {
            return;
        }

        RaidPhotonRoom.ClanWeightLimit[] limits = RaidPhotonRoom.DecodeClanWeightLimits(limitPayload);
        if (_lootTracking != null)
        {
            _lootTracking.ResetClanLootCounts();
            _lootTracking.SetDisplayedClan(_localClanId);
            foreach (RaidPhotonRoom.ClanWeightLimit limit in limits)
            {
                _lootTracking.SetClanLimit(limit.ClanId, limit.MaxWeight);
            }
        }

        RaidPhotonRoom.TrapData[] traps = RaidPhotonRoom.DecodeTraps(trapPayload);
        if (_inventoryHandler != null)
        {
            _inventoryHandler.InitializeSharedInventory(inventorySize, seed, traps);
        }

        _inventoryInitialized = true;
        RefreshParticipantList();
    }

    private void UpdateLobbyCountdown()
    {
        Room room = PhotonRealtimeClient.CurrentRoom;
        string startTimeValue = GetRoomProperty(room, RaidPhotonRoom.RaidStartTimeKey, string.Empty);
        if (!long.TryParse(startTimeValue, out long startTimeMs))
        {
            return;
        }

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float secondsRemaining = Mathf.Max(0f, (startTimeMs - nowMs) / 1000f);
        if (_lobbyCountdownText != null)
        {
            TimeSpan remaining = TimeSpan.FromSeconds(Mathf.CeilToInt(secondsRemaining));
            _lobbyCountdownText.text = $"Kokoaminen alkaa\n{(int)remaining.TotalMinutes}:{remaining.Seconds:00}";
        }

        if (secondsRemaining <= 0f && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            room.SetCustomProperties(new PhotonHashtable
            {
                { RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateStarted }
            });
        }
    }

    private void BeginGameplay()
    {
        if (_gameplayReleased)
        {
            return;
        }

        _gameplayReleased = true;
        if (_overlayCanvas != null)
        {
            _overlayCanvas.gameObject.SetActive(false);
        }

        if (_lootTracking != null)
        {
            _lootTracking.SetDisplayedClan(_localClanId);
        }

        if (_raidTimer != null)
        {
            _raidTimer.StartTimer();
        }
    }

    public void RequestLoot(int slotIndex)
    {
        if (!IsSharedRaidActive || _exitRaid != null && _exitRaid.raidEnded)
        {
            return;
        }

        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            HandleLootRequest(PhotonRealtimeClient.LocalPlayer.ActorNumber, slotIndex);
            return;
        }

        PhotonRealtimeClient.Client.OpRaiseEvent(
            RaidPhotonRoom.LootRequestEvent,
            new object[] { slotIndex },
            new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable);
    }

    private void HandleLootRequest(int senderActorNumber, int slotIndex)
    {
        if (!IsSharedRaidActive || _inventoryPage == null || _lootedSlots.Contains(slotIndex))
        {
            return;
        }

        Player sender = GetRoomPlayer(senderActorNumber);
        string clanId = GetPlayerClanId(sender);
        if (sender == null || string.IsNullOrWhiteSpace(clanId) || !IsParticipatingClan(clanId))
        {
            return;
        }

        Raid_InventoryItem item = _inventoryPage.GetInventoryItem(slotIndex);
        if (item == null || item.ItemWeight <= 0f || item.furnitureData == null)
        {
            return;
        }

        _lootedSlots.Add(slotIndex);
        float weightMultiplier = _inventoryPage.GetNetworkLootWeightMultiplier(slotIndex);
        PhotonRealtimeClient.Client.OpRaiseEvent(
            RaidPhotonRoom.LootAcceptedEvent,
            new object[] { slotIndex, senderActorNumber, clanId, weightMultiplier },
            new RaiseEventArgs { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    private void ApplyLootAccepted(object[] data)
    {
        if (data == null || data.Length < 4 || _inventoryPage == null)
        {
            return;
        }

        int slotIndex = Convert.ToInt32(data[0]);
        int actorNumber = Convert.ToInt32(data[1]);
        string clanId = data[2] as string ?? string.Empty;
        float weightMultiplier = Convert.ToSingle(data[3]);
        bool triggeredByLocalPlayer = PhotonRealtimeClient.LocalPlayer != null
            && PhotonRealtimeClient.LocalPlayer.ActorNumber == actorNumber;

        _lootedSlots.Add(slotIndex);
        _inventoryPage.HandleNetworkLootAccepted(slotIndex, actorNumber, clanId, weightMultiplier, triggeredByLocalPlayer);
    }

    private bool IsParticipatingClan(string clanId)
    {
        if (string.IsNullOrWhiteSpace(clanId) || !IsCurrentRoomRaid())
        {
            return false;
        }

        string limits = GetRoomProperty(PhotonRealtimeClient.CurrentRoom, RaidPhotonRoom.RaidClanWeightLimitsKey, string.Empty);
        return RaidPhotonRoom.DecodeClanWeightLimits(limits).Any(limit => limit.ClanId == clanId);
    }

    private Player GetRoomPlayer(int actorNumber)
    {
        if (!IsCurrentRoomRaid())
        {
            return null;
        }

        PhotonRealtimeClient.CurrentRoom.Players.TryGetValue(actorNumber, out Player player);
        return player;
    }

    private List<Player> GetRaidPlayersWithClans()
    {
        if (!IsCurrentRoomRaid())
        {
            return new List<Player>();
        }

        return PhotonRealtimeClient.CurrentRoom.Players.Values
            .Where(player => !string.IsNullOrWhiteSpace(GetPlayerClanId(player)))
            .OrderBy(player => player.ActorNumber)
            .ToList();
    }

    private RaidPhotonRoom.ClanEntry[] BuildClanEntries(List<Player> players)
    {
        return players
            .GroupBy(GetPlayerClanId)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .Select(group => new RaidPhotonRoom.ClanEntry(
                group.Key,
                group.Select(GetPlayerClanName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? group.Key,
                group.Count()))
            .ToArray();
    }

    private int CountPlayersInClan(string clanId)
    {
        if (string.IsNullOrWhiteSpace(clanId) || !IsCurrentRoomRaid())
        {
            return 0;
        }

        return PhotonRealtimeClient.CurrentRoom.Players.Values.Count(player => GetPlayerClanId(player) == clanId);
    }

    private string GetPlayerClanId(Player player)
    {
        return GetPlayerProperty(player, RaidPhotonRoom.PlayerClanIdKey, string.Empty);
    }

    private string GetPlayerClanName(Player player)
    {
        return GetPlayerProperty(player, RaidPhotonRoom.PlayerClanNameKey, string.Empty);
    }

    private void UpdateMatchmakingStatus()
    {
        if (!IsCurrentRoomRaid() || _matchmakingStatusText == null)
        {
            return;
        }

        int currentPlayers = GetRaidPlayersWithClans().Count;
        _matchmakingStatusText.text = $"Waiting for players: {currentPlayers}/{RaidPhotonRoom.RequiredPlayers}";
        _matchmakingDetailText.text = "Testing mode: Raid starts with two clan players. Each clan can bring up to two players.";
    }

    private void ShowMatchmaking(string title, string status, string detail)
    {
        if (_overlayCanvas != null)
        {
            _overlayCanvas.gameObject.SetActive(true);
        }

        if (_matchmakingPanel != null)
        {
            _matchmakingPanel.SetActive(true);
        }

        if (_lobbyPanel != null)
        {
            _lobbyPanel.SetActive(false);
        }

        if (_matchmakingTitleText != null)
        {
            _matchmakingTitleText.text = title;
        }

        if (_matchmakingStatusText != null)
        {
            _matchmakingStatusText.text = status;
        }

        if (_matchmakingDetailText != null)
        {
            _matchmakingDetailText.text = detail;
        }
    }

    private void ShowLobby()
    {
        if (_overlayCanvas != null)
        {
            _overlayCanvas.gameObject.SetActive(true);
        }

        if (_matchmakingPanel != null)
        {
            _matchmakingPanel.SetActive(false);
        }

        if (_lobbyPanel != null)
        {
            _lobbyPanel.SetActive(true);
        }
    }

    private void RefreshParticipantList()
    {
        RefreshParticipantList(IsCurrentRoomRaid()
            ? PhotonRealtimeClient.CurrentRoom.Players.Values.OrderBy(player => player.ActorNumber).ToList()
            : new List<Player>());
    }

    private void RefreshParticipantList(List<Player> players)
    {
        if (_participantListRoot == null)
        {
            return;
        }

        for (int i = _participantListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_participantListRoot.GetChild(i).gameObject);
        }

        if (_clanListItemTemplate == null)
        {
            Debug.LogError("Raid lobby clan list item template is not assigned in RaidMatchmakingViews.");
            return;
        }

        var clanRows = players
            .Where(player => !string.IsNullOrWhiteSpace(GetPlayerClanId(player)))
            .GroupBy(GetPlayerClanId)
            .Select(group => new
            {
                ClanName = group.Select(GetPlayerClanName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? group.Key,
                PlayerCount = group.Count(),
                FirstActorNumber = group.Min(player => player.ActorNumber)
            })
            .OrderBy(row => row.FirstActorNumber)
            .ToList();

        int rowCount = clanRows.Count;
        for (int i = 0; i < rowCount; i++)
        {
            RaidLobbyClanListItem row = Instantiate(_clanListItemTemplate, _participantListRoot);
            row.name = $"Clan {i + 1}";
            row.gameObject.SetActive(true);

            float slotHeight = 1f / Mathf.Max(1, rowCount);
            float padding = Mathf.Min(0.04f, slotHeight * 0.16f);
            float maxY = 1f - i * slotHeight - padding;
            float minY = 1f - (i + 1) * slotHeight + padding;

            row.SetAnchors(minY, maxY);
            row.Configure(clanRows[i].ClanName, clanRows[i].PlayerCount, RaidPhotonRoom.MaxPlayersPerClan);
        }
    }

    private void CreateOverlayUi()
    {
        RaidMatchmakingViews prefab = viewsPrefab != null
            ? viewsPrefab
            : Resources.Load<RaidMatchmakingViews>("Prefabs/RaidMatchmakingViews");

        if (prefab == null)
        {
            Debug.LogError("Raid matchmaking views prefab was not found at Resources/Prefabs/RaidMatchmakingViews.");
            return;
        }

        _views = Instantiate(prefab);
        _views.name = "Raid Matchmaking Views";
        _views.Initialize(OnSurrenderPressed);
        AssignViewReferences(_views);
    }

    private void AssignViewReferences(RaidMatchmakingViews views)
    {
        _overlayCanvas = views.Canvas;
        _matchmakingPanel = views.MatchmakingPanel;
        _lobbyPanel = views.LobbyPanel;
        _matchmakingTitleText = views.MatchmakingTitleText;
        _matchmakingStatusText = views.MatchmakingStatusText;
        _matchmakingDetailText = views.MatchmakingDetailText;
        _lobbyCountdownText = views.LobbyCountdownText;
        _participantListRoot = views.ParticipantListRoot;
        _clanListItemTemplate = views.ClanListItemTemplate;
        if (_clanListItemTemplate != null)
        {
            _clanListItemTemplate.gameObject.SetActive(false);
        }
    }

    private void OnSurrenderPressed()
    {
        _surrendering = true;
        if (PhotonRealtimeClient.InRoom)
        {
            PhotonRealtimeClient.LeaveRoom(false);
        }

        SceneManager.LoadScene("10-MenuUI");
    }

    private IEnumerator RetryMatchmakingAfterDelay()
    {
        yield return new WaitForSeconds(retryDelaySeconds);
        if (!_surrendering && !PhotonRealtimeClient.InRoom)
        {
            yield return WaitForPhotonReady();
            JoinOrCreateRaidRoom();
        }
    }

    private bool IsCurrentRoomRaid()
    {
        Room room = PhotonRealtimeClient.CurrentRoom;
        return room != null
            && GetRoomProperty(room, PhotonBattleRoom.GameTypeKey, (int)GameType.None) == (int)GameType.Raid
            && GetRoomProperty(room, RaidPhotonRoom.RaidMatchmakingKey, false);
    }

    private static T GetRoomInfoProperty<T>(RoomInfo room, string key, T defaultValue)
    {
        if (room?.CustomProperties == null || !room.CustomProperties.TryGetValue(key, out object value) || value == null)
        {
            return defaultValue;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private static T GetRoomProperty<T>(Room room, string key, T defaultValue)
    {
        if (room?.CustomProperties == null || !room.CustomProperties.TryGetValue(key, out object value) || value == null)
        {
            return defaultValue;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private static T GetPlayerProperty<T>(Player player, string key, T defaultValue)
    {
        if (player?.CustomProperties == null || !player.CustomProperties.TryGetValue(key, out object value) || value == null)
        {
            return defaultValue;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private void ResolveReferences()
    {
        _inventoryHandler = FindObjectOfType<Raid_InventoryHandler>();
        _inventoryPage = FindObjectOfType<Raid_InventoryPage>();
        _lootTracking = FindObjectOfType<Raid_LootTracking>();
        _raidTimer = FindObjectOfType<Raid_Timer>();
        _exitRaid = FindObjectOfType<ExitRaid>();
    }

    private void RegisterPhotonCallbacks()
    {
        if (_callbacksRegistered)
        {
            return;
        }

        PhotonRealtimeClient.AddCallbackTarget(this);
        _callbacksRegistered = true;
    }

    private void UnregisterPhotonCallbacks()
    {
        if (!_callbacksRegistered)
        {
            return;
        }

        PhotonRealtimeClient.RemoveCallbackTarget(this);
        _callbacksRegistered = false;
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
        if (!_joiningOrCreatingRoom && !_surrendering && !PhotonRealtimeClient.InRoom && !string.IsNullOrWhiteSpace(_localClanId))
        {
            JoinOrCreateRaidRoom();
        }
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        ShowMatchmaking("Raid matchmaking disconnected", cause.ToString(), string.Empty);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        ShowMatchmaking("Raid matchmaking authentication failed", debugMessage, string.Empty);
    }

    public void OnJoinedLobby()
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                _knownRooms.Remove(room.Name);
            }
            else
            {
                _knownRooms[room.Name] = room;
            }
        }
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnCreatedRoom()
    {
        _joiningOrCreatingRoom = false;
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        _joiningOrCreatingRoom = false;
        ShowMatchmaking("Finding Raid players", "Could not create a Raid room.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnJoinedRoom()
    {
        if (IsCurrentRoomRaid())
        {
            HandleJoinedRaidRoom();
        }
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        _joiningOrCreatingRoom = false;
        ShowMatchmaking("Finding Raid players", "Could not join that Raid room.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        _joiningOrCreatingRoom = false;
        ShowMatchmaking("Finding Raid players", "No matching Raid room was found.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnLeftRoom()
    {
        _inventoryInitialized = false;
        _gameplayReleased = false;
        _lootedSlots.Clear();

        if (_surrendering)
        {
            return;
        }

        ShowMatchmaking("Finding Raid players", "Rejoining Raid matchmaking...", string.Empty);
        if (_waitingForRetryLeave)
        {
            _waitingForRetryLeave = false;
        }

        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshParticipantList();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshParticipantList();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        ConfigureRaidFromRoomIfReady();
        RefreshParticipantList();
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        RefreshParticipantList();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == RaidPhotonRoom.LootRequestEvent)
        {
            if (!PhotonRealtimeClient.LocalPlayer.IsMasterClient)
            {
                return;
            }

            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 1)
            {
                return;
            }

            HandleLootRequest(photonEvent.Sender, Convert.ToInt32(data[0]));
            return;
        }

        if (photonEvent.Code == RaidPhotonRoom.LootAcceptedEvent)
        {
            ApplyLootAccepted(photonEvent.CustomData as object[]);
        }
    }
}
