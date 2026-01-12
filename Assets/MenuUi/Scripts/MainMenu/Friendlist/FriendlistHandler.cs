
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.UI;
using static ServerManager;


public class FriendlistHandler : AltMonoBehaviour

{


    [SerializeField] private GameObject _friendlistPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _friendlistOnlineTitle;
    [SerializeField] private RectTransform _friendlistContent;
    [SerializeField] private ScrollRect _friendlistScrollView;
    [SerializeField] private Button _closeFriendlistButton;
    [SerializeField] private Button _openFriendlistButton;
    [SerializeField] private FriendlistItem _friendlistItemPrefab;

    private List<FriendlistItem> _friendlistItems = new List<FriendlistItem>();
    private List<ServerFriendRequest> _friendRequests = new List<ServerFriendRequest>();


    void Start()

    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

        ServerManager.OnOnlinePlayersChanged += BuildFriendlist;

        CloseFriendlist();

    }

    private void OnEnable()
    {
        StartCoroutine(BuildFriendlistCoroutine(ServerManager.Instance.OnlinePlayers));
    }

    private void OnDestroy()
    {
        ServerManager.OnOnlinePlayersChanged -= BuildFriendlist;
    }

     public void OpenFriendlist()
    {
        _friendlistPanel.SetActive(true); 
    }

    public void CloseFriendlist()
    { 
        _friendlistPanel.SetActive(false);
    }

    private void BuildFriendlist(List<ServerOnlinePlayer> onlinePlayers) // Rebuilds friendlist when online players update
    {
        StartCoroutine(BuildFriendlistCoroutine(onlinePlayers)); 
    }

    private IEnumerator BuildFriendlistCoroutine(List<ServerOnlinePlayer> onlinePlayers)
    {
        List<ServerFriendPlayer> friendList = null;
        yield return StartCoroutine(ServerManager.Instance.GetFriendlist(list => friendList = list)); //Fetch friend list from server

        List<ServerFriendRequest> friendRequests = null;
        yield return StartCoroutine(
            ServerManager.Instance.GetFriendlistRequests(list => friendRequests = list)
        );

        UpdateOnlineFriendsCount(onlinePlayers, friendList);

        foreach (var item in _friendlistItems)
        {
            Destroy(item.gameObject);
        }

        _friendlistItems.Clear();

        if (friendRequests != null)
        {
            foreach (var request in friendRequests)
            {
                FriendlistItem requestItem = Instantiate(_friendlistItemPrefab, _friendlistContent);

                requestItem.Initialize(
                    request.friend.name ?? request.friend._id,
                    isOnline: false,
                    onAcceptClick: () =>
                    {
                        StartCoroutine(ServerManager.Instance.FriendRequestAccept(
                            request.friend._id,
                            success =>
                            {
                                if (success)
                                    StartCoroutine(BuildFriendlistCoroutine(onlinePlayers));
                            }
                        ));
                    },
                    onDeclineClick: () =>
                    {
                        StartCoroutine(ServerManager.Instance.FriendDelete(
                            request.friend._id,
                            success =>
                            {
                                if (success)
                                    StartCoroutine(BuildFriendlistCoroutine(onlinePlayers));
                            }
                        ));
                    }
                );

                _friendlistItems.Add(requestItem);
            }
        }


        if (friendList == null || friendList.Count == 0)
            yield break;
        foreach (var friend in friendList)
        {
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(friend._id, c => serverPlayer = c)); // Get friend data
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(()=>serverPlayer != null || timeout);

            bool isOnline = onlinePlayers.Any(o => o._id == friend._id); //Check online status

            ClanLogo clanLogo = null;
            AvatarVisualData avatarVisualData = null;

            if (serverPlayer != null)
            {
                clanLogo = serverPlayer.clanLogo;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new AvatarData(serverPlayer.name,serverPlayer.avatar));
            }
         

            FriendlistItem newItem = Instantiate(_friendlistItemPrefab, _friendlistContent);
            newItem.Initialize(
                 serverPlayer?.name ?? friend._id,// Use name if available, otherwise show ID
                 avatarVisualData: avatarVisualData,
                 clanLogo: clanLogo,
                 isOnline: isOnline,
                 onRemoveClick: () => { }

                 

                 );
            _friendlistItems.Add( newItem );
        }
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers, List<ServerFriendPlayer> friendList)
    {
        if (friendList == null)
        {
            _friendlistOnlineTitle.text = $"Kavereita onlinessa 0";
            return;
        }

        int onlineFriendCount = friendList.Count(friend => onlinePlayers.Any(p => p._id == friend._id));

        _friendlistOnlineTitle.text = $"Kavereita onlinessa {onlineFriendCount}";
    }

}
