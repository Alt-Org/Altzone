using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Window;
using System.Collections;
using System;

public class LeaderboardView : MonoBehaviour
{
    [Header("Tabline")]
    [SerializeField] private Button _globalPlayersLeaderboardButton;
    [SerializeField] private Button _friendsLeaderboardButton;
    [SerializeField] private TabLine _tablineScript;
    [SerializeField] private Button _globalClansLeaderboardButton; 


    [Header("Leaderboard panels")]
    [SerializeField] private LeaderboardPodium _podium;
    [SerializeField] private Transform _clansContent;
    [SerializeField] private Transform _playersContent;
    [SerializeField] private Transform _friendsContent;
    [SerializeField] private GameObject _loadingImage;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerWinsItemPrefab;
    [SerializeField] private GameObject _clanPointsItemPrefab;
    [SerializeField] private GameObject _friendsWinsItemPrefab;

    [Header("Scroll to top and to player")]
    [SerializeField] private Button _scrollToTopButton;
    private ScrollRect _currentScrollRect;
    private RectTransform _currentContentPanel;

    [SerializeField] private ScrollRect _clansScrollRect;
    [SerializeField] private ScrollRect _playersScrollRect;
    [SerializeField] private ScrollRect _friendsScrollRect;

    [SerializeField] private Button _scrollToMeButton1;
    [SerializeField] private Button _scrollToMeButton2;
    [SerializeField] private Button _scrollToMeButton3;
    private RectTransform _playerItem;

    private string _playerName;
    private string _playerClanName;

    //friend leaderboard
    private List<FriendPlayer> _friendsPlayersList = new List<FriendPlayer>();
    //for testing
    //private FriendPlayer friendtest1 = new FriendPlayer();

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
        _scrollToTopButton.onClick.AddListener(() => ScrollToTop(_currentScrollRect));
        _scrollToMeButton1.onClick.AddListener(() => ScrollToMe(_currentScrollRect, _currentContentPanel));
        _scrollToMeButton2.onClick.AddListener(() => ScrollToMe(_currentScrollRect, _currentContentPanel));
        _scrollToMeButton3.onClick.AddListener(() => ScrollToMe(_currentScrollRect, _currentContentPanel));

        _playerName = ServerManager.Instance.Player.name.ToString();
        _playerClanName = ServerManager.Instance.Clan.name.ToString();
        _loadingImage.SetActive(false);
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

        _playerItem = null;

        _podium.SetPlayerView();

        switch (_currentLeaderboard)
        {
            case Leaderboard.GlobalClans: //tab 1
                {
                    //apply correct list for scroll to me
                    _currentScrollRect = _clansScrollRect;
                    _currentContentPanel = (RectTransform)_clansContent;
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
                                //add podium to view
                                LeaderboardPodium item = Instantiate(_podium, parent: _clansContent).GetComponent<LeaderboardPodium>();

                                if (ranking.Clan.Name == _playerClanName)
                                { _playerItem = item.GetComponent<RectTransform>(); }

                                //apply all info to the podium

                                _podium.InitilializePodiumClan(rank, ranking.Clan.Name, ranking.Points, clanData, serverClan);

                                if (clanLeaderboard.Count > 1) // make sure that list has enough members
                                    { _podium.InitilializePodiumClan(2, clanLeaderboard[1].Clan.Name, clanLeaderboard[1].Points, clanLeaderboard[1].Clan, clanLeaderboard[1].ServerClan);
                                    if (clanLeaderboard[1].Clan.Name == _playerClanName)
                                        { _playerItem = item.GetComponent<RectTransform>(); }
                                }
                                else
                                    { _podium.InitilializePodiumClan(2, null, 0, clanData, null); }

                                if (clanLeaderboard.Count > 2)
                                    { _podium.InitilializePodiumClan(3, clanLeaderboard[2].Clan.Name, clanLeaderboard[2].Points, clanLeaderboard[2].Clan, clanLeaderboard[2].ServerClan);
                                    if (clanLeaderboard[2].Clan.Name == _playerClanName)
                                        { _playerItem = item.GetComponent<RectTransform>(); }
                                }
                                else
                                    { _podium.InitilializePodiumClan(3, null, 0, clanData, null); }


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

                                if (ranking.Clan.Name == _playerClanName) //make the player's placement item white
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
                    _currentScrollRect = _playersScrollRect;
                    _currentContentPanel = (RectTransform)_playersContent;
                    _scrollToTopButton.onClick.Invoke();

                    StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
                    {
                        playerLeaderboard.Sort((a, b) => b.WonBattles.CompareTo(a.WonBattles));

                        int rank = 1;
                        foreach (PlayerLeaderboard ranking in playerLeaderboard)
                        {
                            if (rank == 1) //The top three are displayed on the podium
                            {
                                // add podium to view
                                LeaderboardPodium item = Instantiate(_podium, parent: _playersContent).GetComponent<LeaderboardPodium>();

                                //apply all info to the podium
                                item.InitilializePodium(rank, ranking);

                                if (ranking.Player.Name == _playerName)
                                { _playerItem = item.GetComponent<RectTransform>(); }

                                if (playerLeaderboard.Count > 1) // make sure that list has enough members
                                {
                                    item.InitilializePodium(2, playerLeaderboard[1]);

                                    if (playerLeaderboard[1].Player.Name == _playerName)
                                    { _playerItem = item.GetComponent<RectTransform>(); }
                                }
                                else
                                { item.InitilializePodium(2, null); }

                                if (playerLeaderboard.Count > 2)
                                {
                                    item.InitilializePodium(3, playerLeaderboard[2]);

                                    if (playerLeaderboard[1].Player.Name == _playerName)
                                    { _playerItem = item.GetComponent<RectTransform>(); }
                                }
                                else
                                { item.InitilializePodium(3, null); }

                                // View player profile -buttons
                                item.FirstOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Player); // Transfer the data for use in the leaderboard 1st place
                                    StartCoroutine(item.FirstOpenPlayerProfileButton.GetComponent<WindowNavigation>().Navigate());
                                });

                                item.SecondOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerLeaderboard[1].Player); // Transfer the data for use in the leaderboard 2nd place
                                    StartCoroutine(item.SecondOpenPlayerProfileButton.GetComponent<WindowNavigation>().Navigate());
                                });

                                item.ThirdOpenPlayerProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerLeaderboard[2].Player); // Transfer the data for use in the leaderboard 3rd place
                                    StartCoroutine(item.ThirdOpenPlayerProfileButton.GetComponent<WindowNavigation>().Navigate());
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

                                if (ranking.Player.Name == _playerName) //make the player's placement item white
                                {
                                    item.RecolorBackground();
                                    _playerItem = item.GetComponent<RectTransform>();
                                }
                            }
                            rank++;
                            StartCoroutine(FrameChecker());
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

                 FriendPlayer _player = new FriendPlayer();

                //apply correct list for scroll to me and to top
                _currentScrollRect = _friendsScrollRect;
                _currentContentPanel = (RectTransform)_friendsContent;
                _scrollToTopButton.onClick.Invoke();

                _friendsPlayersList.Clear();

                //test friends to fill the leaderboard             
                //_friendsPlayersList.Add(friendtest1);
                //.name = "friendtest1";


                foreach (FriendPlayer friend in OnlinePlayersPanel.Instance.Friendlist) //add friends to list
                {
                    _friendsPlayersList.Add(friend);
                    Debug.Log("Adding friends from friendlist... name: " + friend.name);
                }

                //add the player to the leaderboard
                _friendsPlayersList.Add(_player);
                _player.name = ServerManager.Instance.Player.name;
                _player.clan_id = ServerManager.Instance.Player.clan_id;
                _player._id = ServerManager.Instance.Player._id;

                //_friendsPlayersList.Sort((a, b) => b.wins.CompareTo(a.wins)); //sort by wins, not done currently since wins do not exist for friends
                StartCoroutine(FriendLeaderboardList());

                break;
        }
    }

    private IEnumerator FriendLeaderboardList()
    {
        int rank = 1;
        foreach (FriendPlayer friend in _friendsPlayersList)  //add friends to leaderboard
        {
            if (rank == 1)
            {
                //add the podium to the scroll view
                LeaderboardPodium itemPod = Instantiate(_podium, parent: _friendsContent).GetComponent<LeaderboardPodium>();

                AvatarVisualData avatarVisualData1 = null;
                AvatarVisualData avatarVisualData2 = null;
                AvatarVisualData avatarVisualData3 = null;

                avatarVisualData1 = AvatarDesignLoader.Instance.CreateAvatarVisualData(friend.avatar);
                if (_friendsPlayersList.Count > 1)
                    avatarVisualData2 = AvatarDesignLoader.Instance.CreateAvatarVisualData(_friendsPlayersList[1].avatar);
                
                if (_friendsPlayersList.Count > 2)
                    avatarVisualData3 = AvatarDesignLoader.Instance.CreateAvatarVisualData(_friendsPlayersList[2].avatar);

                //apply all info to the podium

                //FIRST
                yield return new WaitForSeconds(0.01f); //wait so that the avatars have time to load
                
                itemPod.InitilializePodium(rank, friend.name, 0, null, avatarVisualData1);

                if (friend.name == _playerName)
                    _playerItem = itemPod.GetComponent<RectTransform>(); 

                //SECOND
                if (_friendsPlayersList.Count > 1) // make sure that list has enough members
                {                 
                    itemPod.InitilializePodium(2, _friendsPlayersList[1].name, 0, null, avatarVisualData2);

                    if (_friendsPlayersList[1].name == _playerName)
                        _playerItem = itemPod.GetComponent<RectTransform>(); 
                }
                else
                { itemPod.InitilializePodium(2, null, 0, null, null); }

                //THIRD
                if (_friendsPlayersList.Count > 2)
                {
                    itemPod.InitilializePodium(3, _friendsPlayersList[2].name, 0, null, avatarVisualData3);

                    if (_friendsPlayersList[2].name == _playerName)
                        _playerItem = itemPod.GetComponent<RectTransform>();
                }
                else
                { itemPod.InitilializePodium(3, null, 0, null, null); }

                // View player profile -buttons
                itemPod.FirstOpenPlayerProfileButton.onClick.AddListener(() =>
                {
                    StartCoroutine(GetFriendProfile(friend._id, null, itemPod.FirstOpenPlayerProfileButton));
                });
                if (_friendsPlayersList.Count > 1)
                {
                    itemPod.SecondOpenPlayerProfileButton.onClick.AddListener(() =>
                    {
                        StartCoroutine(GetFriendProfile(_friendsPlayersList[1]._id, null, itemPod.SecondOpenPlayerProfileButton));
                    });
                }
                if (_friendsPlayersList.Count > 2)
                {
                    itemPod.ThirdOpenPlayerProfileButton.onClick.AddListener(() =>
                    {
                        StartCoroutine(GetFriendProfile(_friendsPlayersList[2]._id, null, itemPod.ThirdOpenPlayerProfileButton));
                    });
                }
            }
            else if (rank > 3)
            {
                AvatarVisualData avatarVisualData = null;
                avatarVisualData = AvatarDesignLoader.Instance.CreateAvatarVisualData(friend.avatar);

                LeaderboardWinsItem item = Instantiate(_friendsWinsItemPrefab, parent: _friendsContent).GetComponent<LeaderboardWinsItem>();

                item.Initialize(rank, friend.name, 0, avatarVisualData);

                //open friend profile
                //item.OpenProfileButton.onClick.RemoveAllListeners();
                item.OpenProfileButton.onClick.AddListener(() =>
                {
                    StartCoroutine(GetFriendProfile(friend._id, item));
                });

                if (friend.name == _playerName) //color player's item white
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

                item.Initialize(i, "", 0, null);
            }
        }
        yield return null;
    }

    private IEnumerator GetFriendProfile(string _id, LeaderboardWinsItem item = null, Button itemPod = null)
    {
        bool timeout = false;
        ServerPlayer serverPlayer = null;

        //open loading image
        _loadingImage.SetActive(true);

        StartCoroutine(ServerManager.Instance.GetOtherPlayerFromServer(_id, c => serverPlayer = c)); // Get friend data
        yield return new WaitUntil(() => serverPlayer != null || timeout);

        DataCarrier.AddData(DataCarrier.PlayerProfile, new PlayerData(serverPlayer));

        //close loading image
        _loadingImage.SetActive(false);

        if (itemPod != null)
        {
            yield return itemPod.GetComponent<WindowNavigation>().Navigate();
        }
        else if(item != null)
        {
            yield return item.GetComponent<WindowNavigation>().Navigate();
        }
    }

    private void Update()
    {
        if (_currentScrollRect != null)
        {
            if (_currentScrollRect.verticalNormalizedPosition > 0.99)
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

        if (_playerItem != null)
        {
            RectTransform target = _playerItem;

            Vector2 listPostition = _scrollRect.viewport.localPosition;
            Vector2 targetPostition = target.localPosition;

            Vector2 newPosition = new Vector2(0,
                0 - (listPostition.y + targetPostition.y));

            _contentPanel.localPosition = newPosition;
        }
    }

    private long _frameTimeStart;
    private IEnumerator FrameChecker()
    {
        if (FrameCheck())
        {
            yield return null;
            _frameTimeStart = DateTime.Now.Ticks;
        }
    }
    private bool FrameCheck()
    {
        if ((new TimeSpan(DateTime.Now.Ticks - _frameTimeStart)).TotalSeconds > 1f / 60f)
        {
            return true;
        }
        return false;
    }
}
