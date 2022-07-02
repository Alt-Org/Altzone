using System;
using Prg.Scripts.Common.Unity.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Raid.Scripts.Test
{
    /// <summary>
    /// Raid UI implementation test script.
    /// </summary>
    public class RaidImplementationTest : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject _fullRaidOverlay;
        [SerializeField] private GameObject _miniRaidIndicator;

        [Header("Live Data"), SerializeField, ReadOnly] private bool _isRaiding;
        
        [Header("Debug Settings"), SerializeField] private Key _controlKey = Key.F5;

        private Action _uiClosedCallback;
        
        private void Awake()
        {
            HideRaid();
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame && _isRaiding)
            {
                _uiClosedCallback?.Invoke();
            }
        }

        
        public void ShowRaid(bool isLocal, Action uiClosedCallback)
        {
            _isRaiding = true;
            _uiClosedCallback = uiClosedCallback;
            _fullRaidOverlay.SetActive(isLocal);
            _miniRaidIndicator.SetActive(!isLocal);
        }

        public void HideRaid()
        {
            _isRaiding = false;
            _fullRaidOverlay.SetActive(false);
            _miniRaidIndicator.SetActive(false);
        }
    }
}