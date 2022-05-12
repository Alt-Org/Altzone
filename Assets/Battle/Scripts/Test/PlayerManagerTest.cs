using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class PlayerManagerTest : MonoBehaviour
    {
        [Header("Debug Only")] public bool _startCountdown;
        public bool _useScoreFlash;

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
                if (_useScoreFlash)
                {
                    Debug.Log("StartCountdown");
                    ScoreFlash.Push("Start Countdown");
                    _playerManager.StartCountdown(() =>
                    {
                        Debug.Log("Countdown done");
                        ScoreFlash.Push("Countdown done");
                    });
                }
                return;
            }
        }
    }
}