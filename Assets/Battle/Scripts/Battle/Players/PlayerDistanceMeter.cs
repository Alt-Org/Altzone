using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerDistanceMeter : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private Transform _playerTransform1;
        [SerializeField] private Transform _playerTransform2;
        [SerializeField] private float _sqrMinVisibleDistance;
        [SerializeField] private float _sqrCurrentDistance;
        [SerializeField] private bool _currentVisibility;

        private IPlayerDriver _playerDriver;

        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            // IPlayerDriver might get instantiated, awoken and enabled before we are called at all - not even Awake() is called!

            _playerDriver = playerDriver;
            _playerTransform1 = playerDriver.PlayerTransform;
            _playerTransform2 = _playerTransform1;
            Debug.Log($"me {_playerDriver} @ {(Vector2)_playerTransform1.position}");

            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            var isSinglePlayerShieldOn = features._isSinglePlayerShieldOn;
            playerDriver.SetShieldVisibility(isSinglePlayerShieldOn);

            var defence = playerDriver.CharacterModel.Defence;
            var variables = runtimeGameConfig.Variables;
            var multiplier = variables._shieldDistanceMultiplier;
            _sqrMinVisibleDistance = (defence * multiplier) * (defence * multiplier);

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
            var player = data.Player;
            if (player.ActorNumber == _playerDriver.ActorNumber)
            {
                return;
            }
            if (player.TeamNumber != _playerDriver.TeamNumber)
            {
                return;
            }
            _playerTransform2 = player.PlayerTransform;
            StartCoroutine(TrackPlayers());
        }

        private void OnPlayerLeft(PlayerLeft data)
        {
            Debug.Log($"me {_playerDriver} {data}");
            var player = data.Player;
            if (player.TeamNumber != _playerDriver.TeamNumber)
            {
                return;
            }
            _sqrCurrentDistance = 0;
            StopAllCoroutines();
        }

        private IEnumerator TrackPlayers()
        {
            var delay = new WaitForFixedUpdate();
            while (PhotonNetwork.InRoom)
            {
                _sqrCurrentDistance = Mathf.Abs((_playerTransform1.position - _playerTransform2.position).sqrMagnitude);
                var isVisible = _sqrCurrentDistance < _sqrMinVisibleDistance;
                if (isVisible != _currentVisibility)
                {
                    _currentVisibility = isVisible;
                    _playerDriver.SetShieldVisibility(_currentVisibility);
                }
                yield return delay;
            }
        }
    }
}