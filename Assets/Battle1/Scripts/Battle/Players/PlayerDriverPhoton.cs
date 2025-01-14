
using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Battle1.Scripts.Battle.Game;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;*/

namespace Battle1.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : MonoBehaviour, IPlayerDriver
    {
        // Serialized Fields
        [Header("Testing")]
        [SerializeField, Tooltip("Deprecated")] [Obsolete("Deprecated")] private bool _isTesting = false;
        [SerializeField, Tooltip("Deprecated")] [Obsolete("Deprecated")] private CharacterID _playerCharacterID;

        // { Public Properties and Fields

        public string PlayerName;
        /*public string NickName => _photonView.Owner.NickName;*/
       /* public bool IsLocal => _photonView.Owner.IsLocal;
        public int ActorNumber => _photonView.Owner.ActorNumber;*/
        public string NickName => "_photonView.Owner.NickName";
        public bool IsLocal => true;
        public int ActorNumber => 0;

        public bool MovementEnabled
        {
            get => _state.MovementEnabled;
            set
            {
                _state.MovementEnabled = value;
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement set to {3}", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition, _state.MovementEnabled));
            }
        }

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public int PeerCount => _peerCount;

        // } Public Properties and Fields

        #region Public Methods

        public void MoveTo(Vector2 targetPosition)
        {
            // debug
            const string DEBUG_LOG_MOVEMENT_DENIED = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement denied";

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement requested", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition));
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
            if (!_state.MovementEnabled || !_state.CanRequestMove)
            {
                Debug.Log(string.Format(DEBUG_LOG_MOVEMENT_DENIED, _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition));
                return;
            }
            _state.IsWaitingToMove(true);
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);

            Transform shieldTransform = _battlePlayer.PlayerShieldManager.transform;
            Vector2 position = new(shieldTransform.position.x, shieldTransform.position.y);
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            GridPos targetGridPos = _gridManager.ClampGridPosition(_gridManager.WorldPointToGridPosition(targetPosition));

            Debug.Log(string.Format(
                DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement info (current position: {3}, current grid position: ({4}), target position: {5}, target grid position: ({6}))",
                _syncedFixedUpdateClock.UpdateCount,
                _teamNumber,
                _playerPosition,
                position,
                gridPos,
                targetPosition,
                targetGridPos
            ));

            if (targetGridPos.Equals(gridPos) || !_gridManager.IsMovementGridSpaceFree(targetGridPos, _teamNumber))
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Invalid movement destination", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition));
                Debug.Log(string.Format(DEBUG_LOG_MOVEMENT_DENIED, _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition));
                _state.IsWaitingToMove(false);
                _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
                return;
            }

            float distance = (targetPosition - position).magnitude;
            double movementTimeS = Math.Max(distance / _playerMovementSpeed, _movementMinTimeS);
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS);
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Sending player movement network message", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPosition));
           /* _photonView.RPC(nameof(MoveRpc), RpcTarget.All, targetGridPos.Row, targetGridPos.Col, teleportUpdateNumber);*/
        }

        #endregion Public Methods

        // Player Data
        private BattleTeamNumber _teamNumber;
        private int _playerPosition;

        // Config
        private float _playerMovementSpeed;
        private double _movementMinTimeS;
        private float _arenaScaleFactor;

        private PlayerDriverState _state;

        private int _peerCount;
       /* private static bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;*/

        private BattlePlayer _battlePlayer;

        // Components
       /* private PhotonView _photonView;*/

        // Important Objects
        private PlayerManager _playerManager;
        private GridManager _gridManager;
        private PlayerPlayArea _battlePlayArea;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER DRIVER PHOTON] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO = DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";

        private void Awake()
        {
           /* _photonView = PhotonView.Get(this);*/

            // get important objects
            _playerManager = Context.GetPlayerManager;
            _gridManager = Context.GetGridManager;
            _battlePlayArea = Context.GetBattlePlayArea;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // get player data
           /* _playerPosition = PhotonBattle.GetPlayerPos(_photonView.Owner);*/
          /*  _teamNumber = PhotonBattle.GetTeamNumber(_playerPosition);*/

            // create battle player
            {
              /*  BattleCharacter battleCharacter = PhotonBattle.GetBattleCharacter(_photonView.Owner);*/

               /* _battlePlayer = new BattlePlayer(_playerPosition, battleCharacter, false,  this);*/
            }


            // get config
            float playerMoveSpeedMultiplier = GameConfig.Get().Variables._playerMoveSpeedMultiplier;
            _movementMinTimeS = GameConfig.Get().Variables._networkDelay;
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;

         /*   InstantiatePlayerPrefab(_photonView.Owner);*/

            _playerMovementSpeed = _battlePlayer.PlayerActor.MovementSpeed * playerMoveSpeedMultiplier;

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        }

        private void OnEnable()
        {
           /* Player player = _photonView.Owner;*/
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_battlePlayer.PlayerActor, _playerPosition, _teamNumber);
            _state.MovementEnabled = false;

            _playerManager.RegisterPlayer(_battlePlayer, _teamNumber);
            _peerCount = 0;
           /* this.ExecuteOnNextFrame(() => _photonView.RPC(nameof(SendPlayerPeerCountRpc), RpcTarget.All));*/
        }

        #region Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _battlePlayer.PlayerActor.SetRotation(_teamNumber == BattleTeamNumber.TeamAlpha ? 0 : 180f);
            _battlePlayer.PlayerActor.SetSpriteVariant(data.LocalPlayer.BattleTeam.TeamNumber == _teamNumber ? PlayerActor.SpriteVariant.A : PlayerActor.SpriteVariant.B);
            _battlePlayer.PlayerActor.ResetSprite();
        }
        #endregion Message Listeners

        private void InstantiatePlayerPrefab(Player player)
        {
            string playerTag = $"{_teamNumber}:{_playerPosition}:{player.NickName}";
            PlayerName = playerTag;
            name = name.Replace("Clone", playerTag);

            Debug.Log(string.Format("Selected player {0}, {1}", _battlePlayer.BattleCharacter.CharacterID, CustomCharacter.GetCharacterClassAndName(_battlePlayer.BattleCharacter.CharacterID)));
            PlayerActor.InstantiatePrefabFor(_battlePlayer, playerTag, _arenaScaleFactor);
        }

        #region Photon RPC

        /*[PunRPC]
        private void SendPlayerPeerCountRpc()
        {
            _peerCount += 1;
            _playerManager.UpdatePeerCount();
        }

        [PunRPC]
        private void MoveRpc(int row, int col, int teleportUpdateNumber)
        {
            var gridPos = new GridPos(row, col);

            // debug
            Transform shieldTransform = _battlePlayer.PlayerShieldManager.transform;
            GridPos debugGridPos = _gridManager.WorldPointToGridPosition(new Vector2(shieldTransform.position.x, shieldTransform.position.y));

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Received player movement network message (current grid position: ({3}), target grid position: ({4}))",
                _syncedFixedUpdateClock.UpdateCount,
                _teamNumber,
                _playerPosition,
                debugGridPos,
                gridPos
            ));

            _playerManager.ReportMovement(teleportUpdateNumber);
            _state.IsWaitingToMove(true);
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
            _state.Move(gridPos, teleportUpdateNumber);
        }*/

        #endregion Photon RPC
    }
}
