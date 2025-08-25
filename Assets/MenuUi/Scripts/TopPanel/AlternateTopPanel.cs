using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlternateTopPanel : AltMonoBehaviour
{
    [Header("Clan/Player info")]
    [SerializeField] private GameObject _playerInfo;
    [SerializeField] private GameObject _clanInfo;
    [SerializeField] private Button _playerButton;
    [SerializeField] private Button _clanButton;

    [Header("LeaderBoard")]
    [SerializeField] private Image _leaderboardButton;
    [SerializeField] private Sprite _star;
    [SerializeField] private Sprite _bow;
    [SerializeField] private TextMeshProUGUI _rankingTextWins;
    [SerializeField] private TextMeshProUGUI _rankingTextActivity;
    [SerializeField] private bool _alternateLeaderboard;


    private float _timerLeaderboard = 5;
    private float _timerInfo = 10;
    private static float _currenttimerLeaderboard = 0;
    private static float _currenttimerInfo = 0;
    private string _ownPlayerID;
    private string _ownClanID;
    private int _playerActivityRanking;
    private int _playerWinsRanking;
    private int _clanActivityRanking;
    private int _clanWinsRanking;

    private enum TopPanelInfo
    {
        Player,
        Clan
    }
    private enum TopLeaderboardInfo
    {
        Wins,
        Activity
    }

    private static TopPanelInfo _currentTopPanelInfo = TopPanelInfo.Player;
    private static TopLeaderboardInfo _currentTopLeaderboardInfo = TopLeaderboardInfo.Wins;
    private static Coroutine _timerCoroutine;
    private static Coroutine _leaderBoardCoroutine;

    private delegate void TopPanelChanged();
    private static event TopPanelChanged OnTopPanelChanged;

    private delegate void LeaderBoardChange();
    private static event LeaderBoardChange OnLeaderBoardChange;


    private void OnEnable()
    {
        OnTopPanelChanged += ChangeInfoData;
        OnLeaderBoardChange += ChangeLeaderboardType;

        if (!_alternateLeaderboard)
        {
            _currentTopPanelInfo = TopPanelInfo.Clan;
            _currentTopLeaderboardInfo = TopLeaderboardInfo.Wins;
        }

        if (ServerManager.Instance.Player != null)
        {
            _ownPlayerID = ServerManager.Instance.Player._id;
            _ownClanID = ServerManager.Instance.Player.clan_id;

            FetchRankings();
            if (ServerManager.Instance.Player?.clan_id == null) _currentTopPanelInfo = TopPanelInfo.Player;
            if (_alternateLeaderboard)
            {
                ChangeInfoData();
                ChangeLeaderboardType();
                if (ServerManager.Instance.Player?.clan_id != null)
                    StartCoroutine(ChangeInfoType());
            }
        }
        else
        {
            StartCoroutine(ServerManager.Instance.GetOwnPlayerFromServer((player) =>
            {
                if (player == null) { Debug.LogError("Cannot get PlayerData."); return; }
                _ownPlayerID = player._id;
                _ownClanID = player.clan_id;

                FetchRankings();
                if (ServerManager.Instance.Player?.clan_id != null) _currentTopPanelInfo = TopPanelInfo.Player;
                if (_alternateLeaderboard)
                {
                    ChangeInfoData();
                    ChangeLeaderboardType();
                    if (ServerManager.Instance.Player?.clan_id != null)
                        StartCoroutine(ChangeInfoType());
                }
            }));
        }
    }
    private void OnDisable()
    {
        OnTopPanelChanged -= ChangeInfoData;
        OnLeaderBoardChange -= ChangeLeaderboardType;
        StopCoroutine(ChangeLeaderboardCoroutine());
    }

    /// <summary>
    /// Alternates between showing player's info and clan's info
    /// </summary>
    private IEnumerator ChangeInfoType()
    {
        if (_currenttimerInfo <= 0) _currenttimerInfo += _timerInfo;
        //if (_currenttimerLeaderboard < 0) _currenttimerLeaderboard = 0;
        if (_timerCoroutine != null)
        StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(StartTimer());
        //YieldInstruction wait = new WaitUntil(() => _currenttimerInfo <= 0);

        while (enabled)
        {
            StartCoroutine(ChangeLeaderboardCoroutine());

            yield return new WaitUntil(() => _currenttimerInfo <= 0);
            switch (_currentTopPanelInfo)
            {
                case TopPanelInfo.Player:
                    _currentTopPanelInfo = TopPanelInfo.Clan;
                    break;
                case TopPanelInfo.Clan:
                    _currentTopPanelInfo = TopPanelInfo.Player;
                    break;
            }
            OnTopPanelChanged.Invoke();
            _currenttimerInfo += _timerInfo;
        }
    }

    private IEnumerator StartTimer()
    {
        while (true)
        {
            yield return null;
            _currenttimerInfo -= Time.deltaTime;
            _currenttimerLeaderboard -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Alternates between the activity and wins leaderboards.
    /// </summary>
    private IEnumerator ChangeLeaderboardCoroutine()
    {
        YieldInstruction wait = new WaitForSeconds(_timerLeaderboard);

        if (_currentTopLeaderboardInfo != TopLeaderboardInfo.Wins)
        {
            if (_currenttimerLeaderboard <= 0) _currenttimerLeaderboard += _timerLeaderboard;
            yield return new WaitUntil(() => _currenttimerLeaderboard <= 0);
            _currentTopLeaderboardInfo = TopLeaderboardInfo.Wins;
            OnLeaderBoardChange.Invoke();
        }
        if (_currenttimerLeaderboard <= 0) _currenttimerLeaderboard += _timerLeaderboard;
        yield return new WaitUntil(() => _currenttimerLeaderboard <= 0);
        _currentTopLeaderboardInfo = TopLeaderboardInfo.Activity;
        OnLeaderBoardChange.Invoke();

    }

    private void ChangeInfoData()
    {
        _playerInfo.SetActive(_currentTopPanelInfo == TopPanelInfo.Player);
        _playerButton.enabled = _currentTopPanelInfo == TopPanelInfo.Player;
        _clanInfo.SetActive(_currentTopPanelInfo == TopPanelInfo.Clan);
        _clanButton.enabled = _currentTopPanelInfo == TopPanelInfo.Clan;
        _clanButton.GetComponent<Image>().raycastTarget = _currentTopPanelInfo == TopPanelInfo.Clan;
        _clanButton.gameObject.SetActive(ServerManager.Instance.Player?.clan_id != null);
    }

    private void ChangeLeaderboardType()
    {
        switch (_currentTopLeaderboardInfo)
        {
            case TopLeaderboardInfo.Wins:
                LoadWins();
                break;
            case TopLeaderboardInfo.Activity:
                LoadActivity();
                break;
        }
    }

    private void LoadWins()
    {
        switch (_currentTopPanelInfo)
        {
            case TopPanelInfo.Player:
                _rankingTextWins.text = _playerWinsRanking.ToString();
                break;
            case TopPanelInfo.Clan:
                _rankingTextWins.text = _clanWinsRanking.ToString();
                break;
        }

        _rankingTextActivity.text = "";
        _leaderboardButton.sprite = _star;
    }

    private void LoadActivity()
    {
        switch (_currentTopPanelInfo)
        {
            case TopPanelInfo.Player:
                _rankingTextActivity.text = _playerActivityRanking.ToString();
                break;
            case TopPanelInfo.Clan:
                _rankingTextActivity.text = _clanActivityRanking.ToString();
                break;
        }

        _rankingTextWins.text = "";
        _leaderboardButton.sprite = _bow;
    }

    private void FetchRankings()
    {
        // Find the player's rankings within clan
        StartCoroutine(GetClanData(ServerManager.Instance.Clan?._id, (clanData) =>
        {
            // Wins
            clanData.Members.Sort((a, b) => a.LeaderBoardWins.CompareTo(b.LeaderBoardWins));

            int rankWins = 1;

            foreach (ClanMember player in clanData.Members)
            {
                if(player.Id.Equals(_ownPlayerID))
                {
                    break;
                }

                rankWins++;
            }

            _playerWinsRanking = rankWins;

            // Activity
            clanData.Members.Sort((a, b) => a.LeaderBoardCoins.CompareTo(b.LeaderBoardCoins));

            int rankActivity = 1;

            foreach (ClanMember player in clanData.Members)
            {
                if (player.Id.Equals(_ownPlayerID))
                {
                    break;
                }

                rankActivity++;
            }

            _playerActivityRanking = rankActivity;
        }));


        // Find the clan's global wins ranking (not available yet)
        //StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
        //{
        //    int rank = 1;

        //    clanLeaderboard.Sort((a, b) => a.WonBattles.CompareTo(b.WonBattles));

        //    foreach (ClanLeaderboard ranking in clanLeaderboard)
        //    {
        //        if (ranking.Clan.Name.Equals(_ownClanName))
        //        {
        //            break;
        //        }

        //        rank++;
        //    }

        //    _clanWinsRanking = rank;
        //}));

        // Find the clan's global activity ranking
        StartCoroutine(ServerManager.Instance.GetClanLeaderboardFromServer((clanLeaderboard) =>
        {
            int rank = 1;

            clanLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

            foreach (ClanLeaderboard ranking in clanLeaderboard)
            {
                if (ranking.Clan.Id.Equals(_ownClanID))
                {
                    break;
                }

                rank++;
            }

            _clanActivityRanking = rank;
        }));
    }
}
