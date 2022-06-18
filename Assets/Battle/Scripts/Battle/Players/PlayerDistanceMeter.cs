using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
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
        private bool _isSinglePlayerShieldOn;

        /// <summary>
        /// Manages <c>IPlayerDriver</c> connection.
        /// </summary>
        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            // IPlayerDriver might get instantiated, awoken and enabled before we are called at all - not even Awake() is called!

            _playerDriver = playerDriver;
            if (playerDriver == null)
            {
                enabled = false;
                return;
            }
            _playerTransform1 = playerDriver.PlayerTransform;
            Debug.Log($"{_playerDriver} @ {(Vector2)_playerTransform1.position}");

            var runtimeGameConfig = RuntimeGameConfig.Get();

            var defence = playerDriver.CharacterModel.Defence;
            var variables = runtimeGameConfig.Variables;
            var multiplier = variables._shieldDistanceMultiplier;
            _sqrMinVisibleDistance = (defence * multiplier) * (defence * multiplier);

            var features = runtimeGameConfig.Features;
            _isSinglePlayerShieldOn = features._isSinglePlayerShieldOn;

            this.Subscribe<PlayerJoined>(OnPlayerJoined);
            this.Subscribe<PlayerLeft>(OnPlayerLeft);

            CheckMyTeam();
            enabled = true;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            this.Unsubscribe();
        }

        private void OnPlayerJoined(PlayerJoined data)
        {
            var player = data.Player;
            if (player.TeamNumber != _playerDriver.TeamNumber)
            {
                return;
            }
            CheckMyTeam();
        }

        private void OnPlayerLeft(PlayerLeft data)
        {
            var player = data.Player;
            if (player.TeamNumber != _playerDriver.TeamNumber)
            {
                return;
            }
            CheckMyTeam();
        }

        private void CheckMyTeam()
        {
            StopAllCoroutines();
            var gameplayManager = Context.GameplayManager;
            var team = gameplayManager.GetBattleTeam(_playerDriver.TeamNumber);
            Debug.Log($"{_playerDriver} team {team}");
            if (team == null || team.PlayerCount < 2)
            {
                _playerTransform2 = null;
                _sqrCurrentDistance = 0;
                _currentVisibility = _isSinglePlayerShieldOn;
                _playerDriver.SetShieldVisibility(_currentVisibility);
                return;
            }
            var otherPlayer = team.GetMyTeamMember(_playerDriver.ActorNumber);
            _playerTransform2 = otherPlayer.PlayerTransform;
            StartCoroutine(TrackPlayers());
        }

        private IEnumerator TrackPlayers()
        {
            var delay = new WaitForFixedUpdate();
            _sqrCurrentDistance = Mathf.Abs((_playerTransform1.position - _playerTransform2.position).sqrMagnitude);
            _currentVisibility = _sqrCurrentDistance < _sqrMinVisibleDistance;
            _playerDriver.SetShieldVisibility(_currentVisibility);
            while (enabled)
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