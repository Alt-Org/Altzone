
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

    // UI-komponentit

    [SerializeField] private GameObject _onlinePlayersPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _onlineTitle;
    [SerializeField] private RectTransform _onlinePlayersPanelContent;
    [SerializeField] private ScrollRect _onlinePlayersPanelScrollView;
    [SerializeField] private Button _closeOnlinePlayersPanelButton;
    [SerializeField] private Button _openOnlinePlayersPanelButton;
    [SerializeField] private OnlinePlayersPanelItem _onlinePlayersPanelItemPrefab;
    [SerializeField] private Button _viewClanPlayersButton;
    [SerializeField] private Button _viewAllPlayersButton;

    private bool _isClanPlayersView = true;

    private List<OnlinePlayersPanelItem> _onlinePlayersPanelItems = new List<OnlinePlayersPanelItem>();
   
    private List<ServerOnlinePlayer> _clanPlayers = new List<ServerOnlinePlayer>();
    private List<ServerOnlinePlayer> _allPlayers = new List<ServerOnlinePlayer>();


    // Start is called before the first frame update
    void Start()

    {
        _openOnlinePlayersPanelButton.onClick.AddListener(OpenOnlinePlayersPanel);
        _closeOnlinePlayersPanelButton.onClick.AddListener(CloseOnlinePlayersPanel);
        _viewClanPlayersButton.onClick.AddListener(ViewClanPlayers);
        _viewAllPlayersButton.onClick.AddListener(ViewAllPlayers);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;
        CloseOnlinePlayersPanel();
        
    }

    private void OnEnable()
    {
        BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers);
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

    private void ViewClanPlayers()
    {
        _isClanPlayersView = true;
       UpdatePlayerList();
    }

    private void ViewAllPlayers()
    {
        _isClanPlayersView = false;
       UpdatePlayerList();
    }
    
    private void BuildOnlinePlayerList(List<ServerOnlinePlayer> onlinePlayers)
    {
        StartCoroutine(BuildOnlineList(onlinePlayers));
    }

    private IEnumerator BuildOnlineList(List<ServerOnlinePlayer> onlinePlayers)
    {
        UpdateOnlineFriendsCount(onlinePlayers);


        foreach (var item in _onlinePlayersPanelItems)
        {
            Destroy(item.gameObject);
        }

        _onlinePlayersPanelItems.Clear();
        
        _clanPlayers.Clear();
        _allPlayers.Clear();
        

        foreach (var player in onlinePlayers)
        {
            string playerName = player.name;
            ServerPlayer serverPlayer = null;
            bool timeout = false;

            StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(player._id, c => serverPlayer = c));
            StartCoroutine(WaitUntilTimeout(3, c => timeout = c));
            yield return new WaitUntil(() => serverPlayer != null || timeout);
            
            if (serverPlayer != null && serverPlayer.clanLogo != null)
            {
                _clanPlayers.Add(player); // Pelaaja on klaanin jäsen
            }
            else
            {
                _allPlayers.Add(player); // Pelaaja ei ole klaanissa
            } 
            

            ClanLogo clanLogo = null;
            AvatarVisualData avatarVisualData = null;

            if (serverPlayer != null)
            {
                clanLogo = serverPlayer.clanLogo;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(new AvatarData(serverPlayer.name, serverPlayer.avatar));
            }


            OnlinePlayersPanelItem newItem = Instantiate(_onlinePlayersPanelItemPrefab, _onlinePlayersPanelContent);
            newItem.Initialize(
                 playerName,
                 avatarVisualData: avatarVisualData,
                 clanLogo: clanLogo,
                 isOnline: true,
                 onRemoveClick: () => { }



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

    }

}
