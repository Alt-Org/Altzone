using System;
using System.Linq;
using Altzone.Scripts.Battle;
using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle.Players2
{
    /// <summary>
    /// Common base class for player actors so that FindObjectsOfType() for <c>PlayerActor</c> can be satisfied for all implementations.
    /// </summary>
    internal abstract class PlayerActor : MonoBehaviour
    {
        internal const int PlayModeNormal = 0;
        internal const int PlayModeFrozen = 1;
        internal const int PlayModeGhosted = 2;

        [Serializable]
        internal class PlayerState
        {
            public int _currentMode;
            public Transform _transform;
            public int _playerPos;
            public int _teamNumber;
            public PlayerActor _teamMate;

            public void InitState(Transform transform, Player player)
            {
                _currentMode = PlayModeNormal;
                _transform = transform;
                _playerPos = PhotonBattle.GetPlayerPos(player);
                _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            }

            public void FindTeamMember()
            {
                var players = FindObjectsOfType<PlayerActor>();
                _teamMate = players
                    .FirstOrDefault(x => x.TeamNumber == _teamNumber && x.PlayerPos != _playerPos);
            }
        }

        [SerializeField] protected PlayerState _state;

        // These are public in IPlayerActor but for simplicity we have them here as internal aliases.
        protected int PlayerPos => _state._playerPos;
        protected int TeamNumber => _state._teamNumber;

        protected PlayerActor TeamMate => _state._teamMate;

        public void ConnectWith(PlayerActor other)
        {
            _state._teamMate = other;
        }
    }
}