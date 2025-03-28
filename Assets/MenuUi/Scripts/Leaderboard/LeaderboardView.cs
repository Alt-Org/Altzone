
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _titleText;

    [Header("Tab Buttons")]
    [SerializeField] private Button _globalLeaderboardButton;
    [SerializeField] private Button _clanLeaderboardButton;
    [SerializeField] private Button _friendsLeaderboardButton;
    [SerializeField] private GameObject _leaderboardTypeButtons;
    [SerializeField] private Button _winsButton;
    [SerializeField] private Button _activityButton;

    [Header("Leaderboard panels")]
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

    private enum Leaderboard
    {
        Global,
        Clan,
        Friends
    }
    private enum LeaderboardType
    {
        Wins,
        Activity
    }
    private Leaderboard _currentLeaderboard = Leaderboard.Global;
    private LeaderboardType _currentLeaderboardType = LeaderboardType.Wins;

    private void Start()
    {
        _globalLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Global));
        _clanLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Clan));
        _friendsLeaderboardButton.onClick.AddListener(() => OpenLeaderboard(Leaderboard.Friends));
        _winsButton.onClick.AddListener(() =>
        {
            SetLeaderboardType(LeaderboardType.Wins);
            UpdateTitle();
            LoadLeaderboard();
        });
        _activityButton.onClick.AddListener(() =>
        {
            SetLeaderboardType(LeaderboardType.Activity);
            UpdateTitle();
            LoadLeaderboard();
        });

        SetLeaderboardType(LeaderboardType.Activity);
        OpenLeaderboard(Leaderboard.Clan);
    }

    private void OpenLeaderboard(Leaderboard leaderboard)
    {
        _currentLeaderboard = leaderboard;

        _leaderboardTypeButtons.SetActive(leaderboard != Leaderboard.Friends);
        if (leaderboard == Leaderboard.Friends) SetLeaderboardType(LeaderboardType.Wins);

        UpdateTitle();
        LoadLeaderboard();
    }

    private void SetLeaderboardType(LeaderboardType leaderboardType)
    {
        _currentLeaderboardType = leaderboardType;
        _winsPanel.SetActive(_currentLeaderboardType == LeaderboardType.Wins);
        _activityPanel.SetActive(_currentLeaderboardType == LeaderboardType.Activity);
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

        switch (_currentLeaderboard)
        {
            case Leaderboard.Global:
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

                        LoadWinsView();
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

                        LoadActivityView();
                    }
                }));
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

                    LoadWinsView();
                }
                else
                {
                    for (int i = 1; i < 5; i++)
                    {
                        LeaderboardActivityItem item = Instantiate(_playerActivityItemPrefab, parent: _activityContent).GetComponent<LeaderboardActivityItem>();
                        item.Initialize(i, ((char)(64 + i)).ToString(), 100);
                    };

                    LoadActivityView();
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

    private void LoadActivityView()
    {
        foreach (GameObject icon in _winsViewIcons)
        {
            icon.SetActive(false);
        }

        foreach (GameObject icon in _activityViewIcons)
        {
            icon.SetActive(true);
        }
    }

    private void LoadWinsView()
    {
        foreach (GameObject icon in _activityViewIcons)
        {
            icon.SetActive(false);
        }

        foreach (GameObject icon in _winsViewIcons)
        {
            icon.SetActive(true);
        }
    }
}
