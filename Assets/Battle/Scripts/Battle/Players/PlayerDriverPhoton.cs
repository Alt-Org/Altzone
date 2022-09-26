using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Battle.Scripts.Ui;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : PlayerDriver, IPlayerDriver, IPlayerActorCollision
    {
        [Serializable]
        internal class DebugSettings
        {
            public PlayerActorBase _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private int _peerCount;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;
        private double _movementDelay;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private IPlayerDriverState _state;
        private IGridManager _gridManager;
        private bool _isLocal;
        private bool _isApplicationQuitting;
        private bool _isDestroyed;
        public bool IsMoving { get; set; }
        private static bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        private void Awake()
        {
            print("++");
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager;
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            Debug.Log($"{player.GetDebugLabel()} {_photonView}");
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            var playerTag = $"{_playerPos}:{((IPlayerDriver)this).NickName}";
            name = name.Replace("Clone", playerTag);
            Application.quitting += () => _isApplicationQuitting = true;
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var variables = runtimeGameConfig.Variables;
            _movementDelay = variables._playerMovementNetworkDelay;
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {_photonView}");
            if (!PhotonBattle.IsRealPlayer(player))
            {
                enabled = false;
                return;
            }
            if (_playerActor != null)
            {
                // Should not be enabled twice!
                return;
            }
            _characterModel = PhotonBattle.GetCharacterModelForRoom(player);
            _playerActor = PlayerActorBase.InstantiatePrefabFor(this, _characterModel.MainDefence, _debug._playerPrefab);
            {
                // This code block should be shared with all PlayerDriver implementations
                _playerActor.Speed = _characterModel.Speed;
                _playerActor.CurrentResistance = _characterModel.Resistance;
                _state = GetPlayerDriverState(this);
                var playerWorldPosition = _state.ResetState(this, _playerActor, _characterModel, _playerActor.Transform.position);
                _state.CheckRotation(playerWorldPosition);
                ConnectDistanceMeter(this, GetComponent<PlayerDistanceMeter>());
            }
            var gameplayManager = Context.PlayerManager;
            gameplayManager.RegisterPlayer(this);
            _peerCount = 0;
           this.ExecuteOnNextFrame(() =>
            {
                // PeerCount handshake protocol
                Debug.Log($"SEND SendMyPeerCountRpc {this} pos {_playerPos} local {_isLocal} : {_peerCount} ->");
                _photonView.RPC(nameof(SendPlayerPeerCountRpc), RpcTarget.All);
            });
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetBattlePlayArea.GetPlayerPlayArea(_playerPos);
            playerInputHandler.SetPlayerDriver(this, _playerActor.Transform, playArea);
        }

        private void OnDestroy()
        {
            if (_isApplicationQuitting || _playerActor == null)
            {
                return;
            }
            _isDestroyed = true;
            print("xx");
            Debug.Log($"{name}");
            DisconnectDistanceMeter(this, GetComponent<PlayerDistanceMeter>());
            var gameplayManager = Context.PlayerManager;
            gameplayManager?.UnregisterPlayer(this, _playerActor.GameObject);
            _playerActor.ResetPlayerDriver();
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            playerInputHandler?.ResetPlayerDriver();
        }

        #region IPlayerActorCollision

        void IPlayerActorCollision.OnShieldCollision(Collision2D collision)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            // This call can invalidate current collider!
            _state.OnShieldCollision(out var hitType);
            this.Publish(new UiEvents.ShieldCollision(collision, this, hitType));
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            // This call can invalidate current collider!
            _state.OnHeadCollision();
            this.Publish(new UiEvents.HeadCollision(collision, this));
            this.Publish(new UiEvents.RestartBattle(this));
        }

        #endregion

        #region IPlayerInfo (readonly)

        int IPlayerInfo.ActorNumber => _photonView.Owner.ActorNumber;

        int IPlayerInfo.TeamNumber => _teamNumber;

        bool IPlayerInfo.IsLocal => _photonView.Owner.IsLocal;

        Vector2 IPlayerInfo.Position => _playerActor.Transform.position;

        double IPlayerInfo.LastBallHitTime => _state.LastBallHitTime;
        
        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => _photonView.Owner.NickName;

        bool IPlayerDriver.IsValid => !_isDestroyed && _playerActor != null;

        int IPlayerDriver.PeerCount => _peerCount;

        int IPlayerDriver.PlayerPos => _playerPos;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Transform IPlayerDriver.PlayerTransform => _playerActor.Transform;

        BattlePlayMode IPlayerDriver.BattlePlayMode => _playerActor.BattlePlayMode;

        IPlayerActorCollision IPlayerDriver.PlayerActorCollision => this;

        void IPlayerDriver.Rotate(bool isUpsideDown)
        {
            // NO IsNetworkSynchronize check!
            // - rotation is based on initial player position Y coordinate.
            _photonView.RPC(nameof(RotatePlayerRpc), RpcTarget.All, isUpsideDown);
        }

        void IPlayerDriver.FixCameraRotation(Camera gameCamera)
        {
            // This is just local operation!
            _playerActor.FixCameraRotation(gameCamera);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            // NO IsNetworkSynchronize check!
            // - If input is configured to us, lets do it!
            _photonView.RPC(nameof(MovePlayerToRpc), RpcTarget.All, targetPosition);
        }

        void IPlayerDriver.SendMoveRequest(GridPos gridPos)
        {
            if (IsMoving) { return; }
            _photonView.RPC(nameof(ProcessMoveRequestRpc), RpcTarget.MasterClient, gridPos.Row, gridPos.Col);
        }

        void IPlayerDriver.SetCharacterPose(int poseIndex)
        {
            SetPlayerCharacterPoseRpc(poseIndex);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerCharacterPoseRpc), RpcTarget.Others, poseIndex);
        }

        void IPlayerDriver.SetPlayMode(BattlePlayMode playMode)
        {
            SetPlayerPlayModeRpc(playMode);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerPlayModeRpc), RpcTarget.Others, playMode);
        }

        void IPlayerDriver.SetShieldVisibility(bool state)
        {
            SetPlayerShieldVisibilityRpc(state);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerShieldVisibilityRpc), RpcTarget.Others, state);
        }

        void IPlayerDriver.SetShieldResistance(int resistance)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerShieldResistanceRpc), RpcTarget.All, resistance);
        }

        void IPlayerDriver.SetStunned(float duration)
        {
            SetPlayerStunnedRpc(duration);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerStunnedRpc), RpcTarget.Others, duration);
        }

        void IPlayerDriver.PlayerActorDestroyed()
        {
            if (_isDestroyed)
            {
                return;
            }
            Debug.Log($"{name}");
            _playerActor = null;
            DisconnectDistanceMeter(this, GetComponent<PlayerDistanceMeter>());
        }

        void IPlayerDriver.SetSpaceFree(GridPos gridPos)
        {
            _photonView.RPC(nameof(SetSpaceFreeRpc), RpcTarget.MasterClient, gridPos.Row, gridPos.Col);
        }

        void IPlayerDriver.SetSpaceTaken(GridPos gridPos)
        {
            _photonView.RPC(nameof(SetSpaceTakenRpc), RpcTarget.MasterClient, gridPos.Row, gridPos.Col);
        }
        #endregion

        #region Photon RPC

        // NOTE! When adding new RPC method check that the name is unique in PhotonServerSettings Rpc List!

        [PunRPC]
        private void RotatePlayerRpc(bool isUpsideDown)
        {
            _playerActor.Rotate(isUpsideDown);
        }

        [PunRPC]
        private void MovePlayerToRpc(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }

        [PunRPC]
        private void SetPlayerCharacterPoseRpc(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        [PunRPC]
        private void SetPlayerPlayModeRpc(BattlePlayMode playMode)
        {
            _playerActor.SetPlayMode(playMode);
        }

        [PunRPC]
        private void SetPlayerShieldVisibilityRpc(bool state)
        {
            _playerActor.SetShieldVisibility(state);
        }

        [PunRPC]
        void SetPlayerShieldResistanceRpc(int resistance)
        {
            _playerActor.CurrentResistance = resistance;
        }

        [PunRPC]
        private void SetPlayerStunnedRpc(float duration)
        {
            _playerActor.SetBuff(PlayerBuff.Stunned, duration);
        }

        [PunRPC]
        private void ProcessMoveRequestRpc(int row, int col, PhotonMessageInfo info)
        {
            if (!_gridManager._gridEmptySpaces[row, col])
            {
                Debug.Log($"Grid check failed. row: {row}, col: {col}");
                return;
            }
            var movementDelay = info.SentServerTime + _movementDelay - PhotonNetwork.Time;
            Debug.Log($"Grid Request approved: row: {row}, col: {col}, player: {info.Sender}, time: {movementDelay}");
            _photonView.RPC(nameof(SetSpaceTakenRpc), RpcTarget.All, row, col);
            _photonView.RPC(nameof(MoveDelayedRpc), info.Sender, row, col, movementDelay);
        }

        [PunRPC]
        private void MoveDelayedRpc(int row, int col, double movementStartTime)
        {
            IsMoving = true;
            GridPos gridPos = new GridPos(row, col);
            _state.DelayedMove(gridPos, movementStartTime);
        }

        [PunRPC]
        private void SetSpaceTakenRpc(int row, int col)
        {
            _gridManager._gridEmptySpaces[row, col] = false;
            Debug.Log($"Grid space taken: row: {row}, col: {col}, {_gridManager._gridEmptySpaces[row, col]}");
        }

        [PunRPC]
        public void SetSpaceFreeRpc(int row, int col)
        {
            _gridManager._gridEmptySpaces[row, col] = true;
            Debug.Log($"Grid space free: row: {row}, col: {col}, {_gridManager._gridEmptySpaces[row, col]}");
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetSpaceFreeRpc), RpcTarget.Others, row, col);
            }
        }

        #endregion

        #region Photon RPC for PeerCount handshake protocol

        [PunRPC]
        private void SendPlayerPeerCountRpc()
        {
            Debug.Log($"{this} pos {_playerPos} local {_isLocal} : {_peerCount} <- {_peerCount + 1}");
            _peerCount += 1;
            var gameplayManager = Context.PlayerManager;
            gameplayManager.UpdatePeerCount(this);
        }

        #endregion
    }
}
