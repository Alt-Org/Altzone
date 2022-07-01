using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Static <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverStatic : PlayerDriver, IPlayerInfo, IPlayerDriver, IPlayerActorCollision
    {
        [Serializable]
        internal class Settings
        {
            public string _nickName;
            public int _playerPos = PhotonBattle.PlayerPosition1;
            public int _teamNumber = PhotonBattle.TeamBlueValue;
            public Defence _playerMainSkill = Defence.Deflection;
            public PlayerActor _playerPrefab;
            public bool _isLocal;
        }

        [Header("Settings"), SerializeField] private Settings _settings;

        [Header("Live Data"), SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField] private Transform _playerActorTransform;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private IPlayerDriverState _state;
        private bool _isApplicationQuitting;
        private bool _isDestroyed;

        private void Awake()
        {
            print("++");
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(_settings._playerPos), "PhotonBattle.IsValidGameplayPos(_playerPos)");
            Application.quitting += () => _isApplicationQuitting = true;
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }
            var gameplayManager = Context.PlayerManager;
            _actorNumber = -(gameplayManager.PlayerCount + 1);
            _characterModel = Storefront.Get().GetCharacterModel((int)_settings._playerMainSkill);
            var playerActorInstance = PlayerActor.InstantiatePrefabFor(this, _settings._playerPrefab);
            {
                // This code block should be shared with all PlayerDriver implementations
                _playerActor = playerActorInstance;
                _playerActorTransform = playerActorInstance.GetComponent<Transform>();
                _playerActor.Speed = _characterModel.Speed;
                _playerActor.CurrentResistance = _characterModel.Resistance;
                _state = GetPlayerDriverState(this);
                _state.ResetState(this, _characterModel);
                _state.CheckRotation(_playerActorTransform.position);
                ConnectDistanceMeter(this, GetComponent<PlayerDistanceMeter>());
            }
            gameplayManager.RegisterPlayer(this);
            if (!_settings._isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetBattlePlayArea.GetPlayerPlayArea(_settings._playerPos);
            playerInputHandler.SetPlayerDriver(this, _playerActorTransform, playArea);
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
            if (!_settings._isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            playerInputHandler?.ResetPlayerDriver();
        }

        #region IPlayerActorCollision

        void IPlayerActorCollision.OnShieldCollision(Collision2D collision)
        {
            // This call can invalidate current collider!
            _state.OnShieldCollision(out var hitType);
            this.Publish(new UiEvents.ShieldCollision(collision, this, hitType));
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            // This call can invalidate current collider!
            _state.OnHeadCollision();
            this.Publish(new UiEvents.HeadCollision(collision, this));
            this.Publish(new UiEvents.RestartBattle(this));
        }

        #endregion

        #region IPlayerInfo (readonly)

        int IPlayerInfo.ActorNumber => _actorNumber;

        int IPlayerInfo.TeamNumber => _settings._teamNumber;

        bool IPlayerInfo.IsLocal => _settings._isLocal;

        Vector2 IPlayerInfo.Position => _playerActorTransform.position;

        double IPlayerInfo.LastBallHitTime => _state.LastBallHitTime;

        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => _settings._nickName;

        int IPlayerDriver.PeerCount => 0;

        bool IPlayerDriver.IsValid => !_isDestroyed && _playerActor != null;

        int IPlayerDriver.PlayerPos => _settings._playerPos;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Transform IPlayerDriver.PlayerTransform => _playerActorTransform;

        BattlePlayMode IPlayerDriver.BattlePlayMode => _playerActor.BattlePlayMode;

        IPlayerActorCollision IPlayerDriver.PlayerActorCollision => this;

        void IPlayerDriver.Rotate(bool isUpsideDown)
        {
            _playerActor.Rotate(isUpsideDown);
        }

        void IPlayerDriver.FixCameraRotation(Camera gameCamera)
        {
            _playerActor.FixCameraRotation(gameCamera);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }

        void IPlayerDriver.SetCharacterPose(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        void IPlayerDriver.SetPlayMode(BattlePlayMode playMode)
        {
            _playerActor.SetPlayMode(playMode);
        }

        void IPlayerDriver.SetShieldVisibility(bool state)
        {
            _playerActor.SetShieldVisibility(state);
        }

        void IPlayerDriver.SetShieldResistance(int resistance)
        {
            _playerActor.CurrentResistance = resistance;
        }

        void IPlayerDriver.SetStunned(float duration)
        {
            _playerActor.SetBuff(PlayerBuff.Stunned, duration);
        }

        void IPlayerDriver.PlayerActorDestroyed()
        {
            if (_isDestroyed)
            {
                return;
            }
            Debug.Log($"{name}");
            _playerActor = null;
            _playerActorTransform = null;
            DisconnectDistanceMeter(this, GetComponent<PlayerDistanceMeter>());
        }

        #endregion
    }
}