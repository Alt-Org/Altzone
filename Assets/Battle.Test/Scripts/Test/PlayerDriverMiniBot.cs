using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using Battle.Test.Scripts.Battle.Players;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    internal class PlayerDriverMiniBot : MonoBehaviour
    {
        [Header("Live Data"), SerializeField, ReadOnly] private string _nickname;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            Debug.Log($"{name}");
            _playerDriver = GetComponent<IPlayerDriver>();
            if (_playerDriver == null)
            {
                _nickname = "no player driver";
                enabled = false;
                return;
            }
            if (_playerDriver.IsLocal)
            {
                _nickname = "not with local player";
                enabled = false;
                return;
            }
            _nickname = "wait";
        }
        
        private void OnEnable()
        {
            StartCoroutine(WaitForPlayerDriverToRegister());
        }

        private IEnumerator WaitForPlayerDriverToRegister()
        {
            var gameplayManager = GameplayManager.Get();
            yield return new WaitUntil(() => gameplayManager.GetPlayerByActorNumber(_playerDriver.ActorNumber) != null);
            _nickname = _playerDriver.NickName ?? "noname";
            Debug.Log($"{name} {_nickname}");
            if (_playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _playerDriver.Rotate(true);
            }
            var playerArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(_playerDriver.PlayerPos);
            var center = playerArea.center;
            _playerDriver.MoveTo(center);
        }
    }
}
