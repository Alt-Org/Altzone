using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Photon.Client;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class RaidMatchmakingController : MonoBehaviour, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback
{
    private static bool _startNextRaidInDebugInventoryMode;
    private const float DebugInventoryModeRaidTimeSeconds = 45f;
    private const float DebugInventoryModeStartCountdownSeconds = 3f;

    [SerializeField] private float lobbyCountdownSeconds = 30f;
    [SerializeField] private float retryDelaySeconds = 1.25f;
    [SerializeField] private int minInventoryRows = 6;
    [SerializeField] private int maxInventoryRowsExclusive = 12;
    [SerializeField] private float matchmakingDotToggleSeconds = 0.5f;
    [SerializeField] private RaidMatchmakingViews _views;

    private readonly Dictionary<string, RoomInfo> _knownRooms = new();
    private readonly HashSet<string> _rejectedRoomNames = new(StringComparer.Ordinal);
    private readonly HashSet<int> _lootedSlots = new();

    private Raid_InventoryHandler _inventoryHandler;
    private Raid_InventoryPage _inventoryPage;
    private Raid_LootTracking _lootTracking;
    private Raid_Timer _raidTimer;
    private ExitRaid _exitRaid;
    private string _localPlayerName = string.Empty;
    private string _localPlayerId = string.Empty;
    private string _localClanId = string.Empty;
    private string _localClanName = string.Empty;
    private AvatarData _localAvatarData;

    private bool _joiningOrCreatingRoom;
    private bool _waitingForRetryLeave;
    private bool _surrendering;
    private bool _inventoryInitialized;
    private bool _gameplayReleased;
    private bool _sharedRaidActive;
    private bool _callbacksRegistered;
    private bool _matchmakingSearchVisualsEnabled;
    private bool _debugInventoryMode;
    private bool _lobbyCountdownActive;
    private bool _showFiveMatchmakingDots = true;
    private Coroutine _matchmakingDotsCoroutine;
    private Coroutine _lobbyCountdownCoroutine;
    private Coroutine _debugStartCoroutine;

    public static RaidMatchmakingController Instance { get; private set; }

    public event Action<bool> GameplayReleasedChanged;

    public bool ControlsInventorySetup => !_debugInventoryMode;
    public bool HasReleasedGameplay => _gameplayReleased;
    public bool IsSharedRaidActive => _sharedRaidActive && IsCurrentRoomRaid();
    public string LocalClanId => _localClanId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        _debugInventoryMode = _startNextRaidInDebugInventoryMode;
        _startNextRaidInDebugInventoryMode = false;

        Instance = this;
        ResolveReferences();
        CreateOverlayUi();
        RegisterPhotonCallbacks();
    }

    private void Start()
    {
        // Other singleton instances register during Awake, after this controller's early Awake.
        ResolveReferences();

        if (_debugInventoryMode)
        {
            StartDebugInventoryMode();
            return;
        }

        ShowMatchmakingSearchState(1);
        StartCoroutine(StartRaidMatchmakingFlow());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        StopMatchmakingDots();
        StopLobbyCountdownUpdates();
        UnregisterRaidTimerStarted();
        UnregisterPhotonCallbacks();
    }

    private IEnumerator StartRaidMatchmakingFlow()
    {
        yield return LoadLocalClanData();

        if (string.IsNullOrWhiteSpace(_localPlayerName) || string.IsNullOrWhiteSpace(_localPlayerId))
        {
            Debug.LogError("Raid matchmaking could not load valid local player data from the DataStore.");
            ShowMatchmakingMessage("Raid matchmaking unavailable", "Player data could not be loaded.", string.Empty);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_localClanId))
        {
            ShowMatchmakingMessage("Raid requires a clan", "Join a clan before entering Raid matchmaking.", string.Empty);
            yield break;
        }

        if (PhotonRealtimeClient.InRoom)
        {
            if (IsCurrentRoomRaid())
            {
                HandleJoinedRaidRoom();
                yield break;
            }

            ShowMatchmakingMessage("Preparing Raid matchmaking", "Leaving the previous room...", string.Empty);
            PhotonRealtimeClient.LeaveRoom(false);
            yield return new WaitUntil(() => !PhotonRealtimeClient.InRoom);
        }

        bool photonReady = false;
        yield return WaitForPhotonReady(isReady => photonReady = isReady);

        if (!photonReady)
        {
            ShowMatchmakingMessage("Raid matchmaking unavailable", "Photon is not ready for room matchmaking.", PhotonRealtimeClient.NetworkClientState.ToString());
            yield break;
        }

        JoinOrCreateRaidRoom();
    }

    public static void RestartNextSceneInDebugInventoryMode()
    {
        _startNextRaidInDebugInventoryMode = true;
    }

    private void StartDebugInventoryMode()
    {
        StopMatchmakingDots();
        StopLobbyCountdownUpdates();
        _surrendering = true;
        SetGameplayReleased(true);
        _sharedRaidActive = false;
        _inventoryInitialized = false;

        if (PhotonRealtimeClient.InRoom)
        {
            PhotonRealtimeClient.LeaveRoom(false);
        }

        _views?.Hide();

        if (_lootTracking != null)
        {
            _lootTracking.ResetLootCount();
        }

        if (_raidTimer != null)
        {
            _raidTimer.CurrentTime = DebugInventoryModeRaidTimeSeconds;
            _raidTimer.TimeUntilStart = DebugInventoryModeStartCountdownSeconds;
        }
    }

    private IEnumerator LoadLocalClanData()
    {
        PlayerData playerData = null;
        ClanData clanData = null;

        bool playerLoaded = false;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            playerData = data;
            playerLoaded = true;
        });

        yield return new WaitUntil(() => playerLoaded);

        if (playerData == null)
        {
            yield break;
        }

        _localPlayerName = playerData.Name;
        _localPlayerId = playerData.Id;
        _localClanId = playerData.ClanId ?? string.Empty;
        _localAvatarData = playerData.AvatarData;

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

        if (PhotonRealtimeClient.Client != null)
        {
            PhotonRealtimeClient.NickName = _localPlayerName;
        }
    }

    private IEnumerator WaitForPhotonReady(Action<bool> onCompleted)
    {
        if (PhotonRealtimeClient.Client != null && !PhotonRealtimeClient.IsConnected && PhotonRealtimeClient.CanConnect)
        {
            ShowMatchmakingMessage("Connecting to Raid matchmaking", "Connecting to Photon...", string.Empty);
            PhotonRealtimeClient.Connect(_localPlayerName);
        }

        float timeout = Time.time + 20f;
        while (!CanUsePhotonMatchmaking() && Time.time < timeout)
        {
            ShowMatchmakingMessage("Connecting to Raid matchmaking", "Waiting for Photon master server...", PhotonRealtimeClient.NetworkClientState.ToString());
            yield return new WaitForSeconds(0.25f);
        }

        onCompleted?.Invoke(CanUsePhotonMatchmaking());
    }

    private static bool CanUsePhotonMatchmaking()
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
        ShowMatchmakingSearchState(1);

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

    private static bool JoinRandomOrCreateRaidRoom()
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
                { RaidPhotonRoom.RaidMatchmakingKey, true }
            },
            ExpectedMaxPlayers = RaidPhotonRoom.RoomCapacity,
            Lobby = createArgs.Lobby
        };

        return PhotonRealtimeClient.Client.OpJoinRandomOrCreateRoom(joinRandomArgs, createArgs);
    }

    private static bool CreateRaidRoom()
    {
        EnterRoomArgs createArgs = new()
        {
            RoomName = $"Raid_{Guid.NewGuid():N}",
            RoomOptions = CreateRaidRoomOptions(),
            Lobby = PhotonRealtimeClient.Client.InLobby ? PhotonRealtimeClient.Client.CurrentLobby : null
        };

        return PhotonRealtimeClient.Client.OpCreateRoom(createArgs);
    }

    private static RoomOptions CreateRaidRoomOptions()
    {
        PhotonHashtable customRoomProperties = new()
        {
            { PhotonBattleRoom.GameTypeKey, (int)GameType.Raid },
            { RaidPhotonRoom.RaidMatchmakingKey, true },
            { RaidPhotonRoom.RaidClanCountsKey, string.Empty },
            { RaidPhotonRoom.RaidStateKey, (int)RaidPhotonRoom.RaidState.Matchmaking },
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

        RaidPhotonRoom.RaidState state = GetRoomInfoProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);
        if (state != RaidPhotonRoom.RaidState.Matchmaking && state != RaidPhotonRoom.RaidState.Lobby)
        {
            return false;
        }

        RaidPhotonRoom.ClanEntry[] clans = RaidPhotonRoom.GetClanCounts(room);
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

        if (!SetLocalPlayerRaidProperties())
        {
            return;
        }

        RefreshParticipantList();
        HandleCurrentRaidRoomState();

        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }

        StartCoroutine(ValidateLocalRoomMembership());
    }

    private bool SetLocalPlayerRaidProperties()
    {
        if (PhotonRealtimeClient.LocalPlayer == null || string.IsNullOrWhiteSpace(_localClanId))
        {
            return false;
        }

        return PhotonRealtimeClient.LocalPlayer.SetCustomProperties(new PhotonHashtable
        {
            { RaidPhotonRoom.PlayerIdKey, _localPlayerId },
            { RaidPhotonRoom.PlayerClanIdKey, _localClanId },
            { RaidPhotonRoom.PlayerClanNameKey, _localClanName },
            { RaidPhotonRoom.PlayerCharacterIdKey, GetLocalCharacterId() },
            { RaidPhotonRoom.PlayerAvatarDataKey, GetLocalAvatarPayload() }
        });
    }

    private IEnumerator ValidateLocalRoomMembership()
    {
        yield return new WaitForSeconds(0.5f);

        if (!PhotonRealtimeClient.InRoom)
        {
            yield break;
        }

        if (!IsCurrentRoomRaid())
        {
            _waitingForRetryLeave = true;
            ShowMatchmakingMessage("Finding Raid players", "Joined room is not a Raid matchmaking room.", "Trying another room...");
            PhotonRealtimeClient.LeaveRoom(false);
            yield break;
        }

        RaidPhotonRoom.RaidState state = GetRoomProperty(PhotonRealtimeClient.CurrentRoom, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);
        if (state != RaidPhotonRoom.RaidState.Matchmaking && state != RaidPhotonRoom.RaidState.Lobby)
        {
            _rejectedRoomNames.Add(PhotonRealtimeClient.CurrentRoom.Name);
            _waitingForRetryLeave = true;
            ShowMatchmakingMessage("Finding Raid players", "That Raid room has already started.", "Trying another room...");
            PhotonRealtimeClient.LeaveRoom(false);
            yield break;
        }

        int localClanPlayers = CountPlayersInClan(_localClanId);
        if (localClanPlayers > RaidPhotonRoom.MaxPlayersPerClan)
        {
            _rejectedRoomNames.Add(PhotonRealtimeClient.CurrentRoom.Name);
            _waitingForRetryLeave = true;
            ShowMatchmakingMessage("Finding Raid players", "Your clan already has two players in that Raid room.", "Trying another room...");
            PhotonRealtimeClient.LeaveRoom(false);
            yield break;
        }

        // Player properties have now had time to reach the room. Re-apply the
        // current room state so late joiners see the lobby and its clan list.
        RefreshParticipantList();
        HandleCurrentRaidRoomState();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    private void RefreshMasterRoomState()
    {
        if (!IsCurrentRoomRaid() || !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        RaidPhotonRoom.RaidState state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);
        if (state != RaidPhotonRoom.RaidState.Matchmaking && state != RaidPhotonRoom.RaidState.Lobby)
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

        if (state == RaidPhotonRoom.RaidState.Matchmaking && validClanFormation)
        {
            StartLobbyCountdown(validPlayers, clanEntries);
        }
        else if (state == RaidPhotonRoom.RaidState.Lobby
            && validClanFormation
            && validPlayers.Count == RaidPhotonRoom.RoomCapacity)
        {
            StartRaid(room);
        }
    }

    private void HandleCurrentRaidRoomState()
    {
        if (!IsCurrentRoomRaid())
        {
            StopLobbyCountdownUpdates();
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        RaidPhotonRoom.RaidState state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);

        if (state == RaidPhotonRoom.RaidState.Matchmaking)
        {
            StopLobbyCountdownUpdates();
            UpdateMatchmakingStatus();
            return;
        }

        if (state == RaidPhotonRoom.RaidState.Lobby)
        {
            ConfigureRaidFromRoomIfReady();
            ShowLobby();
            StartLobbyCountdownUpdates();
            return;
        }

        if (state == RaidPhotonRoom.RaidState.Started)
        {
            StopLobbyCountdownUpdates();
            ConfigureRaidFromRoomIfReady();
            BeginGameplay();
            return;
        }

        StopLobbyCountdownUpdates();
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

        long startTimeMs = DateTimeOffset.UtcNow.AddSeconds(lobbyCountdownSeconds).ToUnixTimeMilliseconds();
        room.SetCustomProperties(new PhotonHashtable
        {
            { RaidPhotonRoom.RaidClanCountsKey, RaidPhotonRoom.EncodeClanCounts(clanEntries) },
            { RaidPhotonRoom.RaidStateKey, (int)RaidPhotonRoom.RaidState.Lobby },
            { RaidPhotonRoom.RaidSetupReadyKey, true },
            { RaidPhotonRoom.RaidStartTimeKey, startTimeMs.ToString() },
            { RaidPhotonRoom.RaidInventorySizeKey, inventorySize },
            { RaidPhotonRoom.RaidInventorySeedKey, seed },
            { RaidPhotonRoom.RaidTrapSlotsKey, RaidPhotonRoom.EncodeTraps(traps) }
        });

        room.IsOpen = true;
        room.IsVisible = true;
        RefreshParticipantList(validPlayers);
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
        byte[] trapPayload = GetRoomProperty(room, RaidPhotonRoom.RaidTrapSlotsKey, Array.Empty<byte>());

        if (inventorySize <= 0)
        {
            return;
        }

        _lootTracking?.ResetLootCount();

        RaidPhotonRoom.TrapData[] traps = RaidPhotonRoom.DecodeTraps(trapPayload);
        if (_inventoryHandler != null)
        {
            _inventoryHandler.InitializeSharedInventory(inventorySize, seed, traps);
        }

        _inventoryInitialized = true;
        RefreshParticipantList();
    }

    private float UpdateLobbyCountdown()
    {
        if (!IsCurrentRoomRaid())
        {
            _lobbyCountdownActive = false;
            return 0f;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        RaidPhotonRoom.RaidState state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);
        if (state != RaidPhotonRoom.RaidState.Lobby)
        {
            _lobbyCountdownActive = false;
            return 0f;
        }

        long startTimeMs = GetRoomProperty(room, RaidPhotonRoom.RaidStartTimeKey, -1L);
        if (startTimeMs < 0)
        {
            return 0.25f;
        }

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long millisecondsRemaining = Math.Max(0L, startTimeMs - nowMs);
        TimeSpan remaining = TimeSpan.FromMilliseconds(millisecondsRemaining);
        string timeText = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:00}";
        _views?.SetLobbyCountdown(timeText);

        if (millisecondsRemaining == 0L && PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            StartRaid(room);
        }

        long millisecondsUntilNextSecond = millisecondsRemaining % 1000L;
        if (millisecondsUntilNextSecond == 0L)
        {
            millisecondsUntilNextSecond = 1000L;
        }

        return millisecondsUntilNextSecond / 1000f;
    }

    private static void StartRaid(Room room)
    {
        room.IsOpen = false;
        room.IsVisible = false;
        room.SetCustomProperty(
            RaidPhotonRoom.RaidStateKey,
            (int)RaidPhotonRoom.RaidState.Started);
    }

    private void StartLobbyCountdownUpdates()
    {
        _lobbyCountdownActive = true;

        if (_lobbyCountdownCoroutine == null)
        {
            _lobbyCountdownCoroutine = StartCoroutine(UpdateLobbyCountdownRoutine());
        }
    }

    private void StopLobbyCountdownUpdates()
    {
        _lobbyCountdownActive = false;
        if (_lobbyCountdownCoroutine != null)
        {
            StopCoroutine(_lobbyCountdownCoroutine);
            _lobbyCountdownCoroutine = null;
        }
    }

    private IEnumerator UpdateLobbyCountdownRoutine()
    {
        while (_lobbyCountdownActive)
        {
            float secondsUntilNextUpdate = UpdateLobbyCountdown();
            if (!_lobbyCountdownActive)
            {
                break;
            }

            yield return new WaitForSeconds(secondsUntilNextUpdate);
        }

        _lobbyCountdownCoroutine = null;
    }

    private void BeginGameplay()
    {
        if (_gameplayReleased)
        {
            return;
        }

        StopMatchmakingDots();
        StopLobbyCountdownUpdates();
        SetGameplayReleased(true);
        _sharedRaidActive = false;
        _views?.Hide();

        if (_raidTimer != null)
        {
            _raidTimer.TimerStarted -= OnRaidTimerStarted;
            _raidTimer.TimerStarted += OnRaidTimerStarted;

            if (_raidTimer.HasStarted)
            {
                OnRaidTimerStarted();
            }
        }
        else
        {
            _sharedRaidActive = true;
        }
    }

    private void SetGameplayReleased(bool released)
    {
        if (_gameplayReleased == released)
        {
            return;
        }

        _gameplayReleased = released;
        GameplayReleasedChanged?.Invoke(_gameplayReleased);
    }

    private void OnRaidTimerStarted()
    {
        if (IsCurrentRoomRaid() && !_surrendering && (_exitRaid == null || !_exitRaid.raidEnded))
        {
            _sharedRaidActive = true;
        }
    }

    private void ResetRaidStartCountdown()
    {
        _raidTimer?.ResetStartCountdown();
    }

    private void UnregisterRaidTimerStarted()
    {
        if (_raidTimer != null)
        {
            _raidTimer.TimerStarted -= OnRaidTimerStarted;
        }
    }

    public void RequestLoot(int slotIndex)
    {
        if (!CanProcessSharedLoot(slotIndex))
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
            RaidPhotonRoom.EncodeLootRequest(slotIndex),
            new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable);
    }

    private void HandleLootRequest(int senderActorNumber, int slotIndex)
    {
        bool requestFromLocalPlayer = PhotonRealtimeClient.LocalPlayer != null
            && PhotonRealtimeClient.LocalPlayer.ActorNumber == senderActorNumber;
        bool ignoreLocalPlayerState = !requestFromLocalPlayer;

        if (!CanProcessSharedLoot(slotIndex, ignoreLocalPlayerState) || _lootedSlots.Contains(slotIndex))
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
        if (item == null || item.ItemWeight <= 0f || item.FurnitureData == null)
        {
            return;
        }

        float weightMultiplier = _inventoryPage.GetNetworkLootWeightMultiplier(slotIndex);
        int characterId = GetPlayerCharacterId(sender);
        string avatarPayload = GetPlayerAvatarPayload(sender);
        RaidPhotonRoom.LootAcceptedData acceptedLootData = new(slotIndex, senderActorNumber, weightMultiplier, characterId, avatarPayload);
        byte[] acceptedLootBytes = RaidPhotonRoom.EncodeLootAccepted(acceptedLootData);
        bool eventRaised = PhotonRealtimeClient.Client.OpRaiseEvent(
            RaidPhotonRoom.LootAcceptedEvent,
            acceptedLootBytes,
            new RaiseEventArgs { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);

        if (eventRaised)
        {
            ApplyLootAccepted(acceptedLootBytes);
        }
    }

    private void ApplyLootAccepted(byte[] bytes)
    {
        if (_inventoryPage == null || !RaidPhotonRoom.TryDecodeLootAccepted(bytes, out RaidPhotonRoom.LootAcceptedData data))
        {
            return;
        }

        AvatarData avatarData = RaidPhotonRoom.DecodeAvatarData(data.AvatarPayload);
        bool triggeredByLocalPlayer = PhotonRealtimeClient.LocalPlayer != null
            && PhotonRealtimeClient.LocalPlayer.ActorNumber == data.ActorNumber;
        Player roomPlayer = GetRoomPlayer(data.ActorNumber);
        string actorName = GetPlayerDisplayName(roomPlayer, data.ActorNumber);

        _lootedSlots.Add(data.SlotIndex);
        _inventoryPage.HandleNetworkLootAccepted(data.SlotIndex, data.ActorNumber, data.WeightMultiplier, triggeredByLocalPlayer, actorName, (CharacterID)data.CharacterId, avatarData);
    }

    private bool CanProcessSharedLoot(int slotIndex, bool ignoreLocalPlayerState = false)
    {
        if (ignoreLocalPlayerState)
        {
            return IsCurrentRoomRaid()
                && _inventoryPage != null
                && _inventoryPage.HasLootableItem(slotIndex);
        }

        return IsSharedRaidActive
            && _inventoryPage != null
            && (_exitRaid == null || !_exitRaid.raidEnded)
            && _inventoryPage.CanRequestLoot(slotIndex);
    }

    private static bool IsParticipatingClan(string clanId)
    {
        if (string.IsNullOrWhiteSpace(clanId) || !IsCurrentRoomRaid())
        {
            return false;
        }

        string clans = GetRoomProperty(PhotonRealtimeClient.CurrentRoom, RaidPhotonRoom.RaidClanCountsKey, string.Empty);
        return RaidPhotonRoom.DecodeClanCounts(clans).Any(clan => clan.ClanId == clanId);
    }

    private static Player GetRoomPlayer(int actorNumber)
    {
        if (!IsCurrentRoomRaid())
        {
            return null;
        }

        PhotonRealtimeClient.CurrentRoom.Players.TryGetValue(actorNumber, out Player player);
        return player;
    }

    private static string GetPlayerDisplayName(Player player, int actorNumber)
    {
        if (!string.IsNullOrWhiteSpace(player?.NickName))
        {
            return player.NickName;
        }

        return actorNumber > 0 ? $"Player {actorNumber}" : "Player";
    }

    private static string GetClanDisplayName(Player player, int actorNumber)
    {
        string clanName = GetPlayerClanName(player);
        return string.IsNullOrWhiteSpace(clanName)
            ? GetPlayerDisplayName(player, actorNumber)
            : clanName;
    }

    private static List<Player> GetRaidPlayersWithClans()
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

    private static RaidPhotonRoom.ClanEntry[] BuildClanEntries(List<Player> players)
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

    private static int CountPlayersInClan(string clanId)
    {
        if (string.IsNullOrWhiteSpace(clanId) || !IsCurrentRoomRaid())
        {
            return 0;
        }

        return PhotonRealtimeClient.CurrentRoom.Players.Values.Count(player => GetPlayerClanId(player) == clanId);
    }

    private static string GetPlayerClanId(Player player)
    {
        return GetPlayerProperty(player, RaidPhotonRoom.PlayerClanIdKey, string.Empty);
    }

    private static string GetPlayerClanName(Player player)
    {
        return GetPlayerProperty(player, RaidPhotonRoom.PlayerClanNameKey, string.Empty);
    }

    private static int GetPlayerCharacterId(Player player)
    {
        return GetPlayerProperty(player, RaidPhotonRoom.PlayerCharacterIdKey, (int)CharacterID.None);
    }

    private string GetPlayerAvatarPayload(Player player)
    {
        string payload = GetPlayerProperty(player, RaidPhotonRoom.PlayerAvatarDataKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(payload))
        {
            return payload;
        }

        return player != null && PhotonRealtimeClient.LocalPlayer != null && player.ActorNumber == PhotonRealtimeClient.LocalPlayer.ActorNumber
            ? GetLocalAvatarPayload()
            : string.Empty;
    }

    private string GetLocalAvatarPayload()
    {
        return RaidPhotonRoom.EncodeAvatarData(GetLocalAvatarData());
    }

    private AvatarData GetLocalAvatarData()
    {
        if (_localAvatarData != null)
        {
            return _localAvatarData;
        }

        ServerPlayer serverPlayer = ServerManager.Instance?.Player;
        if (serverPlayer?.avatar != null)
        {
            _localAvatarData = new AvatarData(serverPlayer.name ?? _localPlayerName, serverPlayer.avatar);
            return _localAvatarData;
        }

        return _localAvatarData;
    }

    private static int GetLocalCharacterId()
    {
        int? currentAvatarId = ServerManager.Instance?.Player?.currentAvatarId;
        if (currentAvatarId.HasValue && Enum.IsDefined(typeof(CharacterID), currentAvatarId.Value))
        {
            return currentAvatarId.Value;
        }

        return (int)CharacterID.None;
    }

    private void UpdateMatchmakingStatus()
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        ShowMatchmakingSearchState(GetCurrentRaidPlayerCount());
    }

    private static int GetCurrentRaidPlayerCount()
    {
        return IsCurrentRoomRaid() ? GetRaidPlayersWithClans().Count : 1;
    }

    private void ShowMatchmakingMessage(string title, string status, string detail)
    {
        _matchmakingSearchVisualsEnabled = false;
        StopMatchmakingDots();
        _views?.ShowMatchmaking();
        _views?.UpdateMatchmakingTexts(title, status, detail);
    }

    private void ShowMatchmakingSearchState(int currentPlayers)
    {
        _matchmakingSearchVisualsEnabled = true;
        _views?.ShowMatchmakingSearchState(currentPlayers, _showFiveMatchmakingDots);
        StartMatchmakingDots();
    }

    private void StartMatchmakingDots()
    {
        if (_matchmakingDotsCoroutine == null)
        {
            _matchmakingDotsCoroutine = StartCoroutine(UpdateMatchmakingDotsRoutine());
        }
    }

    private void StopMatchmakingDots()
    {
        _matchmakingSearchVisualsEnabled = false;
        if (_matchmakingDotsCoroutine != null)
        {
            StopCoroutine(_matchmakingDotsCoroutine);
            _matchmakingDotsCoroutine = null;
        }
    }

    private IEnumerator UpdateMatchmakingDotsRoutine()
    {
        while (_matchmakingSearchVisualsEnabled)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, matchmakingDotToggleSeconds));

            if (_views == null || !_views.IsMatchmakingPanelVisible)
            {
                continue;
            }

            _showFiveMatchmakingDots = !_showFiveMatchmakingDots;
            _views.UpdateMatchmakingDots(_showFiveMatchmakingDots);
        }

        _matchmakingDotsCoroutine = null;
    }

    private void ShowLobby()
    {
        StopMatchmakingDots();
        _views?.ShowLobby();
    }

    private void RefreshParticipantList()
    {
        RefreshParticipantList(IsCurrentRoomRaid()
            ? PhotonRealtimeClient.CurrentRoom.Players.Values.OrderBy(player => player.ActorNumber).ToList()
            : new List<Player>());
    }

    private void RefreshParticipantList(List<Player> players)
    {
        List<RaidLobbyClanRowData> clanRows = players
            .Where(player => !string.IsNullOrWhiteSpace(GetPlayerClanId(player)))
            .GroupBy(GetPlayerClanId)
            .OrderBy(group => group.Min(player => player.ActorNumber))
            .Select(group => new RaidLobbyClanRowData(
                group.Select(GetPlayerClanName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? group.Key,
                group.Count(),
                group.OrderBy(player => player.ActorNumber).Select(CreatePlayerIconData).ToList()))
            .ToList();

        _views?.RefreshParticipantList(clanRows, RaidPhotonRoom.MaxPlayersPerClan);
    }

    private RaidPlayerIconData CreatePlayerIconData(Player player)
    {
        int characterId = GetPlayerCharacterId(player);
        CharacterID character = Enum.IsDefined(typeof(CharacterID), characterId)
            ? (CharacterID)characterId
            : CharacterID.None;

        return new RaidPlayerIconData(
            GetPlayerDisplayName(player, player?.ActorNumber ?? 0),
            character,
            RaidPhotonRoom.DecodeAvatarData(GetPlayerAvatarPayload(player)));
    }

    private void CreateOverlayUi()
    {
        if (_views == null)
        {
            Debug.LogError("Raid matchmaking views are not assigned. Assign RaidMatchmakingViews in 40-Raid.");
            return;
        }

        _views.Initialize(OnSurrenderPressed, OnDebugStartPressed);
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

    private void OnDebugStartPressed()
    {
        if (_debugStartCoroutine != null)
        {
            return;
        }

        _debugStartCoroutine = StartCoroutine(DebugStartRaidWhenReady());
    }

    private IEnumerator DebugStartRaidWhenReady()
    {
        if (!IsCurrentRoomRaid())
        {
            ShowMatchmakingMessage(
                GetLocalizedText("Raidin debug-aloitus ei ole k\u00E4ytett\u00E4viss\u00E4", "Debug Raid start unavailable"),
                GetLocalizedText("Liity ensin Raid-pelinmuodostushuoneeseen.", "Join a Raid matchmaking room first."),
                string.Empty);
            _debugStartCoroutine = null;
            yield break;
        }

        if (!SetLocalPlayerRaidProperties())
        {
            _debugStartCoroutine = null;
            yield break;
        }

        float timeout = Time.time + 2f;
        while (IsCurrentRoomRaid()
            && PhotonRealtimeClient.LocalPlayer != null
            && string.IsNullOrWhiteSpace(GetPlayerClanId(PhotonRealtimeClient.LocalPlayer))
            && Time.time < timeout)
        {
            ShowMatchmakingMessage(
                GetLocalizedText("K\u00E4ynnistet\u00E4\u00E4n Raid debug-tilassa", "Debug starting Raid"),
                GetLocalizedText("Odotetaan pelaajan klaanitietoja...", "Waiting for player clan data..."),
                string.Empty);
            yield return new WaitForSeconds(0.1f);
        }

        TryDebugStartRaidIgnoringRequiredPlayers();
        _debugStartCoroutine = null;
    }

    private void TryDebugStartRaidIgnoringRequiredPlayers()
    {
        if (!IsCurrentRoomRaid())
        {
            ShowMatchmakingMessage(
                GetLocalizedText("Raidin debug-aloitus ei ole k\u00E4ytett\u00E4viss\u00E4", "Debug Raid start unavailable"),
                GetLocalizedText("Liity ensin Raid-pelinmuodostushuoneeseen.", "Join a Raid matchmaking room first."),
                string.Empty);
            return;
        }

        if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            ShowMatchmakingMessage(
                GetLocalizedText("Raidin debug-aloitus ei ole k\u00E4ytett\u00E4viss\u00E4", "Debug Raid start unavailable"),
                GetLocalizedText("Vain huoneen johtaja voi pakottaa Raidin alkamaan.", "Only the room leader can force-start a Raid."),
                string.Empty);
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        RaidPhotonRoom.RaidState state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.RaidState.Error);
        if (state != RaidPhotonRoom.RaidState.Matchmaking)
        {
            ShowMatchmakingMessage(
                GetLocalizedText("Raidin debug-aloitus ei ole k\u00E4ytett\u00E4viss\u00E4", "Debug Raid start unavailable"),
                GetLocalizedText("Raid on jo poistumassa pelinmuodostuksesta.", "Raid is already leaving matchmaking."),
                string.Empty);
            return;
        }

        List<Player> validPlayers = GetRaidPlayersWithClans();
        RaidPhotonRoom.ClanEntry[] clanEntries = BuildClanEntries(validPlayers);
        bool canDebugStart = validPlayers.Count > 0
            && validPlayers.Count <= RaidPhotonRoom.RoomCapacity
            && clanEntries.All(clan => clan.Count <= RaidPhotonRoom.MaxPlayersPerClan);

        if (!canDebugStart)
        {
            ShowMatchmakingMessage(
                GetLocalizedText("Raidin debug-aloitus ei ole k\u00E4ytett\u00E4viss\u00E4", "Debug Raid start unavailable"),
                GetLocalizedText("Yht\u00E4\u00E4n kelvollista Raid-pelaajaa ei ole viel\u00E4 valmiina.", "No valid Raid players are ready yet."),
                GetLocalizedText("Odota pelaajien klaanitietojen synkronoitumista ja yrit\u00E4 sitten uudelleen.", "Wait for player clan data to sync, then try again."));
            return;
        }

        ShowMatchmakingMessage(
            GetLocalizedText("K\u00E4ynnistet\u00E4\u00E4n Raid debug-tilassa", "Debug starting Raid"),
            GetLocalizedText("Aloitetaan ilman vaadittua pelaajam\u00E4\u00E4r\u00E4\u00E4.", "Starting without required player count."),
            GetLocalizedText(
                $"{validPlayers.Count}/{RaidPhotonRoom.RequiredPlayers} pelaajaa",
                $"{validPlayers.Count}/{RaidPhotonRoom.RequiredPlayers} players"));
        StartLobbyCountdown(validPlayers, clanEntries);
    }

    private static string GetLocalizedText(string finnishText, string englishText)
    {
        return SettingsCarrier.Instance != null
            && SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English
                ? englishText
                : finnishText;
    }

    private IEnumerator RetryMatchmakingAfterDelay()
    {
        yield return new WaitForSeconds(retryDelaySeconds);
        if (!_surrendering && !PhotonRealtimeClient.InRoom)
        {
            bool photonReady = false;
            yield return WaitForPhotonReady(isReady => photonReady = isReady);
            if (photonReady)
            {
                JoinOrCreateRaidRoom();
            }
        }
    }

    private static bool IsCurrentRoomRaid()
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
            if (typeof(T).IsEnum)
            {
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)));
                T enumValue = (T)Enum.ToObject(typeof(T), underlyingValue);
                return Enum.IsDefined(typeof(T), enumValue) ? enumValue : defaultValue;
            }

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
            if (typeof(T).IsEnum)
            {
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)));
                T enumValue = (T)Enum.ToObject(typeof(T), underlyingValue);
                return Enum.IsDefined(typeof(T), enumValue) ? enumValue : defaultValue;
            }

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
        Raid_References raidReferences = Raid_References.Instance;
        if (raidReferences == null)
        {
            TryGetComponent(out raidReferences);
        }

        _inventoryHandler = raidReferences?.InventoryHandler;
        _inventoryPage = raidReferences?.InventoryPage;
        _lootTracking = raidReferences?.LootTracking;
        _raidTimer = raidReferences?.RaidTimer;
        _exitRaid = ExitRaid.Instance;
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
        ShowMatchmakingMessage("Raid matchmaking disconnected", cause.ToString(), string.Empty);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        ShowMatchmakingMessage("Raid matchmaking authentication failed", debugMessage, string.Empty);
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
        ShowMatchmakingMessage("Finding Raid players", "Could not create a Raid room.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnJoinedRoom()
    {
        _joiningOrCreatingRoom = false;

        if (!IsCurrentRoomRaid())
        {
            StartCoroutine(ValidateLocalRoomMembership());
            return;
        }

        HandleJoinedRaidRoom();
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        _joiningOrCreatingRoom = false;
        ShowMatchmakingMessage("Finding Raid players", "Could not join that Raid room.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        _joiningOrCreatingRoom = false;
        ShowMatchmakingMessage("Finding Raid players", "No matching Raid room was found.", message);
        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnLeftRoom()
    {
        ResetRaidStartCountdown();
        _inventoryInitialized = false;
        SetGameplayReleased(false);
        _sharedRaidActive = false;
        StopLobbyCountdownUpdates();
        _lootedSlots.Clear();

        if (_surrendering)
        {
            return;
        }

        ShowMatchmakingMessage("Finding Raid players", "Rejoining Raid matchmaking...", string.Empty);
        if (_waitingForRetryLeave)
        {
            _waitingForRetryLeave = false;
        }

        StartCoroutine(RetryMatchmakingAfterDelay());
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        RefreshParticipantList();
        HandleCurrentRaidRoomState();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        RefreshParticipantList();
        HandleCurrentRaidRoomState();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        bool raidStateChanged = propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidStateKey);
        bool setupChanged = propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidSetupReadyKey)
            || propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidInventorySizeKey)
            || propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidInventorySeedKey)
            || propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidTrapSlotsKey);
        bool lobbyDisplayChanged = propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidClanCountsKey)
            || propertiesThatChanged.ContainsKey(RaidPhotonRoom.RaidStartTimeKey);

        if (raidStateChanged || setupChanged || lobbyDisplayChanged)
        {
            HandleCurrentRaidRoomState();
        }

        if (setupChanged || lobbyDisplayChanged)
        {
            RefreshParticipantList();
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        RefreshParticipantList();
        HandleCurrentRaidRoomState();
        if (PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            RefreshMasterRoomState();
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!IsCurrentRoomRaid())
        {
            return;
        }

        HandleCurrentRaidRoomState();
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

            byte[] bytes = GetEventBytes(photonEvent.CustomData);
            if (!RaidPhotonRoom.TryDecodeLootRequest(bytes, out int slotIndex))
            {
                return;
            }

            HandleLootRequest(photonEvent.Sender, slotIndex);
            return;
        }

        if (photonEvent.Code == RaidPhotonRoom.LootAcceptedEvent)
        {
            ApplyLootAccepted(GetEventBytes(photonEvent.CustomData));
            return;
        }

    }

    private static byte[] GetEventBytes(object customData)
    {
        if (customData is byte[] bytes)
        {
            return bytes;
        }

        if (customData is ByteArraySlice slice)
        {
            byte[] sliceBytes = new byte[slice.Count];
            Buffer.BlockCopy(slice.Buffer, slice.Offset, sliceBytes, 0, slice.Count);
            return sliceBytes;
        }

        return null;
    }
}
