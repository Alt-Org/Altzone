using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Language;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
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
    private static bool _startNextRaidInDebugInventoryMode;
    private const float DebugInventoryModeRaidTimeSeconds = 45f;
    private const float DebugInventoryModeStartCountdownSeconds = 3f;

    [SerializeField] private float lobbyCountdownSeconds = 10f;
    [SerializeField] private float retryDelaySeconds = 1.25f;
    [SerializeField] private int minInventoryRows = 6;
    [SerializeField] private int maxInventoryRowsExclusive = 12;
    [SerializeField] private float matchmakingDotToggleSeconds = 0.5f;
    [SerializeField] private float matchmakingDotSpacing = 48f;

    private readonly Dictionary<string, RoomInfo> _knownRooms = new();
    private readonly HashSet<string> _rejectedRoomNames = new(StringComparer.Ordinal);
    private readonly HashSet<int> _lootedSlots = new();

    private Raid_InventoryHandler _inventoryHandler;
    private Raid_InventoryPage _inventoryPage;
    private Raid_LootTracking _lootTracking;
    private Raid_Timer _raidTimer;
    private ExitRaid _exitRaid;
    private RaidMatchmakingViews _views;
    private GameObject _overlayRoot;
    private GameObject _matchmakingPanel;
    private GameObject _lobbyPanel;
    private TextMeshProUGUI _matchmakingTitleText;
    private TextMeshProUGUI _matchmakingStatusText;
    private TextMeshProUGUI _matchmakingDetailText;
    private GameObject[] _matchmakingDots;
    private TextMeshProUGUI _lobbyCountdownText;
    private Transform _participantListRoot;
    private RaidLobbyClanListItem _clanListItemTemplate;

    private string _localPlayerName = "Player";
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
    private float _nextMatchmakingDotToggleTime;
    private Coroutine _debugStartCoroutine;

    public static RaidMatchmakingController Instance { get; private set; }

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
        if (_debugInventoryMode)
        {
            StartDebugInventoryMode();
            return;
        }

        ShowMatchmakingSearchState(1);
        StartCoroutine(StartRaidMatchmakingFlow());
    }

    private void Update()
    {
        UpdateMatchmakingDots();

        if (_lobbyCountdownActive)
        {
            UpdateLobbyCountdown();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnregisterRaidTimerStarted();
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

    public static void RestartNextSceneInDebugInventoryMode()
    {
        _startNextRaidInDebugInventoryMode = true;
    }

    private void StartDebugInventoryMode()
    {
        _surrendering = true;
        _gameplayReleased = true;
        _sharedRaidActive = false;
        _inventoryInitialized = false;

        if (PhotonRealtimeClient.InRoom)
        {
            PhotonRealtimeClient.LeaveRoom(false);
        }

        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(false);
        }

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

        if (ServerManager.Instance != null)
        {
            ServerPlayer serverPlayer = ServerManager.Instance.Player;
            _localPlayerName = serverPlayer?.name ?? _localPlayerName;
            _localPlayerId = serverPlayer?._id ?? serverPlayer?.uniqueIdentifier ?? string.Empty;
            _localClanId = serverPlayer?.clan_id ?? string.Empty;
            _localClanName = ServerManager.Instance.Clan?.name ?? string.Empty;
            if (serverPlayer?.avatar != null)
            {
                _localAvatarData = new AvatarData(_localPlayerName, serverPlayer.avatar);
            }
        }

        if (string.IsNullOrWhiteSpace(_localClanId) || _localAvatarData == null)
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
                if (string.IsNullOrWhiteSpace(_localPlayerId))
                {
                    _localPlayerId = !string.IsNullOrWhiteSpace(playerData.Id)
                        ? playerData.Id
                        : playerData.UniqueIdentifier ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(_localClanId))
                {
                    _localClanId = playerData.ClanId ?? string.Empty;
                }

                _localAvatarData ??= playerData.AvatarData;
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

        if (string.IsNullOrWhiteSpace(_localPlayerId))
        {
            _localPlayerId = GameConfig.Get().PlayerSettings.PlayerGuid;
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
        RefreshParticipantList();
        HandleCurrentRaidRoomState();

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

    private void HandleCurrentRaidRoomState()
    {
        if (!IsCurrentRoomRaid())
        {
            _lobbyCountdownActive = false;
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        int state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);

        if (state == RaidPhotonRoom.StateMatchmaking)
        {
            _lobbyCountdownActive = false;
            UpdateMatchmakingStatus();
            return;
        }

        if (state == RaidPhotonRoom.StateLobby)
        {
            ConfigureRaidFromRoomIfReady();
            ShowLobby();
            _lobbyCountdownActive = true;
            UpdateLobbyCountdown();
            return;
        }

        if (state == RaidPhotonRoom.StateStarted)
        {
            _lobbyCountdownActive = false;
            ConfigureRaidFromRoomIfReady();
            BeginGameplay();
            return;
        }

        _lobbyCountdownActive = false;
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
            { RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateLobby },
            { RaidPhotonRoom.RaidSetupReadyKey, true },
            { RaidPhotonRoom.RaidStartTimeKey, startTimeMs.ToString() },
            { RaidPhotonRoom.RaidInventorySizeKey, inventorySize },
            { RaidPhotonRoom.RaidInventorySeedKey, seed },
            { RaidPhotonRoom.RaidTrapSlotsKey, RaidPhotonRoom.EncodeTraps(traps) }
        });

        room.IsOpen = false;
        room.IsVisible = false;
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
        string trapPayload = GetRoomProperty(room, RaidPhotonRoom.RaidTrapSlotsKey, string.Empty);

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

    private void UpdateLobbyCountdown()
    {
        if (!IsCurrentRoomRaid())
        {
            _lobbyCountdownActive = false;
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        int state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);
        if (state != RaidPhotonRoom.StateLobby)
        {
            _lobbyCountdownActive = false;
            return;
        }

        string startTimeValue = GetRoomProperty(room, RaidPhotonRoom.RaidStartTimeKey, string.Empty);
        if (!long.TryParse(startTimeValue, out long startTimeMs))
        {
            return;
        }

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float secondsRemaining = Mathf.Max(0f, (startTimeMs - nowMs) / 1000f);
        TimeSpan remaining = TimeSpan.FromSeconds(Mathf.CeilToInt(secondsRemaining));
        string timeText = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:00}";
        SetLocalizedText(_lobbyCountdownText, "Kokoaminen alkaa\n{0}", "Gathering starts\n{0}", timeText);

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
        _sharedRaidActive = false;
        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(false);
        }

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

    private void OnRaidTimerStarted()
    {
        if (IsCurrentRoomRaid() && !_surrendering && (_exitRaid == null || !_exitRaid.raidEnded))
        {
            _sharedRaidActive = true;
        }
    }

    private void ResetRaidStartCountdown()
    {
        UnregisterRaidTimerStarted();
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
            new object[] { slotIndex },
            new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable);
    }

    private void HandleLootRequest(int senderActorNumber, int slotIndex)
    {
        if (!CanProcessSharedLoot(slotIndex) || _lootedSlots.Contains(slotIndex))
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

        _lootedSlots.Add(slotIndex);
        float weightMultiplier = _inventoryPage.GetNetworkLootWeightMultiplier(slotIndex);
        int characterId = GetPlayerCharacterId(sender);
        string avatarPayload = GetPlayerAvatarPayload(sender);
        object[] acceptedLootData = { slotIndex, senderActorNumber, weightMultiplier, characterId, avatarPayload };
        bool eventRaised = PhotonRealtimeClient.Client.OpRaiseEvent(
            RaidPhotonRoom.LootAcceptedEvent,
            acceptedLootData,
            new RaiseEventArgs { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);

        if (eventRaised)
        {
            ApplyLootAccepted(acceptedLootData);
        }
        else
        {
            _lootedSlots.Remove(slotIndex);
        }
    }

    private void ApplyLootAccepted(object[] data)
    {
        if (data == null || data.Length < 3 || _inventoryPage == null)
        {
            return;
        }

        int slotIndex = Convert.ToInt32(data[0]);
        int actorNumber = Convert.ToInt32(data[1]);
        float weightMultiplier = Convert.ToSingle(data[2]);
        int characterId = data.Length >= 4 ? Convert.ToInt32(data[3]) : (int)CharacterID.None;
        string avatarPayload = data.Length >= 5 ? data[4] as string ?? string.Empty : string.Empty;
        AvatarData avatarData = RaidPhotonRoom.DecodeAvatarData(avatarPayload);
        bool triggeredByLocalPlayer = PhotonRealtimeClient.LocalPlayer != null
            && PhotonRealtimeClient.LocalPlayer.ActorNumber == actorNumber;
        Player roomPlayer = GetRoomPlayer(actorNumber);
        string actorName = GetPlayerDisplayName(roomPlayer, actorNumber);

        _lootedSlots.Add(slotIndex);
        _inventoryPage.HandleNetworkLootAccepted(slotIndex, actorNumber, weightMultiplier, triggeredByLocalPlayer, actorName, (CharacterID)characterId, avatarData);
    }

    private bool CanProcessSharedLoot(int slotIndex)
    {
        return IsSharedRaidActive
            && _inventoryPage != null
            && (_exitRaid == null || !_exitRaid.raidEnded)
            && _inventoryPage.CanRequestLoot(slotIndex);
    }

    private bool IsParticipatingClan(string clanId)
    {
        if (string.IsNullOrWhiteSpace(clanId) || !IsCurrentRoomRaid())
        {
            return false;
        }

        string clans = GetRoomProperty(PhotonRealtimeClient.CurrentRoom, RaidPhotonRoom.RaidClanCountsKey, string.Empty);
        return RaidPhotonRoom.DecodeClanCounts(clans).Any(clan => clan.ClanId == clanId);
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

    private static string GetPlayerDisplayName(Player player, int actorNumber)
    {
        if (!string.IsNullOrWhiteSpace(player?.NickName))
        {
            return player.NickName;
        }

        return actorNumber > 0 ? $"Player {actorNumber}" : "Player";
    }

    private string GetClanDisplayName(Player player, int actorNumber)
    {
        string clanName = GetPlayerClanName(player);
        return string.IsNullOrWhiteSpace(clanName)
            ? GetPlayerDisplayName(player, actorNumber)
            : clanName;
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

    private int GetPlayerCharacterId(Player player)
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

    private int GetLocalCharacterId()
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

        int currentPlayers = GetRaidPlayersWithClans().Count;
        ShowMatchmakingSearchState(currentPlayers);
    }

    private void ShowMatchmaking(string title, string status, string detail)
    {
        _matchmakingSearchVisualsEnabled = false;

        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(true);
        }

        if (_matchmakingPanel != null)
        {
            _matchmakingPanel.SetActive(true);
        }

        if (_lobbyPanel != null)
        {
            _lobbyPanel.SetActive(false);
        }

        SetText(_matchmakingTitleText, title);
        SetText(_matchmakingStatusText, status);
        SetText(_matchmakingDetailText, detail);
        SetTextActive(_matchmakingStatusText, !string.IsNullOrWhiteSpace(status));
        SetTextActive(_matchmakingDetailText, !string.IsNullOrWhiteSpace(detail));
        SetMatchmakingDotsActive(0);
    }

    private void ShowMatchmakingSearchState(int currentPlayers)
    {
        _matchmakingSearchVisualsEnabled = true;

        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(true);
        }

        if (_matchmakingPanel != null)
        {
            _matchmakingPanel.SetActive(true);
        }

        if (_lobbyPanel != null)
        {
            _lobbyPanel.SetActive(false);
        }

        bool playersFound = currentPlayers > 1;
        SetText(
            _matchmakingTitleText,
            GetCurrentLanguage() == SettingsCarrier.LanguageType.English
                ? (playersFound ? "Players found" : "Searching for players")
                : (playersFound ? "Pelaajia l\u00F6ydetty" : "Etsit\u00E4\u00E4n pelaajia"));

        SetTextActive(_matchmakingStatusText, playersFound);
        if (playersFound)
        {
            SetText(_matchmakingStatusText, $"{currentPlayers} / {RaidPhotonRoom.RequiredPlayers}");
        }

        SetTextActive(_matchmakingDetailText, false);
        SetMatchmakingDotText();
    }

    private void UpdateMatchmakingDots()
    {
        if (!_matchmakingSearchVisualsEnabled || _matchmakingPanel == null || !_matchmakingPanel.activeInHierarchy)
        {
            return;
        }

        if (Time.time < _nextMatchmakingDotToggleTime)
        {
            return;
        }

        _showFiveMatchmakingDots = !_showFiveMatchmakingDots;
        _nextMatchmakingDotToggleTime = Time.time + Mathf.Max(0.1f, matchmakingDotToggleSeconds);
        SetMatchmakingDotText();
    }

    private void SetMatchmakingDotText()
    {
        SetMatchmakingDotsActive(_showFiveMatchmakingDots ? 5 : 4);
    }

    private void ShowLobby()
    {
        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(true);
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

        if (_clanListItemTemplate == null)
        {
            Debug.LogError("Raid lobby clan list item template is not assigned in RaidMatchmakingViews.");
            return;
        }

        for (int i = _participantListRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = _participantListRoot.GetChild(i);
            if (child != _clanListItemTemplate.transform)
            {
                Destroy(child.gameObject);
            }
        }

        var clanRows = players
            .Where(player => !string.IsNullOrWhiteSpace(GetPlayerClanId(player)))
            .GroupBy(GetPlayerClanId)
            .Select(group => new
            {
                ClanName = group.Select(GetPlayerClanName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? group.Key,
                PlayerCount = group.Count(),
                FirstActorNumber = group.Min(player => player.ActorNumber),
                Players = group.OrderBy(player => player.ActorNumber).ToList()
            })
            .OrderBy(row => row.FirstActorNumber)
            .ToList();

        int rowCount = clanRows.Count;
        for (int i = 0; i < rowCount; i++)
        {
            RaidLobbyClanListItem row = Instantiate(_clanListItemTemplate, _participantListRoot);
            row.name = $"Clan {i + 1}";
            row.gameObject.SetActive(true);
            row.SetTemplateStackPosition(i, rowCount, 0.04f);
            row.Configure(
                clanRows[i].ClanName,
                clanRows[i].PlayerCount,
                RaidPhotonRoom.MaxPlayersPerClan,
                clanRows[i].Players.Select(CreatePlayerIconData).ToList());
        }
    }

    private RaidLobbyClanListItem.PlayerIconData CreatePlayerIconData(Player player)
    {
        int characterId = GetPlayerCharacterId(player);
        CharacterID character = Enum.IsDefined(typeof(CharacterID), characterId)
            ? (CharacterID)characterId
            : CharacterID.None;

        return new RaidLobbyClanListItem.PlayerIconData(
            GetPlayerDisplayName(player, player?.ActorNumber ?? 0),
            character,
            RaidPhotonRoom.DecodeAvatarData(GetPlayerAvatarPayload(player)));
    }

    private void CreateOverlayUi()
    {
        _views = FindObjectsOfType<RaidMatchmakingViews>(true)
            .FirstOrDefault(view => view.gameObject.scene.IsValid());

        if (_views == null)
        {
            Debug.LogError("Raid matchmaking views are missing from the Raid scene. Add RaidMatchmakingViews to 40-Raid.");
            return;
        }

        _views.Initialize(OnSurrenderPressed, OnDebugStartPressed);
        AssignViewReferences(_views);
    }

    private void AssignViewReferences(RaidMatchmakingViews views)
    {
        _overlayRoot = views.Root;
        _matchmakingPanel = views.MatchmakingPanel;
        _lobbyPanel = views.LobbyPanel;
        _matchmakingTitleText = views.MatchmakingTitleText;
        _matchmakingStatusText = views.MatchmakingStatusText;
        _matchmakingDetailText = views.MatchmakingDetailText;
        _matchmakingDots = views.MatchmakingDots;
        ResolveMatchmakingDotsFallback();
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
            ShowMatchmaking("Debug Raid start unavailable", "Join a Raid matchmaking room first.", string.Empty);
            _debugStartCoroutine = null;
            yield break;
        }

        SetLocalPlayerRaidProperties();

        float timeout = Time.time + 2f;
        while (IsCurrentRoomRaid()
            && PhotonRealtimeClient.LocalPlayer != null
            && string.IsNullOrWhiteSpace(GetPlayerClanId(PhotonRealtimeClient.LocalPlayer))
            && Time.time < timeout)
        {
            ShowMatchmaking("Debug starting Raid", "Waiting for player clan data...", string.Empty);
            yield return new WaitForSeconds(0.1f);
        }

        TryDebugStartRaidIgnoringRequiredPlayers();
        _debugStartCoroutine = null;
    }

    private void TryDebugStartRaidIgnoringRequiredPlayers()
    {
        if (!IsCurrentRoomRaid())
        {
            ShowMatchmaking("Debug Raid start unavailable", "Join a Raid matchmaking room first.", string.Empty);
            return;
        }

        if (PhotonRealtimeClient.LocalPlayer == null || !PhotonRealtimeClient.LocalPlayer.IsMasterClient)
        {
            ShowMatchmaking("Debug Raid start unavailable", "Only the room leader can force-start a Raid.", string.Empty);
            return;
        }

        Room room = PhotonRealtimeClient.CurrentRoom;
        int state = GetRoomProperty(room, RaidPhotonRoom.RaidStateKey, RaidPhotonRoom.StateMatchmaking);
        if (state != RaidPhotonRoom.StateMatchmaking)
        {
            ShowMatchmaking("Debug Raid start unavailable", "Raid is already leaving matchmaking.", string.Empty);
            return;
        }

        List<Player> validPlayers = GetRaidPlayersWithClans();
        RaidPhotonRoom.ClanEntry[] clanEntries = BuildClanEntries(validPlayers);
        bool canDebugStart = validPlayers.Count > 0
            && validPlayers.Count <= RaidPhotonRoom.RoomCapacity
            && clanEntries.All(clan => clan.Count <= RaidPhotonRoom.MaxPlayersPerClan);

        if (!canDebugStart)
        {
            ShowMatchmaking("Debug Raid start unavailable", "No valid Raid players are ready yet.", "Wait for player clan data to sync, then try again.");
            return;
        }

        ShowMatchmaking("Debug starting Raid", "Starting without required player count.", $"{validPlayers.Count}/{RaidPhotonRoom.RequiredPlayers} players");
        StartLobbyCountdown(validPlayers, clanEntries);
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

    private static void SetText(TextMeshProUGUI textField, string text)
    {
        if (textField == null)
        {
            return;
        }

        textField.text = text;
    }

    private static void SetTextActive(TextMeshProUGUI textField, bool isActive)
    {
        if (textField == null)
        {
            return;
        }

        textField.gameObject.SetActive(isActive);
    }

    private void SetMatchmakingDotsActive(int visibleCount)
    {
        if (_matchmakingDots == null)
        {
            return;
        }

        for (int i = 0; i < _matchmakingDots.Length; i++)
        {
            GameObject dot = _matchmakingDots[i];
            if (dot == null)
            {
                continue;
            }

            bool isVisible = i < visibleCount;
            dot.SetActive(isVisible);

            if (isVisible && dot.transform is RectTransform dotTransform)
            {
                float startX = -matchmakingDotSpacing * (visibleCount - 1) * 0.5f;
                dotTransform.anchoredPosition = new Vector2(startX + matchmakingDotSpacing * i, 0f);
            }
        }
    }

    private void ResolveMatchmakingDotsFallback()
    {
        if (_matchmakingDots != null && _matchmakingDots.Length > 0)
        {
            return;
        }

        Transform dotRoot = _matchmakingPanel != null
            ? _matchmakingPanel.transform.Find("MatchmakingDots")
            : null;
        if (dotRoot == null)
        {
            return;
        }

        _matchmakingDots = new GameObject[dotRoot.childCount];
        for (int i = 0; i < dotRoot.childCount; i++)
        {
            _matchmakingDots[i] = dotRoot.GetChild(i).gameObject;
        }
    }

    private static void SetLocalizedText(TextMeshProUGUI textField, string finnishText, string englishText, params string[] additions)
    {
        if (textField == null)
        {
            return;
        }

        TextLanguageSelectorCaller selector = textField.GetComponent<TextLanguageSelectorCaller>();
        if (selector != null)
        {
            selector.SetText(GetCurrentLanguage(), additions ?? Array.Empty<string>());
            return;
        }

        string format = GetCurrentLanguage() == SettingsCarrier.LanguageType.English ? englishText : finnishText;
        textField.text = string.Format(format, additions ?? Array.Empty<string>());
    }

    private static SettingsCarrier.LanguageType GetCurrentLanguage()
    {
        SettingsCarrier.LanguageType language = SettingsCarrier.Instance != null
            ? SettingsCarrier.Instance.Language
            : SettingsCarrier.LanguageType.Finnish;

        return language == SettingsCarrier.LanguageType.English
            ? SettingsCarrier.LanguageType.English
            : SettingsCarrier.LanguageType.Finnish;
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
        Raid_References raidReferences = Raid_References.Instance;
        _inventoryHandler = raidReferences != null
            ? raidReferences.InventoryHandler
            : FindObjectOfType<Raid_InventoryHandler>();
        _inventoryPage = FindObjectOfType<Raid_InventoryPage>();
        _lootTracking = raidReferences != null
            ? raidReferences.LootTracking
            : FindObjectOfType<Raid_LootTracking>();
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
        ResetRaidStartCountdown();
        _inventoryInitialized = false;
        _gameplayReleased = false;
        _sharedRaidActive = false;
        _lobbyCountdownActive = false;
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
            return;
        }

    }
}
