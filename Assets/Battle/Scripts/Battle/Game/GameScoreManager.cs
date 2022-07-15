using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Ui;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Collects local scores (from master client) and synchronizes them over network.
    /// </summary>
    /// <remarks>
    /// Note that wall scores are internally counted but they are never sent outside!
    /// </remarks>
    public class GameScoreManager : MonoBehaviour, IGameScoreManager
    {
        [Header("Settings"), SerializeField] private WindowDef _gameOverWindow;

        [Header("Live Data"), SerializeField] private int _blueHeadScore;
        [SerializeField] private int _blueWallScore;
        [SerializeField] private int _redHeadScore;
        [SerializeField] private int _redWallScore;

        private int BlueScoreTotal => _blueHeadScore + 0;
        private int RedScoreTotal => _redHeadScore + 0;

        private int _headScoreToWin;

        private void Awake()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _headScoreToWin = variables._battleHeadScoreToWin;
            ResetScores();
        }

        public Tuple<int,int> BlueScore => new (_blueHeadScore, 0);

        public Tuple<int,int> RedScore => new (_redHeadScore, 0);

        public void ResetScores()
        {
            _blueHeadScore = 0;
            _blueWallScore = 0;
            _redHeadScore = 0;
            _redWallScore = 0;
        }

        public void OnHeadCollision(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _blueHeadScore += 1;
                if (_blueHeadScore >= _headScoreToWin && PhotonNetwork.InRoom)
                {
                    var room = PhotonNetwork.CurrentRoom;
                    var winType = BlueScoreTotal == RedScoreTotal ? PhotonBattle.WinTypeDraw : PhotonBattle.WinTypeScore;
                    var winningTeam = GetWinningTeam(BlueScoreTotal, RedScoreTotal);
                    ShowGameOverWindow(room, winType, winningTeam, BlueScoreTotal, RedScoreTotal);
                    return;
                }
                return;
            }
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _redHeadScore += 1;
                if (_redHeadScore >= _headScoreToWin && PhotonNetwork.InRoom)
                {
                    var room = PhotonNetwork.CurrentRoom;
                    var winType = BlueScoreTotal == RedScoreTotal ? PhotonBattle.WinTypeDraw : PhotonBattle.WinTypeScore;
                    var winningTeam = GetWinningTeam(BlueScoreTotal, RedScoreTotal);
                    ShowGameOverWindow(room, winType, winningTeam, BlueScoreTotal, RedScoreTotal);
                }
                return;
            }
            throw new UnityException($"invalid collision with {otherGameObject.name} {otherGameObject.tag}");
        }

        public void OnWallCollision(Collision2D collision)
        {
            var otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.RedTeam))
            {
                _blueWallScore += 1;
                this.Publish(new UiEvents.WallCollision(collision, PhotonBattle.TeamBlueValue));
                return;
            }
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _redWallScore += 1;
                this.Publish(new UiEvents.WallCollision(collision, PhotonBattle.TeamRedValue));
                return;
            }
            throw new UnityException($"invalid collision with {otherGameObject.name} {otherGameObject.tag}");
        }

        public void ShowGameOverWindow(Room room, int winType, int winningTeam, int blueScore, int redScore)
        {
            PhotonBattle.SetRoomScores(room, winType, winningTeam, blueScore, redScore);
            StartCoroutine(LoadGameOverWindow());
        }

        private IEnumerator LoadGameOverWindow()
        {
            yield return null;
            Debug.Log($"LoadGameOverWindow {_gameOverWindow}");
            WindowManager.Get().ShowWindow(_gameOverWindow);
        }

        private static int GetWinningTeam(int blueScore, int redScore)
        {
            if (blueScore > redScore)
            {
                return PhotonBattle.TeamBlueValue;
            }
            if (redScore > blueScore)
            {
                return PhotonBattle.TeamRedValue;
            }
            return PhotonBattle.NoTeamValue;
        }
    }
}