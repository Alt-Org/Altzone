using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Realtime;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Collects local scores (from master client) and synchronizes them over network.
    /// </summary>
    public class GameScoreManager : MonoBehaviour, IGameScoreManager
    {
        [Header("Settings"), SerializeField] private WindowDef _gameOverWindow;

        [Header("Live Data"), SerializeField] private int _blueHeadScore;
        [SerializeField] private int _blueWallScore;
        [SerializeField] private int _redHeadScore;
        [SerializeField] private int _redWallScore;

        public void Reset()
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
                return;
            }
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _redHeadScore += 1;
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
                return;
            }
            if (otherGameObject.CompareTag(Tags.BlueTeam))
            {
                _redWallScore += 1;
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
    }
}