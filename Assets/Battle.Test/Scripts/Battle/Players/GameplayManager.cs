using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Manager for gameplay activities where one or more players are involved.<br />
    /// Requires player's registration in order to participate.
    /// </summary>
    internal interface IGameplayManager
    {
        int PlayerCount { get; }

        IPlayerDriver GetPlayerByActorNumber(int actorNumber);

        void RegisterPlayer(IPlayerDriver playerDriver);

        void UnregisterPlayer(IPlayerDriver playerDriver);
    }

    internal class PlayerJoined
    {
        public readonly IPlayerDriver Player;

        public PlayerJoined(IPlayerDriver player)
        {
            Player = player;
        }
    }

    internal class PlayerLeft : PlayerJoined
    {
        public PlayerLeft(IPlayerDriver player) : base(player)
        {
        }
    }

    internal class TeamCreated
    {
        public readonly IPlayerDriver FirstPlayer;
        public readonly IPlayerDriver SecondPlayer;

        public TeamCreated(IPlayerDriver firstPlayer, IPlayerDriver secondPlayer)
        {
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
        }
    }

    internal class TeamBroken
    {
        public readonly IPlayerDriver PlayerWhoLeft;

        public TeamBroken(IPlayerDriver playerWhoLeft)
        {
            PlayerWhoLeft = playerWhoLeft;
        }
    }

    internal class GameplayManager : MonoBehaviour, IGameplayManager
    {
        private const string Tooltip1 = "Do not set player to 'Normal'/'Frozen' when ball travels over team gameplay areas";

        [Serializable]
        internal class DebugSettings
        {
            [Tooltip(Tooltip1)] public bool _isDisableTeamColliderTracking;
            public List<MonoBehaviour> _playerList;
        }

        public static IGameplayManager Get() => FindObjectOfType<GameplayManager>();

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private readonly HashSet<IPlayerDriver> _players = new();
        private readonly List<IPlayerDriver> _teamBlue = new();
        private readonly List<IPlayerDriver> _teamRed = new();

        private TeamColliderTracker _teamBlueTracker;
        private TeamColliderTracker _teamRedTracker;

        private void Awake()
        {
            Debug.Log($"{name}");
            if (_debug._isDisableTeamColliderTracking)
            {
                return;
            }
            var playArea = Context.GetPlayerPlayArea;
            _teamBlueTracker = playArea.BlueTeamCollider.gameObject.AddComponent<TeamColliderTracker>();
            _teamBlueTracker.TeamMembers = _teamBlue;
            _teamRedTracker = playArea.RedTeamCollider.gameObject.AddComponent<TeamColliderTracker>();
            _teamRedTracker.TeamMembers = _teamRed;
        }

        #region IGameplayManager

        int IGameplayManager.PlayerCount => _players.Count;

        IPlayerDriver IGameplayManager.GetPlayerByActorNumber(int actorNumber)
        {
            return _players.FirstOrDefault(x => x.ActorNumber == actorNumber);
        }

        void IGameplayManager.RegisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"add {playerDriver.NickName} pp={playerDriver.PlayerPos} actor={playerDriver.ActorNumber}");
            Assert.IsFalse(_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber),
                "_players.Count > 0 && _players.Any(x => x.ActorNumber == playerDriver.ActorNumber)");
            _players.Add(playerDriver);
            this.Publish(new PlayerJoined(playerDriver));
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue.Add(playerDriver);
                Assert.IsTrue(_teamBlue.Count <= 2, "_teamBlue.Count <= 2");
                if (_teamBlue.Count == 2)
                {
                    _teamBlue.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
                    this.Publish(new TeamCreated(_teamBlue[0], _teamBlue[1]));
                }
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed.Add(playerDriver);
                Assert.IsTrue(_teamRed.Count <= 2, "_teamRed.Count <= 2");
                if (_teamRed.Count == 2)
                {
                    _teamRed.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
                    this.Publish(new TeamCreated(_teamRed[0], _teamRed[1]));
                }
            }
            else
            {
                throw new UnityException($"Invalid team number {playerDriver.TeamNumber}");
            }
            UpdateDebugPlayerList();
        }

        void IGameplayManager.UnregisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"remove {playerDriver.NickName} pp={playerDriver.PlayerPos}");
            _players.Remove(playerDriver);
            this.Publish(new PlayerLeft(playerDriver));
            if (playerDriver.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                _teamBlue.Remove(playerDriver);
                if (_teamBlue.Count == 1)
                {
                    this.Publish(new TeamBroken(_teamBlue[0]));
                }
            }
            else if (playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _teamRed.Remove(playerDriver);
                if (_teamRed.Count == 1)
                {
                    this.Publish(new TeamBroken(_teamRed[0]));
                }
            }
            UpdateDebugPlayerList();
        }

        #endregion

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdateDebugPlayerList()
        {
            var playerList = _players.ToList();
            playerList.Sort((a, b) => a.PlayerPos.CompareTo(b.PlayerPos));
            _debug._playerList = playerList.Cast<MonoBehaviour>().ToList();
        }

        #endregion
    }

    internal class TeamColliderTracker : MonoBehaviour
    {
        public List<IPlayerDriver> TeamMembers { get; set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"trigger_enter {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            foreach (var playerDriver in TeamMembers)
            {
                playerDriver.SetPlayMode(BattlePlayMode.Frozen);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            Debug.Log($"trigger_exit {name} <- {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
            foreach (var playerDriver in TeamMembers)
            {
                playerDriver.SetPlayMode(BattlePlayMode.Normal);
            }
        }
    }
}