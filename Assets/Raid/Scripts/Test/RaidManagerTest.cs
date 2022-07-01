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
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private bool _isLocal;

        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

        private RaidBridge _raidBridge;

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

        void IRaidBridge.ShowRaid(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"teamNumber {teamNumber} playerInfo {playerInfo} isRaiding {_isRaiding}");
            if (_isRaiding)
            {
                if (teamNumber == _teamNumber)
                {
                    _actorNumber = playerInfo?.ActorNumber ?? 0;
                    AddRaidBonus();
                    return;
                }
                ResetState();
                HideRaid();
                return;
            }
            _isLocal = playerInfo.IsLocal;
            _teamNumber = teamNumber;
            _actorNumber = playerInfo.ActorNumber;
            _isRaiding = true;
            _fullRaidOverlay.SetActive(_isLocal);
            _miniRaidIndicator.SetActive(!_isLocal);
        }

        private void ResetState()
        {
            _isLocal = false;
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
            _isRaiding = false;
            _fullRaidOverlay.SetActive(false);
            _miniRaidIndicator.SetActive(false);
        }

        private void AddRaidBonus()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber}");
        }

        private void HideRaid()
        {
            Debug.Log($"teamNumber {_teamNumber} actorNumber {_actorNumber}");
            if (_raidBridge != null)
            {
                ((IBattleBridge)_raidBridge).PlayerClosedRaid();
            }
        }

        #endregion
    }
}