using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Raid.Scripts.Test
{
    /// <summary>
    /// Raid gameplay test implementation.
    /// </summary>
    public class RaidManagerTest : MonoBehaviour, IRaidEvent
    {
        [Header("Settings"), SerializeField] private RaidImplementationTest _raidUi;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private bool _isLocal;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private int _bonusCounter;

        private RaidBridge _raidBridge;
        private IBattleEvent _externalBattleEvent;

        private IEnumerator Start()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            var isDisableRaid = features._isDisableRaid;
            if (isDisableRaid)
            {
                enabled = false;
                yield break;
            }
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
                _externalBattleEvent = null;
            }
        }

        private void UiClosedCallback()
        {
            // Eventually our IRaidEvent.RaidStop will be called to actually stop and hide raiding.
            _externalBattleEvent?.PlayerClosedRaid();
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
            _raidUi.ShowRaid(_isLocal, UiClosedCallback);
        }

        void IRaidEvent.RaidBonus(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            _bonusCounter += 1;
        }

        void IRaidEvent.RaidStop(int teamNumber, IPlayerInfo playerInfo)
        {
            Debug.Log($"team {teamNumber} player {playerInfo} isRaiding {_isRaiding}");
            _raidUi.HideRaid();
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
    }
}