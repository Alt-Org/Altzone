using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // lis‰tty
using Altzone.Scripts.Model.Poco.Clan; // tarvittava kirjasto serverin tietoja hakemista varten ?

public class FriendlistHandler : MonoBehaviour

{

    // UI-komponentit

    [SerializeField] private GameObject _friendlistPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _friendlistTitle;
    [SerializeField] private TMPro.TextMeshProUGUI _friendlistOnlineTitle;
    [SerializeField] private RectTransform _friendlistForeground;
    [SerializeField] private ScrollRect _friendlistScrollView;
    [SerializeField] private Button _closeFriendlistButton;
    [SerializeField] private Button _openFriendlistButton;
    [SerializeField] private FriendlistItem _friendlistItemPrefab;

    private List<FriendlistItem> _friendlistItems = new List<FriendlistItem>(); //  Lista, joka sis. instansioidut FriendlistItem-objektit


    // Start is called before the first frame update
    void Start()

    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList; // alustetaan lista

    }

    private void OnEnable()
    {
        BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers); // alustetaan ja p‰ivitet‰‰n
    }

    private void OnDestroy()
    {
        ServerManager.OnOnlinePlayersChanged -= BuildOnlinePlayerList;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OpenFriendlist()
    {
        _friendlistPanel.SetActive(true); //aktivoi
    }

    void CloseFriendlist()
    { 
        _friendlistPanel.SetActive(false); // piilottaa
    }

    private void BuildOnlinePlayerList(List<ServerOnlinePlayer> onlinePlayers)
    {
        UpdateOnlineFriendsCount(onlinePlayers);
       // UpdateFriendsCount(onlinePlayers);
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers)
    {
        int onlinePlayerCount = onlinePlayers.Count; //HUOM! kaikki online-pelaajat (myos sina itse), ei suodatettu viel‰ vain online-kavereita

        _friendlistOnlineTitle.text = $"Kavereita onlinessa {onlinePlayerCount}"; // Paivitetaan tieto otsikkoon
    }
    private void UpdateFriendsCount(List<ServerOnlinePlayer> onlinePlayers) //Paivittaa ystavalistan UI:n ja asettaa item-objektit paikoilleen (HUOM online-pelaajien perusteella)
    {

        foreach (var item in _friendlistItems)
        {
            Destroy(item.gameObject);
        }

        _friendlistItems.Clear();


        foreach (var player in onlinePlayers) // kaikki online-pelaajat
        {

            FriendlistItem newItem = Instantiate(_friendlistItemPrefab, _friendlistForeground);
            /* new.Initialize(
                 player.name,
                 player.avatar,
                 player.clanLogo,
                 player.isOnline,
                 () => RemoveFriend(player)
             );*/

            _friendlistItems.Add(newItem);
        }
    }

    /*private void RemoveFriend() // Ystavien poisto
    {

    }*/

}
