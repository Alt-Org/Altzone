using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using Battle.Scripts.Ui;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;

namespace Battle.Scripts.Test
{
    internal class StartTheRaidTest : MonoBehaviour, IBattleBridge, IRaidBridge
    {
        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private int _teamNumber;
        [SerializeField, ReadOnly] private Transform _playerTransform;

        public bool IsRaiding => _isRaiding;

        public int ActorNumber => _actorNumber;

        public int TeamNumber => _teamNumber;

        private IPlayerManager _playerManager;
        private RaidBridge _raidBridge;

        private IEnumerator Start()
        {
            var failureTime = Time.time + 2f;
            yield return new WaitUntil(() => (_raidBridge ??= FindObjectOfType<RaidBridge>()) != null || Time.time > failureTime);
            if (_raidBridge != null)
            {
                _raidBridge.SetBattleBridge(this);
            }
        }

        private void OnEnable()
        {
            _playerManager = Context.PlayerManager;
        }

        private void OnDestroy()
        {
            if (_raidBridge != null)
            {
                _raidBridge.SetBattleBridge(null);
            }
        }

        private void ResetState()
        {
            _isRaiding = false;
            _actorNumber = 0;
            _teamNumber = PhotonBattle.NoTeamValue;
            _playerTransform = null;
        }

        #region Incoming: IBattleBridge officially registered for the bridge

        public void CloseRaid()
        {
            HideRaid();
        }

        #endregion

        #region Outgoing: IRaidBridge test implementation

        public void ShowRaid(int actorNumber)
        {
            if (_isRaiding)
            {
                return;
            }
            Debug.Log($"actorNumber {actorNumber}");
            var player = _playerManager.GetPlayerByActorNumber(actorNumber);
            if (player == null)
            {
                return;
            }
            StartCoroutine(OnShowRaid(player));
        }

        public void AddRaidBonus()
        {
            if (!_isRaiding)
            {
                return;
            }
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber}");
            if (_raidBridge != null)
            {
                _raidBridge.AddRaidBonus();
            }
        }

        public void HideRaid()
        {
            Debug.Log($"actorNumber {_actorNumber} teamNumber {_teamNumber} isRaiding {_isRaiding}");
            var player = _playerManager.GetPlayerByActorNumber(_actorNumber);
            if (player == null)
            {
                ResetState();
                return;
            }
            StartCoroutine(OnCloseRaid(player));
        }

        #endregion

        private IEnumerator OnShowRaid(IPlayerDriver player)
        {
            _isRaiding = true;
            _actorNumber = player.ActorNumber;
            _teamNumber = player.TeamNumber;
            _playerTransform = player.PlayerTransform;
            Debug.Log($"actorNumber {_actorNumber}");
            player.SetPlayMode(BattlePlayMode.RaidGhosted);
            yield return null;
            if (_raidBridge != null)
            {
                _raidBridge.ShowRaid(player.ActorNumber);
            }
        }

        private IEnumerator OnCloseRaid(IPlayerDriver player)
        {
            Debug.Log($"actorNumber {_actorNumber}");
            player.SetPlayMode(BattlePlayMode.RaidReturn);
            ResetState();
            yield return null;
            this.Publish(new UiEvents.ExitRaid(player));
        }
    }
}