
using System;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.TabLine;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [Header("Tabline")]
    [SerializeField] private Button _globalLeaderboardButton;
    [SerializeField] private Image _globalLeaderboardImage;
    [SerializeField] private Button _clanLeaderboardButton;
    [SerializeField] private Image _clanLeaderboardImage;
    [SerializeField] private Button _friendsLeaderboardButton;
    [SerializeField] private Image _friendsLeaderboardImage;
    [SerializeField] private Button _leaderboardTypeButton;
    [SerializeField] private Image _tablineRibbon;
    [SerializeField] private TabLine _tablineScript;
    [SerializeField] private GameObject _leaderboardCategoryButtons;
    [SerializeField] private Button _clansButton;
    [SerializeField] private Button _playersButton;

    [Header("Tab sprites")]
    [SerializeField] private Sprite _globalWinsSprite;
    [SerializeField] private Sprite _globalActivitySprite;
    [SerializeField] private Sprite _clanWinsSprite;
    [SerializeField] private Sprite _clanActivitySprite;
    [SerializeField] private Sprite _friendsWinsSprite;
    [SerializeField] private Sprite _friendsActivitySprite;

    [Header("Leaderboard panels")]
    [SerializeField] private LeaderboardPodium _podium;
    [SerializeField] private GameObject _winsPanel;
    [SerializeField] private Transform _winsContent;
    [SerializeField] private GameObject _activityPanel;
    [SerializeField] private Transform _activityContent;

    [Header("Different view icons")]
    [SerializeField] private GameObject[] _activityViewIcons;
    [SerializeField] private GameObject[] _winsViewIcons;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerWinsItemPrefab;
    [SerializeField] private GameObject _playerActivityItemPrefab;
    [SerializeField] private GameObject _clanPointsItemPrefab;

    private enum Leaderboard
    {
        Global,
        Clan,
        Friends
    }
    private enum LeaderboardType
    {
        Wins,
        Activity,
    }
    private enum LeaderboardCategory
    {
        Clans,
        Players
    }
    private Leaderboard _currentLeaderboard = Leaderboard.Global;
    private LeaderboardType _currentLeaderboardType = LeaderboardType.Wins;
    private LeaderboardCategory _currentLeaderboardCategory = LeaderboardCategory.Players;

    private void Start()
    {
        _globalLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Global));
        _clanLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Clan));
        _friendsLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Friends));

        _leaderboardTypeButton.onClick.AddListener(() => ToggleLeaderboardType());

        _clansButton.onClick.AddListener(() => ListClans());
        _playersButton.onClick.AddListener(() => ListPlayers());
    }

    private void OnEnable()
    {
        SetLeaderboardType(LeaderboardType.Activity);
        OpenLeaderboard(Leaderboard.Clan);
        LoadActivityView();
        _tablineScript.ActivateTabButton(1);
    }

    private void OpenLeaderboard(Leaderboard leaderboard)
    {
        _currentLeaderboard = leaderboard;

        _currentLeaderboardCategory = LeaderboardCategory.Players;
        LoadActivityView();
    }

    private void SetLeaderboardType(LeaderboardType leaderboardType)
    {
        _currentLeaderboardType = leaderboardType;
        _winsPanel.SetActive(_currentLeaderboardType == LeaderboardType.Wins);
        _activityPanel.SetActive(_currentLeaderboardType == LeaderboardType.Activity);
    }

    private void LoadLeaderboard()
    {
        foreach (Transform child in _winsContent) Destroy(child.gameObject);
        foreach (Transform child in _activityContent) Destroy(child.gameObject);

        _podium.SetPlayerView();
        _leaderboardTypeButton.interactable = true;

        _leaderboardCategoryButtons.SetActive(_currentLeaderboard == Leaderboard.Global);

        switch (_currentLeaderboard)
        {
            case Leaderboard.Global:
                if (_currentLeaderboardCategory == LeaderboardCategory.Players)
                {
                    StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
                    {
                        if (_currentLeaderboardType == LeaderboardType.Wins)
                        {
                            playerLeaderboard.Sort((a, b) => a.WonBattles.CompareTo(b.WonBattles));

                            int rank = 1;
                            foreach (PlayerLeaderboard ranking in playerLeaderboard)
                            {
                                AvatarVisualData avatarVisualData = null;

                                if (ranking.Clan.SelectedCharacterId != 201 && ranking.Clan.SelectedCharacterId != 0)
                                {
                                    avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(ranking.Clan);
                                }

                                if (rank < 4)
                                {
                                    _podium.InitilializePodium(rank, ranking.Clan.Name, ranking.Points, ranking.Clan);
                                }
                                else
                                {
                                    LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                    item.Initialize(rank, ranking.Clan.Name, ranking.Points, avatarVisualData);

                                    // View player profile button
                                    item.OpenProfileButton.onClick.AddListener(() =>
                                    {
                                        DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Clan);
                                    });
                                }

                                rank++;
                            }
                        }
                        else
                        {
                            playerLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

                            int rank = 1;
                            foreach (PlayerLeaderboard ranking in playerLeaderboard)
                            {
                                AvatarVisualData avatarVisualData = null;

                                if (ranking.Clan.SelectedCharacterId != 201 && ranking.Clan.SelectedCharacterId != 0)
                                {
                                    avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(ranking.Clan);
                                }

                                if (rank < 4)
                                {
                                    _podium.InitilializePodium(rank, ranking.Clan.Name, ranking.Points, ranking.Clan);
                                }
                                else
                                {
                                    LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                                    item.Initialize(rank, ranking.Clan.Name, ranking.Points, avatarVisualData);

                                    // View player profile button
                                    item.OpenProfileButton.onClick.AddListener(() =>
                                    {
                                        DataCarrier.AddData(DataCarrier.PlayerProfile, ranking.Clan);
                                    });
                                }

                                rank++;
                            };
                        }
                    }));
                }
                else // Clans leaderboard
                {
                    StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
                    {

                        _leaderboardTypeButton.interactable = false;
                        _podium.SetClanView();

                        clanLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

                        int rank = 1;
                        foreach (ClanLeaderboard ranking in clanLeaderboard)
                        {
                            ClanData clanData = ranking.Clan;
                            ServerClan serverClan = ranking.ServerClan;

                            if (rank < 4)
                            {
                                _podium.InitilializePodium(rank, ranking.Clan.Name, ranking.Points, clanData, serverClan);
                            }
                            else
                            {
                                LeaderboardClanPointsItem item = Instantiate(_clanPointsItemPrefab, parent: _activityContent).GetComponent<LeaderboardClanPointsItem>();
                                item.Initialize(rank, ranking.Clan.Name, ranking.Points);

                                // Clan heart colors
                                ClanHeartColorSetter clanheart = item.GetComponentInChildren<ClanHeartColorSetter>();
                                clanheart.SetOtherClanColors(clanData);

                                // View clan button
                                item.OpenProfileButton.onClick.RemoveAllListeners();
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.ClanListing, serverClan);
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
                    if (_currentLeaderboardType == LeaderboardType.Wins)
                    {
                        clanData.Members.Sort((a, b) => a.LeaderBoardWins.CompareTo(b.LeaderBoardWins));

                        int rank = 1;
                        foreach (ClanMember player in clanData.Members)
                        {
                            PlayerData playerData = player.GetPlayerData();
                            AvatarVisualData avatarVisualData = null;

                            if (playerData.SelectedCharacterId != 201 && playerData.SelectedCharacterId != 0)
                            {
                                avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(playerData);
                            }

                            if (rank < 4)
                            {
                                _podium.InitilializePodium(rank, player.Name, player.LeaderBoardWins, playerData);
                            }
                            else
                            {
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, player.Name, player.LeaderBoardWins, avatarVisualData);

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
                                    item.Initialize(rank, "", 0, null);
                                }

                                rank++;
                            }
                        }
                    }
                    else
                    {
                        clanData.Members.Sort((a, b) => a.LeaderBoardCoins.CompareTo(b.LeaderBoardCoins));

                        int rank = 1;
                        foreach (ClanMember player in clanData.Members)
                        {
                            PlayerData playerData = player.GetPlayerData();
                            AvatarVisualData avatarVisualData = null;

                            if (playerData.SelectedCharacterId != 201 && playerData.SelectedCharacterId != 0)
                            {
                                avatarVisualData = AvatarDesignLoader.Instance.LoadAvatarDesign(playerData);
                            }

                            if (rank < 4)
                            {
                                _podium.InitilializePodium(rank, player.Name, player.LeaderBoardCoins, playerData);
                            }
                            else
                            {
                                LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                                item.Initialize(rank, player.Name, player.LeaderBoardCoins, avatarVisualData);

                                // View player profile button
                                item.OpenProfileButton.onClick.AddListener(() =>
                                {
                                    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                                });
                            }

                            rank++;
                        }

                        // Add empty placements to fill out the space if necessary 
                        if(clanData.Members.Count < 15)
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
                                    LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                                    item.Initialize(rank, "", 0, null);
                                }

                                rank++;
                            }
                        }
                    }
                });
                break;
            case Leaderboard.Friends:
                // For Testing
                if (_currentLeaderboardType == LeaderboardType.Wins)
                {
                    for (int i = 1; i < 20; i++)
                    {
                        if (i < 4)
                        {
                            _podium.InitilializePodium(i, "", 0, null);
                        }
                        else
                        {
                            LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                            item.Initialize(i, "", 0, null);

                            // View player profile button
                            //item.OpenProfileButton.onClick.AddListener(() =>
                            //{
                            //    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                            //});
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < 20; i++)
                    {
                        if (i < 4)
                        {
                            _podium.InitilializePodium(i, "", 0, null);
                        }
                        else
                        {
                            LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                            item.Initialize(i, "", 0, null);

                            // View player profile button
                            //item.OpenProfileButton.onClick.AddListener(() =>
                            //{
                            //    DataCarrier.AddData(DataCarrier.PlayerProfile, playerData);
                            //});
                        }
                    }
                }

                break;
        }
    }

    private void ToggleLeaderboardType()
    {
        if (_currentLeaderboardType == LeaderboardType.Activity)
        {
            LoadWinsView();
        }
        else
        {
            LoadActivityView();
        }
    }

    private void LoadActivityView()
    {
        SetLeaderboardType(LeaderboardType.Activity);
        LoadLeaderboard();

        // Activate/deactivate necessary icons
        foreach (GameObject icon in _winsViewIcons)
        {
            icon.SetActive(false);
        }

        foreach (GameObject icon in _activityViewIcons)
        {
            icon.SetActive(true);
        }

        // Tabline colors
        Color activityRed = new Color(0.8549f, 0.2352f, 0.3254f);
        _tablineRibbon.color = activityRed;

        // Tab sprites
        _globalLeaderboardImage.sprite = _globalActivitySprite;
        _clanLeaderboardImage.sprite = _clanActivitySprite;
        _friendsLeaderboardImage.sprite = _friendsActivitySprite;
    }

    private void LoadWinsView()
    {
        SetLeaderboardType(LeaderboardType.Wins);
        LoadLeaderboard();

        // Activate/deactivate necessary icons
        foreach (GameObject icon in _activityViewIcons)
        {
            icon.SetActive(false);
        }

        foreach (GameObject icon in _winsViewIcons)
        {
            icon.SetActive(true);
        }

        // Tabline colors
        Color winsYellow = new Color(1f, 0.6313f, 0f);
        _tablineRibbon.color = winsYellow;

        // Tab sprites
        _globalLeaderboardImage.sprite = _globalWinsSprite;
        _clanLeaderboardImage.sprite = _clanWinsSprite;
        _friendsLeaderboardImage.sprite = _friendsWinsSprite;
    }

    private void ListClans()
    {
        _currentLeaderboardCategory = LeaderboardCategory.Clans;
        LoadActivityView();
    }

    private void ListPlayers()
    {
        _currentLeaderboardCategory = LeaderboardCategory.Players;
        LoadActivityView();
    }
}
