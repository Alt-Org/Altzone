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
        [Header("Live Data"), ReadOnly] public bool _isLocal;
        [ReadOnly] public string _nickname;

        private IPlayerDriver _playerDriver;

        private void Awake()
        {
            _playerDriver = GetComponent<IPlayerDriver>();
            if (_playerDriver == null)
            {
                enabled = false;
                return;
            }
            _nickname = _playerDriver.NickName ?? "noname";
            _isLocal = _playerDriver.IsLocal;
            if (_isLocal)
            {
                enabled = false;
            }
        }

        private IEnumerator WaitForPlayerDriver()
        {
            var component = _playerDriver as MonoBehaviour;
            if (component == null)
            {
                yield break;
            }
            yield return new WaitUntil(() => component.enabled);
            if (_playerDriver.TeamNumber == PhotonBattle.TeamRedValue)
            {
                _playerDriver.Rotate(true);
            }
            var playerArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(_playerDriver.PlayerPos);
            var center = playerArea.center;
            _playerDriver.MoveTo(center);
        }
        
        private void OnEnable()
        {
            StartCoroutine(WaitForPlayerDriver());
        }
    }
}
