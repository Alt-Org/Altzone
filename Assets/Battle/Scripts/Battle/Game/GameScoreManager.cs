using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Realtime;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Collects local scores (from master client) and synchronizes them over network.
    /// </summary>
    public class GameScoreManager : MonoBehaviour, IGameScoreManager
    {
        [Header("Settings"), SerializeField] private WindowDef _gameOverWindow;

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