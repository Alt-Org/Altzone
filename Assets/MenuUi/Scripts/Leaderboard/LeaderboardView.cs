
using System;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;

public class LeaderboardView : MonoBehaviour
{
    [Header("Tabline")]
    [SerializeField] private Button _globalPlayersLeaderboardButton;
    //[SerializeField] private Image _globalPlayersLeaderboardImage;
    [SerializeField] private Button _friendsLeaderboardButton;
    //[SerializeField] private Image _friendsLeaderboardImage; 
    //[SerializeField] private Image _tablineRibbon;
    [SerializeField] private TabLine _tablineScript;
    [SerializeField] private Button _globalClansLeaderboardButton; 
    //[SerializeField] private Image _globalClansLeaderboardImage;

    /*
    [Header("Tab sprites")]
    [SerializeField] private Sprite _globalWinsSprite;
    [SerializeField] private Sprite _clanWinsSprite;
    [SerializeField] private Sprite _friendsWinsSprite;
    */

    [Header("Leaderboard panels")]
    [SerializeField] private LeaderboardPodium _podium;
    //[SerializeField] private GameObject _winsPanel;
    //[SerializeField] private Transform _winsContent;
    [SerializeField] private Transform _clansContent;
    [SerializeField] private Transform _playersContent;
    [SerializeField] private Transform _friendsContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerWinsItemPrefab;
    [SerializeField] private GameObject _clanPointsItemPrefab;
    [SerializeField] private GameObject _friendsWinsItemPrefab;

    [Header("Scroll to top and to player")]
    [SerializeField] private Button _scrollToTopButton;
    private ScrollRect _scrollRect;
    private RectTransform _contentPanel;

    [SerializeField] private List<ScrollRect> _scrollRectScrollToMeList = new List<ScrollRect>();
    [SerializeField] private List<RectTransform> _contentPanelScrollToMeList = new List<RectTransform>();

    [SerializeField] private Button _scrollToMeButton1;
    [SerializeField] private Button _scrollToMeButton2;
    [SerializeField] private Button _scrollToMeButton3;
    private RectTransform _playerItem;

    //friend leaderboard
    private List<FriendPlayer> _friendsPlayersList = new List<FriendPlayer>();
    private FriendPlayer player = new FriendPlayer();
    //for testing
    private FriendPlayer friendtest1 = new FriendPlayer();
    private FriendPlayer friendtest2 = new FriendPlayer();
    private FriendPlayer friendtest3 = new FriendPlayer();
    private FriendPlayer friendtest4 = new FriendPlayer();


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
        _friendsLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Friends));
        _globalClansLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.GlobalClans));
        _scrollToTopButton.onClick.AddListener(() => ScrollToTop(_scrollRect));
        _scrollToMeButton1.onClick.AddListener(() => ScrollToMe(_scrollRect, _contentPanel));
        _scrollToMeButton2.onClick.AddListener(() => ScrollToMe(_scrollRect, _contentPanel));
        _scrollToMeButton3.onClick.AddListener(() => ScrollToMe(_scrollRect, _contentPanel));
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
        foreach (Transform child in _clansContent) Destroy(child.gameObject);
        foreach (Transform child in _playersContent) Destroy(child.gameObject);
        foreach (Transform child in _friendsContent) Destroy(child.gameObject);

        _podium.SetPlayerView();

        switch (_currentLeaderboard)
        {
            case Leaderboard.GlobalClans: //tab 1
                {
                    //apply correct list for scroll to me
                    _scrollRect = _scrollRectScrollToMeList[0];
                    _contentPanel = _contentPanelScrollToMeList[0];
                    _scrollToTopButton.onClick.Invoke();

                    StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
                    {
                        _podium.SetClanView();

                        clanLeaderboard.Sort((a, b) => b.Points.CompareTo(a.Points));

                        int rank = 1;
                        foreach (ClanLeaderboard ranking in clanLeaderboard)
                        {
                            ClanData clanData = ranking.Clan;
                            ServerClan serverClan = ranking.ServerClan;

                            if (rank == 1) //The top three are displayed on the podium
                            {
                                //apply all info to the podium
                                _podium.InitilializePodium(rank, ranking.Clan.Name, ranking.Points, clanData, serverClan);

                                if (clanLeaderboard.Count > 1) // make sure that list has enough members
                                    { _podium.InitilializePodium(2, clanLeaderboard[1].Clan.Name, clanLeaderboard[1].Points, clanLeaderboard[1].Clan, clanLeaderboard[1].ServerClan); }
                                else
                                    { _podium.InitilializePodium(2, null, 0, clanData, null); }

                                if (clanLeaderboard.Count > 2)
                                    { _podium.InitilializePodium(3, clanLeaderboard[2].Clan.Name, clanLeaderboard[2].Points, clanLeaderboard[2].Clan, clanLeaderboard[2].ServerClan); }
                                else
                                    { _podium.InitilializePodium(3, null, 0, clanData, null); }

                                //add podium to view
                                LeaderboardPodium item = Instantiate(_podium, parent: _clansContent).GetComponent<LeaderboardPodium>(); 

                                // View clan profile -buttons
                                item.FirstOpenClanProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.ClanListing, serverClan); // Transfer the data for use in the leaderboard 1st place
                                });
                                item.SecondOpenClanProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.ClanListing, clanLeaderboard[1].ServerClan); // Transfer the data for use in the leaderboard 2nd place
                                });
                                item.ThirdOpenClanProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.ClanListing, clanLeaderboard[2].ServerClan); // Transfer the data for use in the leaderboard 3rd place
                                });
                            }
                            else if(rank > 3)
                            {
                                LeaderboardClanPointsItem item = Instantiate(_clanPointsItemPrefab, parent: _clansContent).GetComponent<LeaderboardClanPointsItem>();
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

                                if (ranking.Clan.Name == ServerManager.Instance.Clan.name.ToString()) //make the player's placement item white
                                {
                                    item.RecolorBackground();
                                    _playerItem = item.GetComponent<RectTransform>();
                                }
                            }
                            rank++;
                        }
                    }));
                }
                break;
            case Leaderboard.GlobalPlayers: //tab 2
                {
                    //apply correct list for scroll to me
                    _scrollRect = _scrollRectScrollToMeList[1];
                    _contentPanel = _contentPanelScrollToMeList[1];
                    _scrollToTopButton.onClick.Invoke();

                    StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
                    {
                        playerLeaderboard.Sort((a, b) => b.WonBattles.CompareTo(a.WonBattles));

                        int rank = 1;
                        foreach (PlayerLeaderboard ranking in playerLeaderboard)
                        {
                            //AvatarVisualData avatarVisualData = null;

                            //if (ranking.Player.SelectedCharacterId != 0)
                            //{
                            //avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(ranking.Player);
                            //}

                            if (rank == 1) //The top three are displayed on the podium
                            {
                                // add podium to view
                                LeaderboardPodium item = Instantiate(_podium, parent: _playersContent).GetComponent<LeaderboardPodium>();

                                //apply all info to the podium
                                item.InitilializePodium(rank, ranking);

                                if (playerLeaderboard.Count > 1) // make sure that list has enough members
                                { item.InitilializePodium(2, playerLeaderboard[1]); }
                                else
                                { item.InitilializePodium(2, null); }

                                if (playerLeaderboard.Count > 2)
                                { item.InitilializePodium(3, playerLeaderboard[2]); }
                                else
                                { item.InitilializePodium(3, null); }

                                // View player profile -buttons
                                item.FirstOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Player); // Transfer the data for use in the leaderboard 1st place
                                });

                                item.SecondOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerLeaderboard[1].Player); // Transfer the data for use in the leaderboard 2nd place
                                });

                                item.ThirdOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerLeaderboard[2].Player); // Transfer the data for use in the leaderboard 3rd place
                                });

                            }
                            else if (rank > 3) 
                            {
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _playersContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, ranking);

                                // View player profile button
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Player); // Transfer the data for use in the leaderboard
                                });

                                if (ranking.Player.Name == ServerManager.Instance.Player.name.ToString()) //make the player's placement item white
                                {
                                    item.RecolorBackground();
                                    _playerItem = item.GetComponent<RectTransform>();
                                }

                            }
                            rank++;
                        }
                    }));
                }
                break;
            case Leaderboard.Clan:

                /*
                Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
                {

                    clanData.Members.Sort((a, b) => b.LeaderBoardWins.CompareTo(a.LeaderBoardWins));

                    int rank = 1;
                    foreach (ClanMember player in clanData.Members)
                    {
                        PlayerData playerData = player.GetPlayerData();
                        AvatarVisualData avatarVisualData = null;

                        //if (playerData.SelectedCharacterId != 0)
                        //{
                        avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(playerData);
                        //}


                        if (rank < 4)
                        {
                            _podium.InitilializePodium(rank, player.Name, player.LeaderBoardWins, playerData);

                            if (rank == 1) //add the podium to scroll view
                            {
                                LeaderboardPodium item = Instantiate(_podium, parent: _winsContent).GetComponent<LeaderboardPodium>();
                            }
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

                                if (rank == 1)
                                {
                                    LeaderboardPodium item = Instantiate(_podium, parent: _winsContent).GetComponent<LeaderboardPodium>();
                                }
                            }
                            else
                            {
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, "", 0, null, "");
                            }

                            rank++;
                        }
                    }
                });*/
                break;
            case Leaderboard.Friends: //tab 3

                //apply correct list for scroll to me and to top
                _scrollRect = _scrollRectScrollToMeList[2];
                _contentPanel = _contentPanelScrollToMeList[2];
                _scrollToTopButton.onClick.Invoke();

                _friendsPlayersList.Clear();

                //test friends to fill the leaderboard
                _friendsPlayersList.Add(friendtest1);
                _friendsPlayersList.Add(friendtest2);
                _friendsPlayersList.Add(friendtest3);
                _friendsPlayersList.Add(friendtest4);
                //
                friendtest1.name = "friendtest1";
                friendtest2.name = "friendtest2";
                friendtest3.name = "friendtest3";
                friendtest4.name = "friendtest4";

                foreach (FriendPlayer friend in OnlinePlayersPanel.Instance.Friendlist) //add friends to list
                {
                    _friendsPlayersList.Add(friend);
                    Debug.Log("Adding friends from friendlist... name: " + friend.name);
                }

                //add the player to the leaderboard
                _friendsPlayersList.Add(player);
                player.name = ServerManager.Instance.Player.name;
                player.clan_id = ServerManager.Instance.Player.clan_id;
                player._id = ServerManager.Instance.Player._id;
             
                //_friendsPlayersList.Sort((a, b) => b.wins.CompareTo(a.wins)); //sort by wins, not done currently since wins do not exist for friends

                int rank = 1;
                foreach (FriendPlayer friend in _friendsPlayersList)  //add friends to leaderboard
                {
                    if (rank == 1)
                    {
                        AvatarVisualData avatarVisualData = null;
                        avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(friend.avatar);

                        //add the podium to the scroll view
                        LeaderboardPodium itemPod = Instantiate(_podium, parent: _friendsContent).GetComponent<LeaderboardPodium>();

                        //apply all info to the podium
                        itemPod.InitilializePodium(rank, friend.name, 0, null);

                        if (_friendsPlayersList.Count > 1) // make sure that list has enough members
                        { itemPod.InitilializePodium(2, _friendsPlayersList[1].name, 0, null); }
                        else
                        { itemPod.InitilializePodium(2, null, 0, null); }

                        if (_friendsPlayersList.Count > 2)
                        { itemPod.InitilializePodium(3, _friendsPlayersList[2].name, 0, null); }
                        else
                        { itemPod.InitilializePodium(3, null, 0, null); }

                        // View player profile -buttons
                        itemPod.FirstOpenPlayerProfileButton.onClick.AddListener(() =>
                        {
                            DataCarrier.AddData(DataCarrier.PlayerProfile, friend._id); // Transfer the data for use in the leaderboard 1st place
                        });
                        if (_friendsPlayersList.Count > 1)
                        {
                            itemPod.SecondOpenPlayerProfileButton.onClick.AddListener(() =>
                            {
                                DataCarrier.AddData(DataCarrier.PlayerProfile, _friendsPlayersList[1]._id); // Transfer the data for use in the leaderboard 2nd place
                            });
                        }
                        if (_friendsPlayersList.Count > 2)
                        {
                            itemPod.ThirdOpenPlayerProfileButton.onClick.AddListener(() =>
                            {
                                DataCarrier.AddData(DataCarrier.PlayerProfile, _friendsPlayersList[2]._id); // Transfer the data for use in the leaderboard 3rd place
                            });
                        }
                    }
                    else if( rank > 3)
                    {

                        AvatarVisualData avatarVisualData = null;
                        avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(friend.avatar);
                        
                        LeaderboardWinsItem item = Instantiate(_friendsWinsItemPrefab, parent: _friendsContent).GetComponent<LeaderboardWinsItem>();

                        item.Initialize(rank, friend.name, 0, avatarVisualData, friend.clan_id);

                        //open friend profile                  
                        item.OpenProfileButton.onClick.AddListener(() =>
                        {
                            DataCarrier.AddData(DataCarrier.PlayerProfile, friend._id); // Transfer the data for use in the leaderboard
                        });

                        if (friend.name == ServerManager.Instance.Player.name.ToString()) //color player's item white
                        {
                            item.RecolorBackground();
                            _playerItem = item.GetComponent<RectTransform>();
                        }
                    }
                    rank++;
                }

                if (_friendsPlayersList.Count < 20) //add empty items for testing
                {

                    for (int i = _friendsPlayersList.Count + 1; i <= 20; i++)
                    {
                        LeaderboardWinsItem item = Instantiate(_friendsWinsItemPrefab, parent: _friendsContent).GetComponent<LeaderboardWinsItem>();

                        item.Initialize(i, "", 0, null, "");
                    }
                }

                // For Testing

                //for (int i = 1; i < 20; i++)
                //{
                //    if (i == 1) //add the podium to scroll view
                //    {
                //        LeaderboardPodium itemPod = Instantiate(_podium, parent: _friendsContent).GetComponent<LeaderboardPodium>();
                //    }

                //    if (i < 4)
                //    {
                //        _podium.InitilializePodium(i, "", 0, null);
                //    }
                //    else
                //    {
                //        LeaderboardWinsItem item = Instantiate(_friendsWinsItemPrefab, parent: _friendsContent).GetComponent<LeaderboardWinsItem>();

                //        item.Initialize(i, "", 0, null, "");

                //        /*
                //        if (ranking.Player.Name == ServerManager.Instance.Player.name.ToString())
                //        {
                //            item.RecolorBackground();
                //            _playerItem = item.GetComponent<RectTransform>();
                //        }*/

                //        // View player profile button
                //        //item.OpenProfileButton.onClick.AddListener(() =>
                //        //{
                //        //    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                //        //});

                //    }
                //}
                break;
        }
    }

    private void Update()
    {
        if (_scrollRect != null)
        {
            if (_scrollRect.verticalNormalizedPosition > 0.99)
            { _scrollToTopButton.gameObject.SetActive(false); }
            else
            { _scrollToTopButton.gameObject.SetActive(true); }
        }
    }

    private void ScrollToTop(ScrollRect _scrollRect)
    {
        _scrollRect.verticalNormalizedPosition = 1;
    }

    private void ScrollToMe(ScrollRect _scrollRect, Transform _contentPanel)
    {
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaa");

        if (_playerItem != null)
        {
            Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaa");

            RectTransform target = _playerItem;

            Vector2 listPostition = _scrollRect.viewport.localPosition;
            Vector2 targetPostition = target.localPosition;

            Vector2 newPosition = new Vector2(0,
                0 - (listPostition.y + targetPostition.y));

            _contentPanel.localPosition = newPosition;
        }

        _scrollToTopButton.gameObject.SetActive(false);

    }

}
