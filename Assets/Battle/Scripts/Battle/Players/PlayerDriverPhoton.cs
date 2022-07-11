using System;
using Altzone.Scripts.Battle;
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
    internal class PlayerDriverPhoton : PlayerDriver, IPlayerInfo, IPlayerDriver, IPlayerActorCollision
    {
        [Serializable]
        internal class DebugSettings
        {
            public PlayerActor _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private int _peerCount;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private IPlayerDriverState _state;
        private bool _isLocal;
        private bool _isApplicationQuitting;
        private bool _isDestroyed;
        private bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        private bool IsLocalPlayerSynchronize => _photonView.Owner.IsLocal;

        private void Awake()
        {
            print("++");
            _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            Debug.Log($"{player.GetDebugLabel()} {_photonView}");
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            var playerTag = $"{_playerPos}:{((IPlayerDriver)this).NickName}";
            name = name.Replace("Clone", playerTag);
            Application.quitting += () => _isApplicationQuitting = true;
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
            _playerActor = PlayerActor.InstantiatePrefabFor(this, _debug._playerPrefab);
            {
                // This code block should be shared with all PlayerDriver implementations
                _playerActor.Speed = _characterModel.Speed;
                _playerActor.CurrentResistance = _characterModel.Resistance;
                _state = GetPlayerDriverState(this);
                _state.ResetState(this, _characterModel);
                _state.CheckRotation(_playerActor.Transform.position);
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