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

    [Header("LeaderBoard")]
    [SerializeField] private Image _leaderboardButton;
    [SerializeField] private Sprite _star;
    [SerializeField] private Sprite _bow;
    [SerializeField] private TextMeshProUGUI _rankingTextWins;
    [SerializeField] private TextMeshProUGUI _rankingTextActivity;

    private float _timerLeaderboard = 5;
    private float _timerInfo = 10;
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

    private TopPanelInfo _currentTopPanelInfo = TopPanelInfo.Player;

    private void OnEnable()
    {
        if(ServerManager.Instance.Player != null)
        {
            _ownPlayerID = ServerManager.Instance.Player._id;
            _ownClanID = ServerManager.Instance.Player.clan_id;

            FetchRankings();
            StartCoroutine(ChangeInfoType());
        }
        else
        StartCoroutine(ServerManager.Instance.GetOwnPlayerFromServer((player) =>
        {
            if (player == null) { Debug.LogError("Cannot get PlayerData."); return; }
            _ownPlayerID = player._id;
            _ownClanID = player.clan_id;

            FetchRankings();
            StartCoroutine(ChangeInfoType());
        }));
    }

    /// <summary>
    /// Alternates between showing player's info and clan's info
    /// </summary>
    private IEnumerator ChangeInfoType()
    {
        YieldInstruction wait = new WaitForSeconds(_timerInfo);

        while (enabled)
        {
            _playerInfo.SetActive(_currentTopPanelInfo == TopPanelInfo.Player);
            _clanInfo.SetActive(_currentTopPanelInfo == TopPanelInfo.Clan);
            StartCoroutine(ChangeLeaderboardType());

            yield return wait;

            switch (_currentTopPanelInfo)
            {
                case TopPanelInfo.Player:
                    _currentTopPanelInfo = TopPanelInfo.Clan;
                    break;
                case TopPanelInfo.Clan:
                    _currentTopPanelInfo = TopPanelInfo.Player;
                    break;
            }
        }
    }

    /// <summary>
    /// Alternates between the activity and wins leaderboards.
    /// </summary>
    private IEnumerator ChangeLeaderboardType()
    {
        YieldInstruction wait = new WaitForSeconds(_timerLeaderboard);

        LoadWins();

        yield return wait;

        LoadActivity();
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
