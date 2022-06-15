using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Test;
using Photon.Pun;
using Prg.Scripts.Common.Unity.ToastMessages;
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
            public PlayerActor _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private PlayerActor _playerActorInstance;
        [SerializeField] private int _peerCount;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;
        private char _playerPosChar;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private IPlayerDriverState _state;
        private Transform _playerActorTransform;
        private IGameplayManager _gameplayManager;
        private bool _isLocal;
        private bool _isApplicationQuitting;

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
            _playerPosChar = new[] { '?', 'A', 'B', 'C', 'D' }[_playerPos];
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
            _playerActorInstance = PlayerActor.Instantiate(this, _debug._playerPrefab);
            _playerActor = _playerActorInstance;
            _playerActorTransform = _playerActorInstance.GetComponent<Transform>();
            _playerActor.Speed = _characterModel.Speed;
            _playerActor.CurrentResistance = _characterModel.Resistance;
            _state = GetPlayerDriverState();
            _state.ResetState(this, _characterModel);
            _state.CheckRotation(_playerActorTransform.position);
            _gameplayManager = GameplayManager.Get();
            _gameplayManager.RegisterPlayer(this);
            ScoreFlashNet.RegisterEventListener();
            this.ExecuteOnNextFrame(() =>
            {
                // PeerCount handshake protocol
                Debug.Log($"SEND SendMyPeerCountRpc {this} pos {_playerPos} local {_isLocal} : {_peerCount} ->");
                _photonView.RPC(nameof(TestSendMyPeerCountRpc), RpcTarget.All);
            });
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetBattlePlayArea.GetPlayerPlayArea(_playerPos);
            playerInputHandler.SetPlayerDriver(this, _playerActorInstance.GetComponent<Transform>(), playArea);
        }

        private void OnDestroy()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            print("xx");
            Debug.Log($"{name}");
            _gameplayManager.UnregisterPlayer(this, _playerActorInstance.gameObject);
            if (_playerActor == null)
            {
                // Did not manage to initialize properly?
                return;
            }
            _playerActor.ResetPlayerDriver();
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            playerInputHandler?.ResetPlayerDriver();
        }

        #region IPlayerActorCollision

        void IPlayerActorCollision.OnShieldCollision(Collision2D collision, MonoBehaviour component)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            var contactCount = collision.contactCount;
            var point = collision.GetContact(0).point;
            // This call can invalidate current collider!
            var hitType = _state.OnShieldCollision();
            var message = $"{hitType} {_playerPosChar}";
            ScoreFlashNet.Push(message, point);
            Debug.Log($"{hitType} {name} {component.name} contacts {contactCount} {point}");
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            var contactCount = collision.contactCount;
            var point = collision.GetContact(0).point;
            // This call can invalidate current collider!
            _state.OnHeadCollision();
            var message = $"HEAD {_playerPosChar}";
            ScoreFlashNet.Push(message, point);
            Debug.Log($"HEAD {name} contacts {contactCount} {point}");
        }

        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => _photonView.Owner.NickName;

        int IPlayerDriver.ActorNumber => _photonView.Owner.ActorNumber;

        int IPlayerDriver.PeerCount => _peerCount;

        int IPlayerDriver.PlayerPos => _playerPos;

        int IPlayerDriver.TeamNumber => _teamNumber;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        bool IPlayerDriver.IsLocal => _photonView.Owner.IsLocal;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Vector2 IPlayerDriver.Position => _playerActorTransform.position;

        Transform IPlayerDriver.PlayerTransform => _playerActorTransform;

        IPlayerActorCollision IPlayerDriver.PlayerActorCollision => this;

        void IPlayerDriver.Rotate(bool isUpsideDown)
        {
            // NO IsNetworkSynchronize check!
            // - rotation is based on initial player position Y coordinate.
            _photonView.RPC(nameof(TestRotateRpc), RpcTarget.All, isUpsideDown);
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
            _photonView.RPC(nameof(TestMoveToRpc), RpcTarget.All, targetPosition);
        }

        void IPlayerDriver.SetCharacterPose(int poseIndex)
        {
            TestSetCharacterPoseRpc(poseIndex);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetCharacterPoseRpc), RpcTarget.Others, poseIndex);
        }

        void IPlayerDriver.SetPlayMode(BattlePlayMode playMode)
        {
            TestSetPlayModeRpc(playMode);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetPlayModeRpc), RpcTarget.Others, playMode);
        }

        void IPlayerDriver.SetShieldVisibility(bool state)
        {
            TestSetShieldVisibilityRpc(state);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetShieldVisibilityRpc), RpcTarget.Others, state);
        }

        void IPlayerDriver.SetShieldResistance(int resistance)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetShieldResistanceRpc), RpcTarget.All, resistance);
        }

        void IPlayerDriver.SetStunned(float duration)
        {
            TestSetStunnedRpc(duration);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetStunnedRpc), RpcTarget.Others, duration);
        }

        void IPlayerDriver.StopAndRestartBall()
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            var startTheBallTest = FindObjectOfType<StartTheBallTest>();
            ScoreFlashNet.Push(startTheBallTest != null ? "RESTART" : "CAN NOT RESTART");
            if (startTheBallTest != null)
            {
                startTheBallTest.RestartBallInGame(this);
            }
        }

        #endregion

        #region Photon RPC

        // NOTE! When adding new RPC method check that the name is unique in PhotonServerSettings Rpc List!

        [PunRPC]
        private void TestRotateRpc(bool isUpsideDown)
        {
            _playerActor.Rotate(isUpsideDown);
        }

        [PunRPC]
        private void TestMoveToRpc(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }

        [PunRPC]
        private void TestSetCharacterPoseRpc(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        [PunRPC]
        private void TestSetPlayModeRpc(BattlePlayMode playMode)
        {
            _playerActor.SetPlayMode(playMode);
        }

        [PunRPC]
        private void TestSetShieldVisibilityRpc(bool state)
        {
            _playerActor.SetShieldVisibility(state);
        }

        [PunRPC]
        void TestSetShieldResistanceRpc(int resistance)
        {
            _playerActor.CurrentResistance = resistance;
        }

        [PunRPC]
        private void TestSetStunnedRpc(float duration)
        {
            _playerActor.SetBuff(PlayerBuff.Stunned, duration);
        }

        #endregion

        #region Photon RPC for PeerCount handshake protocol

        [PunRPC]
        private void TestSendMyPeerCountRpc()
        {
            Debug.Log($"{this} pos {_playerPos} local {_isLocal} : {_peerCount} <- {_peerCount + 1}");
            _peerCount += 1;
            _gameplayManager.UpdatePeerCount(this);
        }

        #endregion
    }
}