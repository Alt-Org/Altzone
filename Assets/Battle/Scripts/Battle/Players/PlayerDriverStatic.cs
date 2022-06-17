using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Test;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Static <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverStatic : PlayerDriver, IPlayerDriver, IPlayerActorCollision
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

        private char _playerPosChar;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;
        private IPlayerDriverState _state;
        private Transform _playerActorTransform;
        private bool _isApplicationQuitting;

        private void Awake()
        {
            print("++");
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(_settings._playerPos), "PhotonBattle.IsValidGameplayPos(_playerPos)");
            Application.quitting += () => _isApplicationQuitting = true;
            _playerPosChar = new[] { '?', 'A', 'B', 'C', 'D' }[_settings._playerPos];
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }
            var gameplayManager = Context.GameplayManager;
            _actorNumber = -(gameplayManager.PlayerCount + 1);
            _characterModel = Storefront.Get().GetCharacterModel((int)_settings._playerMainSkill);
            _playerActorInstance = PlayerActor.InstantiatePrefabFor(this, _settings._playerPrefab);
            {
                // This code block should be shared with all PlayerDriver implementations
                _playerActor = _playerActorInstance;
                _playerActorTransform = _playerActorInstance.GetComponent<Transform>();
                _playerActor.Speed = _characterModel.Speed;
                _playerActor.CurrentResistance = _characterModel.Resistance;
                _state = GetPlayerDriverState(this);
                _state.ResetState(this, _characterModel);
                _state.CheckRotation(_playerActorTransform.position);
                ConnectDistanceMeter(this, _playerActorTransform);
            }
            gameplayManager.RegisterPlayer(this);
            if (!_settings._isLocal)
            {
                return;
            }
            var playerInputHandler = PlayerInputHandler.Get();
            var playArea = Context.GetBattlePlayArea.GetPlayerPlayArea(_settings._playerPos);
            playerInputHandler.SetPlayerDriver(this, _playerActorInstance.GetComponent<Transform>(), playArea);
        }

        private void OnDestroy()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            print("xx");
            Context.GameplayManager.UnregisterPlayer(this, _playerActorInstance.gameObject);
            if (_settings._isLocal)
            {
                var playerInputHandler = PlayerInputHandler.Get();
                playerInputHandler?.ResetPlayerDriver();
            }
        }

        #region IPlayerActorCollision

        void IPlayerActorCollision.OnShieldCollision(Collision2D collision, MonoBehaviour component)
        {
            var contactCount = collision.contactCount;
            var point = collision.GetContact(0).point;
            // This call can invalidate current collider!
            _state.OnShieldCollision(out var hitType);
            var message = $"{hitType} {_playerPosChar}";
            ScoreFlash.Push(message, point);
            Debug.Log($"{hitType} {name} {component.name} contacts {contactCount} {point}");
        }

        void IPlayerActorCollision.OnHeadCollision(Collision2D collision)
        {
            var contactCount = collision.contactCount;
            var point = collision.GetContact(0).point;
            // This call can invalidate current collider!
            _state.OnHeadCollision();
            var message = $"HEAD {_playerPosChar}";
            ScoreFlash.Push(message, point);
            Debug.Log($"HEAD {name} contacts {contactCount} {point}");
        }

        #endregion

        #region IPlayerDriver

        string IPlayerDriver.NickName => _settings._nickName;

        int IPlayerDriver.ActorNumber => _actorNumber;

        int IPlayerDriver.PeerCount => 0;

        int IPlayerDriver.PlayerPos => _settings._playerPos;

        int IPlayerDriver.TeamNumber => _settings._teamNumber;

        int IPlayerDriver.MaxPoseIndex => _playerActor.MaxPoseIndex;

        bool IPlayerDriver.IsLocal => _settings._isLocal;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        Vector2 IPlayerDriver.Position => _playerActorTransform.position;

        Transform IPlayerDriver.PlayerTransform => _playerActorTransform;

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

        void IPlayerDriver.StopAndRestartBall()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            var startTheBallTest = FindObjectOfType<StartTheBallTest>();
            ScoreFlash.Push(startTheBallTest != null ? "RESTART" : "CAN NOT RESTART");
            if (startTheBallTest != null)
            {
                startTheBallTest.RestartBallInGame(this);
            }
        }

        #endregion
    }
}