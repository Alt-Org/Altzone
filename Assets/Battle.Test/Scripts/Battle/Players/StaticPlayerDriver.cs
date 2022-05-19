using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    public class StaticPlayerDriver : MonoBehaviour, IPlayerDriver
    {
        [Header("Settings"), SerializeField] private int _playerPos;
        [SerializeField] private Defence _playerMainSkill = Defence.Deflection;
        [SerializeField] private PlayerActor _playerPrefab;

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
        }

        #region IPlayerDriver

        string IPlayerDriver.NickName => "Static";

        int IPlayerDriver.ActorNumber => 0;

        int IPlayerDriver.PlayerPos => _playerPos;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
        {
            _playerActor.MoveTo(targetPosition);
        }

        #endregion
    }
}