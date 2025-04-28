using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlternateTopPanel : MonoBehaviour
{


    [Header("LeaderBoard")]
    [SerializeField] private Image _leaderboardButton;
    [SerializeField] private Sprite _star;
    [SerializeField] private Sprite _bow;
    [SerializeField] private TextMeshProUGUI _rankingTextWins;
    [SerializeField] private TextMeshProUGUI _rankingTextActivity;

    private float _timer = 5;
    private string _ownPlayerName;

    private enum TopPanelInfo
    {
        Player,
        Clan
    }

    private enum LeaderboardType
    {
        Wins,
        Activity
    }

    private TopPanelInfo _currentTopPanelInfo = TopPanelInfo.Player;
    private LeaderboardType _currentLeaderboardType = LeaderboardType.Wins;

    private void OnEnable()
    {
        StartCoroutine(ServerManager.Instance.GetOwnPlayerFromServer((player) =>
        {
            _ownPlayerName = player.name;

            StartCoroutine(ChangeLeaderboardType());
        }));
    }

    /// <summary>
    /// Alternates between the activity and wins leaderboards.
    /// </summary>
    private IEnumerator ChangeLeaderboardType()
    {
        YieldInstruction wait = new WaitForSeconds(_timer);

        while (enabled)
        {
            switch (_currentLeaderboardType)
            {
                case LeaderboardType.Wins:
                    LoadActivity();
                    break;
                case LeaderboardType.Activity:
                    LoadWins();
                    break;
            }

            yield return wait;
        }
    }

    private void LoadWins()
    {
        _currentLeaderboardType = LeaderboardType.Wins;

        // Find the player's wins ranking
        StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
        {
            int rank = 1;

            playerLeaderboard.Sort((a, b) => a.WonBattles.CompareTo(b.WonBattles));

            foreach (PlayerLeaderboard ranking in playerLeaderboard)
            {
                if (ranking.Clan.Name.Equals(_ownPlayerName))
                {
                    break;
                }

                rank++;
            }

            _rankingTextWins.text = rank.ToString();
            _rankingTextActivity.text = "";
            _leaderboardButton.sprite = _star;
        }));
    }

    private void LoadActivity()
    {
        _currentLeaderboardType = LeaderboardType.Activity;
        
        // Find the player's activity ranking
        StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer((playerLeaderboard) =>
        {
            int rank = 1;

            playerLeaderboard.Sort((a, b) => a.Points.CompareTo(b.Points));

            foreach (PlayerLeaderboard ranking in playerLeaderboard)
            {
                if (ranking.Clan.Name.Equals(_ownPlayerName))
                {
                    break;
                }

                rank++;
            }

            _rankingTextActivity.text = rank.ToString();
            _rankingTextWins.text = "";
            _leaderboardButton.sprite = _bow;
        }));
    }
}
