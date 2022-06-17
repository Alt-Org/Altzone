using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerDistanceMeter : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private bool _isSinglePlayerShieldOn;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _sqrDistance;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            _isSinglePlayerShieldOn = features._isSinglePlayerShieldOn;
        }

        public void SetPlayerDriver(IPlayerDriver playerDriver, Transform playerTransform)
        {
            Debug.Log($"me {playerDriver} @ {(Vector2)playerTransform.position}");
            _playerDriver = playerDriver;
            _playerTransform = playerTransform;
            playerDriver.SetShieldVisibility(_isSinglePlayerShieldOn);
            this.Subscribe<PlayerJoined>(OnPlayerJoined);
            this.Subscribe<PlayerLeft>(OnPlayerLeft);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            this.Unsubscribe();
        }

        private void OnPlayerJoined(PlayerJoined data)
        {
            Debug.Log($"me {_playerDriver} {data}");
        }

        private void OnPlayerLeft(PlayerLeft data)
        {
            Debug.Log($"me {_playerDriver} {data}");
        }
    }
}