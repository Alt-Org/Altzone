using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    internal class PlayerDriverStatic : MonoBehaviour, IPlayerDriver, IPlayerActorCollision
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
        [SerializeField] private PlayerActor _playerActorInstance;
        [SerializeField] private PlayerDriverState _state;

        private CharacterModel _characterModel;

        private IPlayerActor _playerActor;
        private bool _isApplicationQuitting;

        private void Awake()
        {
            print("++");
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(_settings._playerPos), "PhotonBattle.IsValidGameplayPos(_playerPos)");
            Application.quitting += () => _isApplicationQuitting = true;
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            var gameplayManager = GameplayManager.Get();
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }
            _actorNumber = -(gameplayManager.PlayerCount + 1);
            _characterModel = Storefront.Get().GetCharacterModel((int)_settings._playerMainSkill);
            _playerActorInstance = PlayerActor.Instantiate(this, _settings._playerPrefab);
            _playerActor = _playerActorInstance;
            _playerActor.Speed = _characterModel.Speed;
            _playerActor.CurrentResistance = _characterModel.Resistance;
            _state = gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(this, _characterModel);
            gameplayManager.RegisterPlayer(this);
            if (!_settings._isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(_settings._playerPos);
            playerInputHandler.SetPlayerDriver(this, _playerActorInstance.GetComponent<Transform>(), playArea);
        }

        private void OnDestroy()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            print("xx");
            GameplayManager.Get().UnregisterPlayer(this);
            if (_settings._isLocal)
            {
                var playerInputHandler = PlayerInputHandler.Get();
                playerInputHandler?.ResetPlayerDriver();
            }
        }

        #region IPlayerActorCollision

        void IPlayerActorCollision.OnShieldCollision(Collision2D collision)
        {
            _state.OnShieldCollision();
            var message = $"SHIELD {_settings._playerPos}";
            var point = collision.GetContact(0).point;
            ScoreFlash.Push(message, point);
            Debug.Log($"SHIELD {name} contacts {collision.contactCount} {point}");
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            _state.OnHeadCollision();
            var message = $"HEAD {_settings._playerPos}";
            var point = collision.GetContact(0).point;
            ScoreFlash.Push(message, point);
            Debug.Log($"HEAD {name} contacts {collision.contactCount} {point}");
        }

        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => _settings._nickName;

        int IPlayerDriver.ActorNumber => _actorNumber;

        int IPlayerDriver.PlayerPos => _settings._playerPos;

        int IPlayerDriver.TeamNumber => _settings._teamNumber;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        bool IPlayerDriver.IsLocal => _settings._isLocal;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Vector2 IPlayerDriver.Position => _playerActor.Position;

        IPlayerActorCollision IPlayerDriver.PlayerActorCollision => this;

        void IPlayerDriver.Rotate(bool isUpsideDown)
        {
            _playerActor.Rotate(isUpsideDown);
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

        #endregion
    }
}