
using System;
using MenuUi.Scripts.TabLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _titleText;

    [Header("Tabline")]
    [SerializeField] private Button _globalLeaderboardButton;
    [SerializeField] private Button _clanLeaderboardButton;
    [SerializeField] private Button _friendsLeaderboardButton;
    [SerializeField] private Button _leaderboardTypeButton;
    [SerializeField] private Image _tablineRibbon;
    [SerializeField] private TabLine _tablineScript;
    [SerializeField] private GameObject _leaderboardCategoryButtons;
    [SerializeField] private Button _clansButton;
    [SerializeField] private Button _playersButton;

    [Header("Leaderboard panels")]
    [SerializeField] private GameObject _winsPanel;
    [SerializeField] private Transform _winsContent;
    [SerializeField] private GameObject _activityPanel;
    [SerializeField] private Transform _activityContent;
    [SerializeField] private GameObject _clanPointsPanel;
    [SerializeField] private Transform _clanPointsContent;

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
        ClanPoints
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

        SetLeaderboardType(LeaderboardType.Activity);
        OpenLeaderboard(Leaderboard.Clan);
        LoadActivityView();
        _tablineScript.ActivateTabButton(1);
    }

    private void OpenLeaderboard(Leaderboard leaderboard)
    {
        _currentLeaderboard = leaderboard;

        SetLeaderboardType(LeaderboardType.Activity);
        _currentLeaderboardCategory = LeaderboardCategory.Players;
        UpdateTitle();
        LoadLeaderboard();
    }

    private void SetLeaderboardType(LeaderboardType leaderboardType)
    {
        _currentLeaderboardType = leaderboardType;
        _winsPanel.SetActive(_currentLeaderboardType == LeaderboardType.Wins);
        _activityPanel.SetActive(_currentLeaderboardType == LeaderboardType.Activity);
        _clanPointsPanel.SetActive(_currentLeaderboardType == LeaderboardType.ClanPoints);
    }

    private void UpdateTitle()
    {
        _titleText.text = _currentLeaderboard switch
        {
            Leaderboard.Global => "Globaali tulostaulukko",
            Leaderboard.Clan => "Klaanin tulostaulukko",
            Leaderboard.Friends => "Kavereitten tulostaulukko",
            _ => ""
        };
    }

    private void LoadLeaderboard()
    {
        foreach (Transform child in _winsContent) Destroy(child.gameObject);
        foreach (Transform child in _activityContent) Destroy(child.gameObject);
        foreach (Transform child in _clanPointsContent) Destroy(child.gameObject);

        _leaderboardTypeButton.interactable = (_currentLeaderboardType != LeaderboardType.ClanPoints);
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
                                LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                                item.Initialize(rank, ranking.Clan.Name, ranking.WonBattles);
                                rank++;
                            }
                        }
                        else
                        {
                            playerLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

                            int rank = 1;
                            foreach (PlayerLeaderboard ranking in playerLeaderboard)
                            {
                                LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                                item.Initialize(rank, ranking.Clan.Name, ranking.Points);
                                rank++;
                            };
                        }
                    }));
                }
                else
                {
                    StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
                    {
                        clanLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

                        int rank = 1;
                        foreach (ClanLeaderboard ranking in clanLeaderboard)
                        {
                            LeaderboardClanPointsItem item = Instantiate(_clanPointsItemPrefab, parent: _clanPointsContent).GetComponent<LeaderboardClanPointsItem>();
                            item.Initialize(rank, ranking.Clan.Name, ranking.Points);
                            rank++;
                        }

                    }));
                }
                break;
            case Leaderboard.Clan:
                // For Testing
                if (_currentLeaderboardType == LeaderboardType.Wins)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                        item.Initialize(i, ((char)(64 + i)).ToString(), 16);
                    }
                }
                else
                {
                    for (int i = 1; i < 5; i++)
                    {
                        LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                        item.Initialize(i, ((char)(64 + i)).ToString(), 100);
                    };
                }
                break;
            case Leaderboard.Friends:
                // For Testing
                for (int i = 1; i < 5; i++)
                {
                    LeaderboardWinsItem item = Instantiate(_playerWinsItemPrefab, parent: _winsContent).GetComponent<LeaderboardWinsItem>();
                    item.Initialize(i, ((char)(64 + i)).ToString(), 16);
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

        //Tabline colors
        Color activityRed = new Color(0.8549f, 0.2352f, 0.3254f);
        ChangeTablineButtonColors(_friendsLeaderboardButton, activityRed);
        ChangeTablineButtonColors(_clanLeaderboardButton, activityRed);
        ChangeTablineButtonColors(_globalLeaderboardButton, activityRed);
        _tablineRibbon.color = activityRed;
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

        //Tabline colors
        Color winsYellow = new Color(1f, 0.6313f, 0f);
        ChangeTablineButtonColors(_friendsLeaderboardButton, winsYellow);
        ChangeTablineButtonColors(_clanLeaderboardButton, winsYellow);
        ChangeTablineButtonColors(_globalLeaderboardButton, winsYellow);
        _tablineRibbon.color = winsYellow;
    }

    private void ChangeTablineButtonColors(Button button, Color color)
    {
        var colors = button.colors;
        colors.normalColor = color;
        colors.pressedColor = color;
        colors.selectedColor = color;
        button.colors = colors;
    }

    private void ListClans()
    {
        _currentLeaderboardCategory = LeaderboardCategory.Clans;
        SetLeaderboardType(LeaderboardType.ClanPoints);
        LoadLeaderboard();
    }

    private void ListPlayers()
    {
        _currentLeaderboardCategory = LeaderboardCategory.Players;
        SetLeaderboardType(LeaderboardType.Activity);
        LoadLeaderboard();
    }
}
