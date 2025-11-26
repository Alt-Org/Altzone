using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // lisätty
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

   



    // Start is called before the first frame update
    void Start()

    {
        _openFriendlistButton.onClick.AddListener(OpenFriendlist);
        _closeFriendlistButton.onClick.AddListener(CloseFriendlist);

        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList; // alustetaan lista



    }

    private void OnEnable()
    {
        BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers); // alustetaan ja päivitetään
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
    }

    private void UpdateOnlineFriendsCount(List<ServerOnlinePlayer> onlinePlayers)
    {
        int onlinePlayerCount = onlinePlayers.Count; //HUOM! kaikki online-pelaajat (myös sinä itse), ei suodatettu vielä vain online-kavereita

        _friendlistOnlineTitle.text = $"Kavereita onlinessa {onlinePlayerCount}"; // Päivitetään tieto otsikkoon
    }

}
