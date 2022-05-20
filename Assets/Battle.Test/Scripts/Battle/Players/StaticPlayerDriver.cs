using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    public class StaticPlayerDriver : MonoBehaviour, IPlayerDriver
    {
        [Header("Settings"), SerializeField] private int _playerPos;
        [SerializeField] private Defence _playerMainSkill = Defence.Deflection;
        [SerializeField] private PlayerActor _playerPrefab;
        [SerializeField] private bool _isConnectInputHandler;

        [Header("Live Data"), SerializeField] private PlayerActor _playerActorInstance;

        private CharacterModel _characterModel;
        private IPlayerActor _playerActor;

        private void Awake()
        {
            print("+");
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(_playerPos), "PhotonBattle.IsValidGameplayPos(_playerPos)");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _characterModel = Storefront.Get().GetCharacterModel((int)_playerMainSkill);
            _playerActorInstance = PlayerActor.Instantiate(this, _playerPrefab);
            _playerActor = _playerActorInstance;
            _playerActor.Speed = _characterModel.Speed;
            if (_isConnectInputHandler)
            {
                var playerInputHandler = PlayerInputHandler.Get();
                var playArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(_playerPos);
                playerInputHandler.SetPlayerDriver(this, _playerActorInstance.GetComponent<Transform>(), playArea);
            }
        }

        private void OnDestroy()
        {
            if (_isConnectInputHandler)
            {
                var playerInputHandler = PlayerInputHandler.Get();
                playerInputHandler?.ResetPlayerDriver();
            }
        }

        #region IPlayerDriver

        string IPlayerDriver.NickName => "Static";

        int IPlayerDriver.ActorNumber => 0;

        int IPlayerDriver.PlayerPos => _playerPos;

        int IPlayerDriver.MaxPoseIndex => 0;
        
        bool IPlayerDriver.IsLocal => false; 

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        void IPlayerDriver.SetStunned(float duration)
        {
            _playerActor.SetBuff(PlayerBuff.Stunned, duration);
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

        #endregion
    }
}