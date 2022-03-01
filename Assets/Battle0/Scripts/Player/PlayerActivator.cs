using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Battle0.Scripts.interfaces;
using Photon.Pun;
using UnityEngine;

namespace Battle0.Scripts.Player
{
    /// <summary>
    /// Helper to collect essential player data before all players can be enabled for the game play.
    /// </summary>
    public class PlayerActivator : MonoBehaviour
    {
        public static readonly List<IPlayerActor> AllPlayerActors = new List<IPlayerActor>();
        public static int HomeTeamNumber;
        public static int LocalTeamNumber;

        [Header("Live Data")] public int _playerPos;
        public int _teamNumber;
        public bool _isLocal;
        public int _oppositeTeamNumber;
        public int _teamMatePos;
        public bool _isAwake;

        private void Awake()
        {
            var photonView = PhotonView.Get(this);
            var player = photonView.Owner;
            _playerPos = PhotonBattle.GetPlayerPos(player);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            _isLocal = photonView.IsMine;
            _oppositeTeamNumber = PhotonBattle.GetOppositeTeamNumber(_teamNumber);
            _teamMatePos = PhotonBattle.GetTeamMatePos(_playerPos);
            if (player.IsMasterClient)
            {
                // The player who created this room is in "home team"!
                HomeTeamNumber = _teamNumber;
                Debug.Log($"HomeTeamNumber={HomeTeamNumber} pos={_playerPos}");
            }
            if (_isLocal)
            {
                LocalTeamNumber = _teamNumber;
                Debug.Log($"LocalTeamNumber={LocalTeamNumber} pos={_playerPos}");
            }
            Debug.Log($"Awake {player.NickName} pos={_playerPos} team={_teamNumber}");

            _isAwake = true; // Signal that we have configured ourself
        }
    }
}