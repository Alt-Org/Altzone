
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using System.Threading;
using Assets.Altzone.Scripts.Model.Poco.Player;


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


    void Start()

    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;
        CloseFriendlist();

    }

    private void OnEnable()
    {
        BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers);
    }

    private void OnDestroy()
    {
        ServerManager.OnOnlinePlayersChanged -= BuildOnlinePlayerList;
    }

     public void OpenFriendlist()
    {
        _friendlistPanel.SetActive(true); 
    }

    public void CloseFriendlist()
    { 
        _friendlistPanel.SetActive(false);
    }

    private void BuildOnlinePlayerList(List<ServerOnlinePlayer> onlinePlayers)
    {
        StartCoroutine(BuildOnlineList(onlinePlayers));
    }

    private IEnumerator BuildOnlineList(List<ServerOnlinePlayer> onlinePlayers)
    {
        UpdateOnlineFriendsCount(onlinePlayers);

        foreach (var item in _friendlistItems)
        {
            Destroy(item.gameObject);
        }

        _friendlistItems.Clear();

        foreach (var player in onlinePlayers)
        {
            string playerName = player.name;
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(player._id, c => serverPlayer = c));
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(()=>serverPlayer != null || timeout);


            ClanLogo clanLogo = null;
            AvatarVisualData avatarVisualData = null;

            if (serverPlayer != null)
            {
                clanLogo = serverPlayer.clanLogo;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new AvatarData(serverPlayer.name,serverPlayer.avatar));
            }
         

            FriendlistItem newItem = Instantiate(_friendlistItemPrefab, _friendlistContent);
            newItem.Initialize(
                 playerName,
                 avatarVisualData: avatarVisualData,
                 clanLogo: clanLogo,
                 isOnline: true,
                 onRemoveClick: () => { }

                 

                 );
            _friendlistItems.Add( newItem );
        }
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers)
    {
        int onlinePlayerCount = onlinePlayers.Count;

        _friendlistOnlineTitle.text = $"Kavereita onlinessa {onlinePlayerCount}";
    }

}
