
using System;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [Header("Tabline")]
    [SerializeField] private Button _globalPlayersLeaderboardButton;
    [SerializeField] private Image _globalPlayersLeaderboardImage;
    [SerializeField] private Button _clanLeaderboardButton;
    [SerializeField] private Image _clanLeaderboardImage;
    [SerializeField] private Button _friendsLeaderboardButton;
    [SerializeField] private Image _friendsLeaderboardImage; 
    [SerializeField] private Image _tablineRibbon;
    [SerializeField] private TabLine _tablineScript;
    [SerializeField] private Button _globalClansLeaderboardButton; 
    [SerializeField] private Image _globalClansLeaderboardImage;

    [Header("Tab sprites")]
    [SerializeField] private Sprite _globalWinsSprite;
    [SerializeField] private Sprite _clanWinsSprite;
    [SerializeField] private Sprite _friendsWinsSprite;
   

    [Header("Leaderboard panels")]
    [SerializeField] private LeaderboardPodium _podium;
    [SerializeField] private GameObject _winsPanel;
    [SerializeField] private Transform _winsContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerWinsItemPrefab;
    [SerializeField] private GameObject _clanPointsItemPrefab;

    private enum Leaderboard
    {
        GlobalPlayers,
        GlobalClans,
        Clan,
        Friends 
    }


    private Leaderboard _currentLeaderboard = Leaderboard.GlobalClans;


    private void Start()
    {
        _globalPlayersLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.GlobalPlayers));
        _clanLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Clan));
        _friendsLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Friends));
        _globalClansLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.GlobalClans)); 

    }

    private void OnEnable()
    {
        OpenLeaderboard(Leaderboard.GlobalClans);
        _tablineScript.ActivateTabButton(0);

    }

    private void OpenLeaderboard(Leaderboard leaderboard)
    {
        _currentLeaderboard = leaderboard;

      LoadLeaderboard();
    }

    private void LoadLeaderboard()
    {
        foreach (Transform child in _winsContent) Destroy(child.gameObject);
        

        _podium.SetPlayerView();
      
        switch (_currentLeaderboard)
        {
            case Leaderboard.GlobalClans: 
                {
                    StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
                    {

                        _podium.SetClanView();

                        clanLeaderboard.Sort((a, b) => b.Points.CompareTo(a.Points)); 

                        int rank = 1;
                        foreach (ClanLeaderboard ranking in clanLeaderboard)
                        {
                            ClanData clanData = ranking.Clan;
                            ServerClan serverClan = ranking.ServerClan;


                            if (rank < 4) //The top three are displayed on the podium
                            {
                                _podium.InitilializePodium(rank, ranking.Clan.Name, ranking.Points, clanData, serverClan);
                            }
                            else
                            {
                                LeaderboardClanPointsItem item = Instantiate(_clanPointsItemPrefab, parent: _winsContent).GetComponent<LeaderboardClanPointsItem>();
                                item.Initialize(rank, ranking.Clan.Name, ranking.Points);

                                // Clan heart colors
                                ClanHeartColorSetter clanheart = item.GetComponentInChildren<ClanHeartColorSetter>();
                                clanheart.SetOtherClanColors(clanData);

                                // View clan button
                                item.OpenProfileButton.onClick.RemoveAllListeners();
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.ClanListing, serverClan); // Transfer the data for use in the leaderboard
                                });
                            }

                            rank++;
                        }
                    }));
                }
                break;
            case Leaderboard.GlobalPlayers:
                {

                    StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
                    {

                        playerLeaderboard.Sort((a, b) => b.WonBattles.CompareTo(a.WonBattles));

                        int rank = 1;
                        foreach (PlayerLeaderboard ranking in playerLeaderboard)
                        {
                            AvatarVisualData avatarVisualData = null;

                            if (ranking.Player.SelectedCharacterId != 0)
                            {
                                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(ranking.Player);
                            }


                            if (rank < 4) //The top three are displayed on the podium
                            {
                                _podium.InitilializePodium(rank, ranking);
                            }
                            else
                            {
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, ranking);



                                // View player profile button
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Player); // Transfer the data for use in the leaderboard
                                });
                            }

                            rank++;
                        }

                    }));
                }
                break;
            case Leaderboard.Clan:

                Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
                {

                        clanData.Members.Sort((a, b) => b.LeaderBoardWins.CompareTo(a.LeaderBoardWins));

                        int rank = 1;
                        foreach (ClanMember player in clanData.Members)
                        {
                            PlayerData playerData = player.GetPlayerData();
                            AvatarVisualData avatarVisualData = null;

                            if (playerData.SelectedCharacterId != 0)
                            {
                                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                            }


                            if (rank < 4)
                        {
                                _podium.InitilializePodium(rank, player.Name, player.LeaderBoardWins, playerData);
                            }
                            else
                            {
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, player.Name, player.LeaderBoardWins, avatarVisualData, ""); 

                                //View player profile button
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                                });
                            }

                            rank++;
                        }

                        // Add empty placements to fill out the space if necessary 
                        if (clanData.Members.Count < 15)
                        {
                            int placements = 15 - clanData.Members.Count;

                            for (int i = 0; i < placements; i++)
                            {
                                if (rank < 4)
                                {
                                    _podium.InitilializePodium(rank, "", 0, null);
                                }
                                else
                                {
                                    LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                    item.Initialize(rank, "", 0, null, "");
                                }

                                rank++;
                            }
                        }
                });
                break;
            case Leaderboard.Friends:
                // For Testing

                    for (int i = 1; i < 20; i++)
                    {
                        if (i < 4)
                        {
                            _podium.InitilializePodium(i, "", 0, null);
                    }
                        else
                        {
                            LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                            item.Initialize(i, "", 0, null, "");

                            // View player profile button
                            //item.OpenProfileButton.onClick.AddListener(() =>
                            //{
                            //    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                            //});
                        }
                    }
                break;
        }
    }
}
