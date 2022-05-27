using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Manager for gameplay activities where one or more players are involved.<br />
    /// Requires registration in order to participate.
    /// </summary>
    internal interface IGameplayManager
    {
        void RegisterPlayer(IPlayerDriver playerDriver);

        void UnregisterPlayer(IPlayerDriver playerDriver);
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
        [Serializable]
        internal class DebugSettings
        {
            public List<MonoBehaviour> _playerList;
        }

        public static IGameplayManager Get() => FindObjectOfType<GameplayManager>();

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private readonly HashSet<IPlayerDriver> _players = new();
        private readonly List<IPlayerDriver> _teamBlue = new();
        private readonly List<IPlayerDriver> _teamRed = new();

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        #region IGameplayManager

        void IGameplayManager.RegisterPlayer(IPlayerDriver playerDriver)
        {
            Debug.Log($"add {playerDriver.NickName} pp={playerDriver.PlayerPos}");
            _players.Add(playerDriver);
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
}