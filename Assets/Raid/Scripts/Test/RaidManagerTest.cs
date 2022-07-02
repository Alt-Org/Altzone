using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Raid.Scripts.Test
{
    /// <summary>
    /// Raid gameplay test implementation.
    /// </summary>
    public class RaidManagerTest : MonoBehaviour, IRaidEvent
    {
        [Header("Settings"), SerializeField] private GameObject _fullRaidOverlay;
        [SerializeField] private GameObject _miniRaidIndicator;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private bool _isLocal;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private int _bonusCounter;

        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

        private RaidBridge _raidBridge;
        private IBattleEvent _externalBattleEvent;

        private IEnumerator Start()
        {
            var failureTime = Time.time + 2f;
            yield return new WaitUntil(() => (_raidBridge ??= FindObjectOfType<RaidBridge>()) != null || Time.time > failureTime);
            if (_raidBridge == null)
            {
                enabled = false;
                yield break;
            }
            _raidBridge.SetRaidEventHandler(this);
            _externalBattleEvent = _raidBridge;
            ResetState();
        }

        private void OnDestroy()
        {
            if (_raidBridge != null)
            {
                _raidBridge.SetRaidEventHandler(null);
            }
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame && _isRaiding)
            {
                // Eventually IRaidEvent.RaidStop will be called to actually stop and hide raiding.
                _externalBattleEvent.PlayerClosedRaid();
            }
        }

        #region IRaidBridge

        void IRaidEvent.RaidStart(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");

            _isRaiding = true;
            _isLocal = playerInfo?.IsLocal ?? true;
            _teamNumber = teamNumber;
            _actorNumber = playerInfo?.ActorNumber ?? 0;
            _bonusCounter = 0;
            ShowRaid();
        }

        void IRaidEvent.RaidBonus(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            _bonusCounter += 1;
        }

        void IRaidEvent.RaidStop(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            HideRaid();
            ResetState();
        }

        #endregion

        private void ResetState()
        {
            _isRaiding = false;
            _isLocal = false;
            _teamNumber = PhotonBattle.NoTeamValue;
            _actorNumber = 0;
            _bonusCounter = 0;
       }

        private void ShowRaid()
        {
            _fullRaidOverlay.SetActive(_isLocal);
            _miniRaidIndicator.SetActive(!_isLocal);
        }

        private void HideRaid()
        {
            _fullRaidOverlay.SetActive(false);
            _miniRaidIndicator.SetActive(false);
        }
    }
}