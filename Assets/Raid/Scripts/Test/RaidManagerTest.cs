using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity.Attributes;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Raid.Scripts.Test
{
    public class RaidManagerTest : MonoBehaviour, IRaidBridge
    {
        [Header("Settings"), SerializeField] private GameObject _fullRaidOverlay;
        [SerializeField] private GameObject _miniRaidIndicator;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        [SerializeField, ReadOnly] private bool _isRaidVisible;
        [SerializeField, ReadOnly] private int _actorNumber;
        [SerializeField, ReadOnly] private bool _isLocal;

        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

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
            var player = GetPhotonPlayer(actorNumber);
            if (player == null)
            {
                ScoreFlash.Push(PhotonNetwork.OfflineMode ? "NO OFFLINE RAID" : "NO PHOTON PLAYER");
                _isLocal = false;
                _actorNumber = actorNumber;
            }
            else
            {
                _isLocal = player.IsLocal;
                _actorNumber = player.ActorNumber;
            }
            _isRaiding = true;
            _isRaidVisible = _isLocal;
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
            _isRaiding = false;
            _isRaidVisible = false;
            _actorNumber = 0;
            _fullRaidOverlay.SetActive(false);
            _miniRaidIndicator.SetActive(false);
        }

        #endregion

        private static Player GetPhotonPlayer(int actorNumber)
        {
            if (!PhotonNetwork.InRoom)
            {
                return null;
            }
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == actorNumber)
                {
                    return player;
                }
            }
            return null;
        }
    }
}