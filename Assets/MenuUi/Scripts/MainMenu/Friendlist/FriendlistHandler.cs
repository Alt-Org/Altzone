
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // lis�tty
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player; // tarvittava kirjasto serverin tietoja hakemista varten ?

public class FriendlistHandler : MonoBehaviour

{

    // UI-komponentit

    [SerializeField] private GameObject _friendlistPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _friendlistOnlineTitle;
    [SerializeField] private RectTransform _friendlistContent;
    [SerializeField] private ScrollRect _friendlistScrollView;
    [SerializeField] private Button _closeFriendlistButton;
    [SerializeField] private Button _openFriendlistButton;
    [SerializeField] private FriendlistItem _friendlistItemPrefab;

    private List<FriendlistItem> _friendlistItems = new List<FriendlistItem>();


    // Start is called before the first frame update
    void Start()

    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;

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
        UpdateOnlineFriendsCount(onlinePlayers);

        foreach (var item in _friendlistItems)
        {
            Destroy(item.gameObject);
        }

        _friendlistItems.Clear();

        foreach (var player in onlinePlayers)
        {
            string playerName = player.name;
            


            FriendlistItem newItem = Instantiate(_friendlistItemPrefab, _friendlistContent);
            newItem.Initialize(
                 playerName
                
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
