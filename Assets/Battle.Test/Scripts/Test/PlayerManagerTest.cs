using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    internal class PlayerManagerTest : MonoBehaviour
    {
        [Header("Debug Only")] public bool _useScoreFlash;
        public bool _startCountdown;
        public bool _startGameplay;

        private IPlayerManager _playerManager;

        private void Awake()
        {
            _playerManager = Context.GetPlayerManager;
        }

        private void Update()
        {
            if (_startCountdown)
            {
                _startCountdown = false;
                Debug.Log("StartCountdown");
                if (_useScoreFlash) ScoreFlash.Push("StartCountdown");
                _playerManager.StartCountdown(() =>
                {
                    Debug.Log("Countdown done");
                    if (_useScoreFlash) ScoreFlash.Push("StartCountdown done");
                });
                return;
            }
            if (_startGameplay)
            {
                _startGameplay = false;
                Debug.Log("StartGameplay");
                if (_useScoreFlash) ScoreFlash.Push("StartGameplay");
                _playerManager.StartGameplay();
                return;
            }
        }
    }
}