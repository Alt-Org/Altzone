using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Raid.Scripts.Test
{
    public class StartTheRaidTest : MonoBehaviour, IRaidBridge
    {
        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;

        private RaidBridge _raidBridge;

        private IEnumerator Start()
        {
            var failureTime = Time.time + 2f;
            yield return new WaitUntil(() => (_raidBridge ??= FindObjectOfType<RaidBridge>()) != null || Time.time > failureTime);
            if (_raidBridge != null)
            {
                _raidBridge.SetRaidBridge(this);
            }
        }

        private void OnDestroy()
        {
            if (_raidBridge != null)
            {
                _raidBridge.SetRaidBridge(null);
            }
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame && _isRaiding)
            {
                HideRaid();
            }
        }

        #region IRaidBridge

        public void ShowRaid(int actorNumber)
        {
            Debug.Log($"actorNumber {_actorNumber} <- {actorNumber} isRaiding {_isRaiding}");
            Assert.IsFalse(actorNumber == 0, "actorNumber == 0");
            _isRaiding = true;
            _actorNumber = actorNumber;
        }

        public void AddRaidBonus()
        {
            if (!_isRaiding)
            {
                return;
            }
            Debug.Log($"actorNumber {_actorNumber} isRaiding {_isRaiding}");
        }

        public void HideRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} isRaiding {_isRaiding}");
            if (_isRaiding && _raidBridge != null)
            {
                _raidBridge.CloseRaid();
            }
            _isRaiding = false;
            _actorNumber = 0;
        }

        #endregion
    }
}