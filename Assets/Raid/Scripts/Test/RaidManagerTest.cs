using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Raid.Scripts.Test
{
    public class RaidManagerTest : MonoBehaviour, IRaidBridge
    {
        [Header("Settings"), SerializeField] private GameObject _fullRaidOverlay;
        [SerializeField] private GameObject _miniRaidIndicator;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private bool _isLocal;

        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

        private RaidBridge _raidBridge;

        private void ResetState()
        {
            _isLocal = false;
            _actorNumber = 0;
            _isRaiding = false;
            _fullRaidOverlay.SetActive(false);
            _miniRaidIndicator.SetActive(false);
        }
        
        private IEnumerator Start()
        {
            ResetState();
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

        public void ShowRaid(IPlayerInfo playerInfo)
        {
            Debug.Log($"playerInfo {playerInfo} isRaiding {_isRaiding}");
            _isLocal = playerInfo.IsLocal;
            _actorNumber = playerInfo.ActorNumber;
            _isRaiding = true;
            _fullRaidOverlay.SetActive(_isLocal);
            _miniRaidIndicator.SetActive(!_isLocal);
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
                _raidBridge.PlayerClosedRaid();
            }
            ResetState();
        }

        #endregion
    }
}