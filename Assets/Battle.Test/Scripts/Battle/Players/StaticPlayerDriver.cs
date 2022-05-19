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

        [Header("Live Data"), SerializeField] private PlayerActor _playerActor;

        private IPlayerDriver _interface;
        private CharacterModel _characterModel;
        
        private void Awake()
        {
            print("+");
            _interface = this;
            Assert.IsTrue(PhotonBattle.IsValidGameplayPos(_interface.PlayerPos), "PhotonBattle.IsValidGameplayPos(_interface.PlayerPos)");
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _characterModel = Storefront.Get().GetCharacterModel((int)_playerMainSkill);
            _playerActor = PlayerActor.Instantiate(this, _playerPrefab);
        }
        
        #region IPlayerDriver

        string IPlayerDriver.NickName => "Static";

        int IPlayerDriver.ActorNumber => 0;

        int IPlayerDriver.PlayerPos => _playerPos;

        CharacterModel IPlayerDriver.CharacterModel => _characterModel;

        #endregion
        
    }
}