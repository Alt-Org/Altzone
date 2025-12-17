
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using System.Threading;
using Assets.Altzone.Scripts.Model.Poco.Player;


public class OnlinePlayersPanel : AltMonoBehaviour

{
    private enum OnlinePlayersView
    {
        Clan,
        All
    }
    

    [SerializeField] private GameObject _onlinePlayersPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _onlineTitle;
    [SerializeField] private RectTransform _onlinePlayersPanelContent;
    [SerializeField] private ScrollRect _onlinePlayersPanelScrollView;
    [SerializeField] private Button _closeOnlinePlayersPanelButton;
    [SerializeField] private Button _openOnlinePlayersPanelButton;
    [SerializeField] private OnlinePlayersPanelItem _onlinePlayersPanelItemPrefab;
    [SerializeField] private Button _viewClanPlayersButton;
    [SerializeField] private Button _viewAllPlayersButton;

    private OnlinePlayersView _currentView = OnlinePlayersView.Clan;

    private List<OnlinePlayersPanelItem> _onlinePlayersPanelItems = new List<OnlinePlayersPanelItem>();
    private List<ServerOnlinePlayer> _clanPlayers = new List<ServerOnlinePlayer>();
    private List<ServerOnlinePlayer> _allPlayers = new List<ServerOnlinePlayer>();


    void Start()

    {
        _openOnlinePlayersPanelButton.onClick.AddListener(OpenOnlinePlayersPanel);
        _closeOnlinePlayersPanelButton.onClick.AddListener(CloseOnlinePlayersPanel);
        _viewClanPlayersButton.onClick.AddListener(() => SetView(OnlinePlayersView.Clan));
        _viewAllPlayersButton.onClick.AddListener(() => SetView(OnlinePlayersView.All));

        SetView(_currentView);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;
        CloseOnlinePlayersPanel();
        
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
    }

    public void CloseOnlinePlayersPanel()
    {
        _onlinePlayersPanel.SetActive(false);
    }

    public void OpenOnlinePlayersPanel()
    {
        _onlinePlayersPanel.SetActive(true);
    }

    private void SetView(OnlinePlayersView view)
    {
        _currentView = view;
       UpdatePlayerList();
    }

    
    private void BuildOnlinePlayerList(List<ServerOnlinePlayer> onlinePlayers)
    {
        StartCoroutine(BuildOnlineList(onlinePlayers));
    }

    private IEnumerator BuildOnlineList(List<ServerOnlinePlayer> onlinePlayers)
    {
        UpdateOnlineFriendsCount(onlinePlayers);

        _clanPlayers.Clear();
        _allPlayers.Clear();


        foreach (var item in _onlinePlayersPanelItems)
        {
            Destroy(item.gameObject);
        }

        _onlinePlayersPanelItems.Clear();
        
        

        foreach (var player in onlinePlayers)
        {
            string playerName = player.name;
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(player._id, c => serverPlayer = c));
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(() => serverPlayer != null || timeout);


            ClanLogo clanLogo = null;
            AvatarVisualData avatarVisualData = null;

            if (serverPlayer != null)
            {
                _allPlayers.Add(player);

                if(serverPlayer.clan_id == ServerManager.Instance.Clan._id/*|| player._id == ServerManager.Instance.Player._id*/)
                {
                    _clanPlayers.Add(player);
                }
                clanLogo = serverPlayer.clanLogo;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new AvatarData(serverPlayer.name, serverPlayer.avatar));

            }

            bool hideLogo = _currentView == OnlinePlayersView.Clan; //Hide logo if we are in ClanView

            OnlinePlayersPanelItem newItem = Instantiate(_onlinePlayersPanelItemPrefab, _onlinePlayersPanelContent);
            newItem.Initialize(
                 playerName,
                 avatarVisualData: avatarVisualData,
                 clanLogo: hideLogo ? null : clanLogo, //Set logo to null in Clanview
                 isOnline: true,
                 onRemoveClick: () => { },
                 hideClanLogo: hideLogo // Use hideLogo to control logo visibility



                 );
            _onlinePlayersPanelItems.Add(newItem);
        }
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers)
    {
        int onlinePlayerCount = onlinePlayers.Count;

        _onlineTitle.text = $"Online-pelaajia {onlinePlayerCount}";
    }

    private void UpdatePlayerList()
    {
        List<ServerOnlinePlayer> playersToShow = _currentView == OnlinePlayersView.Clan  // Determine which players to show based on the current view (Clan or All)
            ? _clanPlayers
            : _allPlayers;

        for (int i = 0; i < _onlinePlayersPanelItems.Count; i++)    // Loop through the panel items
        {
            var item = _onlinePlayersPanelItems[i];
            if (i < playersToShow.Count) // If we are in Clan view, hide players that don't belong to the clan
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

}
