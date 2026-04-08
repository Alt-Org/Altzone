
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using Prg.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class OnlinePlayersPanel : AltMonoBehaviour

{
    private enum OnlinePlayersView
    {
        Clan,
        All,
        Friends
    }

    [Header("Main panel page")]
    [SerializeField] public GameObject _onlinePlayersPanel;
    [SerializeField] private TextMeshProUGUI _onlineTitle;
    [Header("Global page")]
    [SerializeField] private GameObject _onlinePlayersPage;
    [SerializeField] private RectTransform _onlinePlayersPanelContent;
    [Header("Clan page")]
    [SerializeField] private GameObject _clanPlayersPage;
    [SerializeField] private RectTransform _clanPlayersPanelContent;
    [Header("Friend page")]
    [SerializeField] private GameObject _friendsPage;
    [SerializeField] private RectTransform _friendsContent;
    [Header("Panel prefabs")]
    [SerializeField] private OnlinePlayersPanelItem _onlinePlayersPanelItemPrefab;
    [SerializeField] private FriendlistItem _friendlistItemPrefab;
    [Header("Tab Buttons")]
    [SerializeField] private Button _viewClanPlayersButton;
    [SerializeField] private Button _viewAllPlayersButton;
    [SerializeField] private Button _viewFriendListButton;

    private OnlinePlayersView _currentView = OnlinePlayersView.Clan;

    private List<OnlinePlayersPanelItem> _onlinePlayersPanelItems = new List<OnlinePlayersPanelItem>();
    private List<OnlinePlayersPanelItem> _clanPlayersPanelItems = new List<OnlinePlayersPanelItem>();
    private List<OnlinePlayersPanelItem> _friendPanelItems = new List<OnlinePlayersPanelItem>();
    private List<ServerOnlinePlayer> _clanPlayers = new List<ServerOnlinePlayer>();
    private List<ServerOnlinePlayer> _allPlayers = new List<ServerOnlinePlayer>();
    private List<ServerFriendPlayer> _friendlist = new List<ServerFriendPlayer>();
    private List<ServerFriendRequest> _friendRequests = new List<ServerFriendRequest>();

    void Start()

    {
        _viewClanPlayersButton.onClick.AddListener(() => SetView(OnlinePlayersView.Clan));
        _viewAllPlayersButton.onClick.AddListener(() => SetView(OnlinePlayersView.All));
        _viewFriendListButton.onClick.AddListener(() => SetView(OnlinePlayersView.Friends));

        SetView(_currentView);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;
        OverlayPanelCheck.OnToggleOnlinePlayerList += ToggleOnlinePlayersPanel;
        OnlinePlayersPanelItem.OnContentRefreshRequested += RefreshListStatus;
        ToggleOnlinePlayersPanel(false);
    }

    private void OnEnable()
    {
        if (gameObject.activeInHierarchy) // Ensures the list is built only if the object is active
        {
            BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers);
        }
    }

    private void OnDestroy()
    {
        ServerManager.OnOnlinePlayersChanged -= BuildOnlinePlayerList;
        OverlayPanelCheck.OnToggleOnlinePlayerList -= ToggleOnlinePlayersPanel;
        OnlinePlayersPanelItem.OnContentRefreshRequested -= RefreshListStatus;
    }
    private bool _closing = false;

    private void LateUpdate()
    {
        if(ClickStateHandler.GetClickState() is ClickState.Start)
        {
            if (_onlinePlayersPanel.activeSelf)
            {
                if(!CheckIfPanel()) _closing = true;
            }
        }
        if(ClickStateHandler.GetClickState() is ClickState.End && _closing)
        {
            if (!CheckIfPanel()) Hide();
            _closing = false;
        }
    }

    private bool CheckIfPanel()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData data = new(EventSystem.current);
        data.position = ClickStateHandler.GetClickPosition();
        if (data.position == Vector2.negativeInfinity) return false;
        var modules = RaycasterManager.GetRaycasters();
        foreach (var module in modules)
        {
            module.Raycast(data, results);
        }
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == _onlinePlayersPanel) return true;
        }
        return false;
    }

    public void ToggleOnlinePlayersPanel(bool? value)
    {
        value ??= (!_onlinePlayersPanel.activeSelf);

        if ((bool)value)
        {
            OpenPanel();
        }
        else
        {
            Hide();
        }
    }

    private void SetView(OnlinePlayersView view)
    {
        _currentView = view;
        switch (_currentView)
        {
            case OnlinePlayersView.Clan:
                _clanPlayersPage.SetActive(true);
                _onlinePlayersPage.SetActive(false);
                _friendsPage.SetActive(false);
                break;
            case OnlinePlayersView.All:
                _clanPlayersPage.SetActive(false);
                _onlinePlayersPage.SetActive(true);
                _friendsPage.SetActive(false);
                break;
            case OnlinePlayersView.Friends:
                _clanPlayersPage.SetActive(false);
                _onlinePlayersPage.SetActive(false);
                _friendsPage.SetActive(true);
                break;
        }
    }


    private void BuildOnlinePlayerList(List<ServerOnlinePlayer> onlinePlayers)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(BuildOnlineList(onlinePlayers));
    }

    private IEnumerator FetchFriendData() // Fetches friend list and sent requests before building the player list
    {
        yield return ServerManager.Instance.GetFriendlist(list => _friendlist = list ?? new List<ServerFriendPlayer>());
        yield return ServerManager.Instance.GetFriendlistRequests(list => _friendRequests = list ?? new List<ServerFriendRequest>());
    }
    private IEnumerator BuildOnlineList(List<ServerOnlinePlayer> onlinePlayers)
    {
        yield return StartCoroutine(FetchFriendData());
        UpdateOnlineFriendsCount(onlinePlayers);

        List<OnlinePlayersPanelItem> _onlinePlayersPanelsToCheck = new(_onlinePlayersPanelItems);
        List<OnlinePlayersPanelItem> _onlinePlayersPanelsChecked = new();
        List<ServerOnlinePlayer> newOnlinePlayers = new List<ServerOnlinePlayer>();

        foreach (var player in onlinePlayers)
        {
            OnlinePlayersPanelItem panel = _onlinePlayersPanelsToCheck.Find(f => f.Player._id == player._id);
            if (panel)
            {
                _onlinePlayersPanelsChecked.Add(panel);
                _onlinePlayersPanelsToCheck.Remove(panel);
            }
            else
            {
                newOnlinePlayers.Add(player);
            }
        }

        foreach (var item in _onlinePlayersPanelsToCheck)
        {
            Destroy(item.gameObject);
        }

        _onlinePlayersPanelItems = _onlinePlayersPanelsChecked;


        foreach (var player in newOnlinePlayers)
        {
            string playerName = player.name;
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(player._id, c => serverPlayer = c));
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(() => serverPlayer != null || timeout);

            bool alreadyFriend = _friendlist.Exists(f => f._id == player._id);
            bool alreadyRequested = _friendRequests.Exists(r => r.friend._id == player._id);

            OnlinePlayersPanelItem newItem = Instantiate(_onlinePlayersPanelItemPrefab, _onlinePlayersPanelContent);
            StartCoroutine(newItem.Initialize(
                 player: serverPlayer,
                 onlineState: OnlineState.Global,
                 onRemoveClick: () => { },
                 friendstate: alreadyFriend ? FriendState.Friend : alreadyRequested ? FriendState.Sending : FriendState.None,
                 onAddFriendClick: () =>
                 {
                     StartCoroutine(ServerManager.Instance.SendFriendRequest(player._id, success =>
                     {
                         if (success)
                             Debug.Log($"Friend request sent to {playerName}");
                     }));

                 }
            ));
            _onlinePlayersPanelItems.Add(newItem);

        }

        ClanData data2 = null;
        Storefront.Get().GetClanData(ServerManager.Instance.Player.clan_id, c => data2 = c);

        foreach (var member in _clanPlayersPanelItems)
        {
            if (data2.Members.Exists(f => f._id == member.Player._id)) continue;
            Destroy(member.gameObject);
        }

        foreach (var member in data2.Members)
        {
            if (_clanPlayersPanelItems.Exists(f => f.Player._id == member._id)) continue;
            bool isOnline = onlinePlayers.Any(o => o._id == member._id);
            bool alreadyFriend = _friendlist.Exists(f => f._id == member._id);
            bool alreadyRequested = _friendRequests.Exists(r => r.friend._id == member._id);

            OnlinePlayersPanelItem clanItem = Instantiate(_onlinePlayersPanelItemPrefab, _clanPlayersPanelContent);
            StartCoroutine(clanItem.Initialize(
                player: member.Player,
                onlineState: isOnline ? OnlineState.Online : OnlineState.Offline,
                onRemoveClick: () => { },
                friendstate: alreadyFriend ? FriendState.Friend : alreadyRequested ? FriendState.Sending : FriendState.None,
                onAddFriendClick: () =>
                {
                    StartCoroutine(ServerManager.Instance.SendFriendRequest(member._id, success =>
                    {
                        if (success)
                            Debug.Log($"Friend request sent to {member.Name}");
                    }));

                }
                ));
            _clanPlayersPanelItems.Add(clanItem);
        }

        _clanPlayersPanelItems = _clanPlayersPanelItems.OrderBy(a => a.Player.name).ToList();
        _clanPlayersPanelItems = _clanPlayersPanelItems.OrderByDescending(a => a.IsOnline).ToList();

        for(int i =0; i<_clanPlayersPanelItems.Count ;i++)
        {
            _clanPlayersPanelItems[i].transform.SetSiblingIndex(i);
        }

        yield return UpdateFriendList();
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers)
    {
        int onlinePlayerCount = onlinePlayers.Count;

        _onlineTitle.text = $"Online-pelaajia {onlinePlayerCount}";
    }

    private IEnumerator UpdateFriendList()
    {

        List<OnlinePlayersPanelItem> _onlinePlayersPanelsToCheck = new(_friendPanelItems);
        List<OnlinePlayersPanelItem> _onlinePlayersPanelsChecked = new();
        List<ServerFriendRequest> newFriendRequests = new List<ServerFriendRequest>();
        foreach (ServerFriendRequest player in _friendRequests)
        {
            OnlinePlayersPanelItem panel = _onlinePlayersPanelsToCheck.Find(f => f.Player._id == player.friend._id);
            if (panel)
            {
                _onlinePlayersPanelsChecked.Add(panel);
                _onlinePlayersPanelsToCheck.Remove(panel);
            }
            else
            {
                newFriendRequests.Add(player);
            }
        }
        List<ServerFriendPlayer> newFriends = new List<ServerFriendPlayer>();
        foreach (ServerFriendPlayer player in _friendlist)
        {
            OnlinePlayersPanelItem panel = _onlinePlayersPanelsToCheck.Find(f => f.Player._id == player._id);
            if (panel)
            {
                _onlinePlayersPanelsChecked.Add(panel);
                _onlinePlayersPanelsToCheck.Remove(panel);
            }
            else
            {
                newFriends.Add(player);
            }
        }

        foreach (OnlinePlayersPanelItem player in _onlinePlayersPanelsToCheck)
        {
            Destroy(player.gameObject);
        }


        yield return new WaitForEndOfFrame();

        if (_friendRequests != null)
        {
            foreach (var request in newFriendRequests)
            {
                ServerPlayer serverPlayer = null;
                bool timeout = false;

                StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(request.friend._id, c => serverPlayer = c)); // Get friend data
                StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
                yield return new WaitUntil(() => serverPlayer != null || timeout);

                OnlinePlayersPanelItem requestItem = Instantiate(_onlinePlayersPanelItemPrefab, _friendsContent); // Instantiate a new UI item for each friend request

                bool isOnline = ServerManager.Instance.OnlinePlayers.Any(o => o._id == request.friend._id); //Check online status

                StartCoroutine(requestItem.Initialize(
                    player: serverPlayer,
                    onlineState: isOnline ? OnlineState.Online : OnlineState.Offline,
                    friendstate: FriendState.Receiving,
                    onAcceptClick: () =>
                    {
                        // Accept the request
                        StartCoroutine(ServerManager.Instance.FriendRequestAccept(
                            request.friend._id,
                            success =>
                            {
                                if (success)
                                    StartCoroutine(UpdateFriendList());
                            }
                        ));
                    },
                    onDeclineClick: () =>
                    {
                        // Decline the request
                        StartCoroutine(ServerManager.Instance.FriendDelete(
                            request.friend._id,
                            success =>
                            {
                                if (success)
                                    StartCoroutine(UpdateFriendList());
                            }
                        ));
                    }
                ));
                _friendPanelItems.Add(requestItem);
            }
        }

        foreach (var friend in newFriends)
        {
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            // Fetch friend profile from the server
            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(friend._id, c => serverPlayer = c)); // Get friend data
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(() => serverPlayer != null || timeout);

            bool isOnline = ServerManager.Instance.OnlinePlayers.Any(o => o._id == friend._id); //Check online status

            // Instantiate UI item for the friend
            OnlinePlayersPanelItem newItem = Instantiate(_onlinePlayersPanelItemPrefab, _friendsContent);
            StartCoroutine(newItem.Initialize(
                 player: serverPlayer,
                 onlineState: isOnline ? OnlineState.Online : OnlineState.Offline,
                 friendstate: FriendState.Friend,
                 onRemoveClick: () =>
                 {
                     // Remove friend 
                     StartCoroutine(ServerManager.Instance.FriendDelete(friend._id, success =>
                     {
                         if (success)
                             StartCoroutine(UpdateFriendList());
                     }));
                 }
            ));
            _friendPanelItems.Add(newItem);
        }
        _friendPanelItems = _friendPanelItems.OrderBy(a => a.Player.name).ToList();
        _friendPanelItems = _friendPanelItems.OrderByDescending(a => a.IsOnline).ToList();
        _friendPanelItems = _friendPanelItems.OrderByDescending(a => a.Friendstate == FriendState.Receiving).ToList();

        for (int i = 0; i < _clanPlayersPanelItems.Count; i++)
        {
            _clanPlayersPanelItems[i].transform.SetSiblingIndex(i);
        }

    }

    private void RefreshListStatus()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_clanPlayersPanelContent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_onlinePlayersPanelContent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_friendsContent);
    }

    public void OpenPanel()
    {
        _onlinePlayersPanel.SetActive(true);
    }

    public void Hide()
    {
        _onlinePlayersPanel.SetActive(false);
    }
}
