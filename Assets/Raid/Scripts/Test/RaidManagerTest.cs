using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Prg.Scripts.Common.Unity.Attributes;
using Prg.Scripts.Common.Unity.ToastMessages;
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
            ScoreFlashNet.RegisterEventListener();
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
                // We do not have cache player available for now.
                ResetState();
                HideRaid(_raidBridge, null);
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
                    AddRaidBonus(playerInfo);
                    return;
                }
                ResetState();
                HideRaid(_raidBridge, playerInfo);
                return;
            }
            _isLocal = playerInfo.IsLocal;
            _teamNumber = teamNumber;
            _actorNumber = playerInfo.ActorNumber;
            _isRaiding = true;
            _fullRaidOverlay.SetActive(_isLocal);
            _miniRaidIndicator.SetActive(!_isLocal);

            var info = _teamNumber == PhotonBattle.TeamBlueValue ? "RED"
                : _teamNumber == PhotonBattle.TeamRedValue ? "BLUE"
                : $"({teamNumber})";
            if (PhotonNetwork.IsMasterClient)
            {
                ScoreFlashNet.Push($"RAID {info}", playerInfo.Position);
            }
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

        private void AddRaidBonus(IPlayerInfo playerInfo)
        {
            Debug.Log($"playerInfo {playerInfo}");
            if (PhotonNetwork.IsMasterClient)
            {
                var position = playerInfo?.Position ?? Vector2.zero;
                ScoreFlashNet.Push($"RAID BONUS", position);
            }
        }

        private static void HideRaid(IBattleBridge battleBridge, IPlayerInfo playerInfo)
        {
            Debug.Log($"playerInfo {playerInfo}");
            if (PhotonNetwork.IsMasterClient)
            {
                ScoreFlashNet.Push("RAID EXIT", Vector2.zero);
                battleBridge?.PlayerClosedRaid();
            }
        }

        #endregion
    }
}