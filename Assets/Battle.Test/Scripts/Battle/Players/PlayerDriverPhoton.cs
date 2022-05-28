using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : MonoBehaviourPunCallbacks, IPlayerDriver, IPlayerActorCollision
    {
        [Serializable]
        internal class DebugSettings
        {
            public PlayerActor _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private PlayerActor _playerActorInstance;
        [SerializeField] private PlayerDriverState _state;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private int _playerPos;
        private int _teamNumber;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private bool _isLocal;
        private bool _isApplicationQuitting;

        private bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        public static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
        }

        private void Awake()
        {
            print("++");
            var player = photonView.Owner;
            _isLocal = player.IsLocal;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            _playerPos = PhotonBattle.GetPlayerPos(photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            var playerTag = $"{_playerPos}:{((IPlayerDriver)this).NickName}";
            name = name.Replace("Clone", playerTag);
            Application.quitting += () => _isApplicationQuitting = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
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
            _playerActor.Speed = _characterModel.Speed;
            _playerActor.CurrentResistance = _characterModel.Resistance;
            _state = gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(this, _characterModel);
            GameplayManager.Get().RegisterPlayer(this);
            ScoreFlashNet.RegisterEventListener();
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(_playerPos);
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
            GameplayManager.Get().UnregisterPlayer(this);
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
            _state.OnShieldCollision();
            var message = $"SHIELD {_playerPos}";
            var point = collision.GetContact(0).point;
            ScoreFlashNet.Push(message, point);
            Debug.Log($"SHIELD {name} contacts {collision.contactCount} {point}");
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _state.OnHeadCollision();
            var message = $"HEAD {_playerPos}";
            var point = collision.GetContact(0).point;
            ScoreFlashNet.Push(message, point);
            Debug.Log($"HEAD {name} contacts {collision.contactCount} {point}");
        }

        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => photonView.Owner.NickName;

        int IPlayerDriver.ActorNumber => photonView.Owner.ActorNumber;

        int IPlayerDriver.PlayerPos => _playerPos;

        int IPlayerDriver.TeamNumber => _teamNumber;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        bool IPlayerDriver.IsLocal => photonView.Owner.IsLocal;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Vector2 IPlayerDriver.Position => _playerActor.Position;

        IPlayerActorCollision IPlayerDriver.IPlayerActorCollision => this;

        void IPlayerDriver.Rotate(bool isUpsideDown)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestRotateRpc), RpcTarget.All, isUpsideDown);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            // NO IsNetworkSynchronize check!
            // - If input is configured to us, lets do it!
            photonView.RPC(nameof(TestMoveToRpc), RpcTarget.All, targetPosition);
        }

        void IPlayerDriver.SetCharacterPose(int poseIndex)
        {
            TestSetCharacterPoseRpc(poseIndex);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestSetCharacterPoseRpc), RpcTarget.Others, poseIndex);
        }

        void IPlayerDriver.SetPlayMode(BattlePlayMode playMode)
        {
            TestSetPlayModeRpc(playMode);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestSetPlayModeRpc), RpcTarget.Others, playMode);
        }

        void IPlayerDriver.SetShieldVisibility(bool state)
        {
            TestSetShieldVisibilityRpc(state);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestSetShieldVisibilityRpc), RpcTarget.Others, state);
        }

        void IPlayerDriver.SetShieldResistance(int resistance)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestSetShieldResistanceRpc), RpcTarget.All, resistance);
        }

        void IPlayerDriver.SetStunned(float duration)
        {
            TestSetStunnedRpc(duration);
            if (!IsNetworkSynchronize)
            {
                return;
            }
            photonView.RPC(nameof(TestSetStunnedRpc), RpcTarget.Others, duration);
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
    }
}