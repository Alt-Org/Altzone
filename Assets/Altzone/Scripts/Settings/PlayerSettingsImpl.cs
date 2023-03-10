using System;
using System.Collections;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Settings
{
    /// <summary>
    /// Factory access method to <c>IPlayerSettings</c>. 
    /// </summary>
    public static class PlayerSettings
    {
        public static IPlayerSettings Create() => new PlayerSettingsImpl.PlayerSettingsLocal();
    }

    /// <summary>
    /// <c>IPlayerSettings</c> default implementation.
    /// </summary>
    internal class PlayerSettingsImpl : IPlayerSettings
    {
        private const SystemLanguage DefaultLanguage = SystemLanguage.Finnish;

        private readonly PlayerData _playerData = new();

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        /// <remarks>
        /// When new player is detected this could be set and persisted in all external systems in order to identify this player unambiguously.
        /// </remarks>
        public string PlayerGuid
        {
            get => _playerData.PlayerGuid;
            private set
            {
                _playerData.PlayerGuid = value ?? string.Empty;
                Save();
            }
        }

        /// <summary>
        /// Player's UNITY language.
        /// </summary>
        /// <remarks>
        /// We use <c>Localizer</c> to do actual storing of the language.
        /// </remarks>
        public SystemLanguage Language
        {
            get => _playerData.Language;
            set
            {
                _playerData.Language = value;
                Save();
            }
        }

        /// <summary>
        /// Is Terms Of Service accepted?
        /// </summary>
        public bool IsTosAccepted
        {
            get => _playerData.IsTosAccepted;
            set
            {
                _playerData.IsTosAccepted = value;
                Save();
            }
        }

        /// <summary>
        /// Is this first time game is started?
        /// </summary>
        public bool IsFirstTimePlaying
        {
            get => _playerData.IsFirstTimePlaying;
            set
            {
                _playerData.IsFirstTimePlaying = value;
                Save();
            }
        }

        /// <summary>
        /// Is player's third party account verified?
        /// </summary>
        public bool IsAccountVerified
        {
            get => _playerData.IsAccountVerified;
            set
            {
                _playerData.IsAccountVerified = value;
                Save();
            }
        }

        /// <summary>
        /// Version number for <c>IPlayerSettings</c> for future game updates.
        /// </summary>
        public int PlayerSettingsVersion => 1;

        /// <summary>
        /// Debug FLag for debugging and diagnostics purposes.
        /// </summary>
        /// <remarks>
        /// Should be hidden in final product.
        /// </remarks>
        public bool IsDebugFlag
        {
            get => _playerData.IsDebugFlag;
            set
            {
                _playerData.IsDebugFlag = value;
                Save();
            }
        }

        public void SetPlayerGuid(string newPlayerGuid)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(newPlayerGuid), "!string.IsNullOrWhiteSpace(newPlayerGuid)");
            if (_playerData.PlayerGuid != null)
            {
                Assert.AreNotEqual(_playerData.PlayerGuid, newPlayerGuid);
                Assert.AreEqual(_playerData.PlayerGuid.Length, newPlayerGuid.Length);
            }
            PlayerGuid = newPlayerGuid;
        }

        /// <summary>
        /// Protected <c>Save</c> method to handle single property change.
        /// </summary>
        protected virtual void Save()
        {
            // Placeholder for actual implementation in derived class.
        }

        /// <summary>
        /// Protected <c>InternalSave</c> to handle actual saving somewhere.
        /// </summary>
        protected virtual void InternalSave()
        {
            // Placeholder for actual implementation in derived class.
        }

#if UNITY_EDITOR
        public void DebugResetPlayerSettings()
        {
            _playerData.ResetData(DefaultLanguage);
        }

        public void DebugSavePlayerSettings()
        {
            InternalSave();
        }
#endif

        /// <summary>
        /// Convenience class to keep all local storage related settings in one place.
        /// </summary>
        private class PlayerData
        {
            public string PlayerGuid;
            public SystemLanguage Language;
            public bool IsTosAccepted;
            public bool IsFirstTimePlaying;
            public bool IsAccountVerified;
            public bool IsDebugFlag;

            public void ResetData(SystemLanguage defaultLanguage)
            {
                PlayerGuid = string.Empty;
                Language = (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefKeys.LanguageCode, (int)defaultLanguage);
                IsTosAccepted = false;
                IsFirstTimePlaying = true;
                IsAccountVerified = false;
                IsDebugFlag = false;
            }

            public override string ToString()
            {
                return $"{nameof(Language)}: {Language}, {nameof(IsTosAccepted)}: {IsTosAccepted}" +
                       $", {nameof(IsFirstTimePlaying)}: {IsFirstTimePlaying}, {nameof(IsAccountVerified)}: {IsAccountVerified}" +
                       $", {nameof(IsDebugFlag)}: {IsDebugFlag}, {nameof(PlayerGuid)}: {PlayerGuid}";
            }
        }

        /// <summary>
        /// <c>PlayerDataCache</c> implementation using UNITY <c>PlayerPrefs</c> as backing storage.
        /// </summary>
        internal class PlayerSettingsLocal : PlayerSettingsImpl
        {
            private readonly MonoBehaviour _host;
            private Coroutine _delayedSave;

            public PlayerSettingsLocal()
            {
                _host = UnityMonoHelper.Instance;
                _playerData.PlayerGuid = PlayerPrefs.GetString(PlayerPrefKeys.PlayerGuid, string.Empty);
                _playerData.Language = (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefKeys.LanguageCode, (int)DefaultLanguage);
                _playerData.IsTosAccepted = PlayerPrefs.GetInt(PlayerPrefKeys.TermsOfServiceAccepted, 0) == 1;
                _playerData.IsFirstTimePlaying = PlayerPrefs.GetInt(PlayerPrefKeys.IsFirstTimePlaying, 1) == 1;
                _playerData.IsAccountVerified = PlayerPrefs.GetInt(PlayerPrefKeys.IsAccountVerified, 0) == 1;
                _playerData.IsDebugFlag = PlayerPrefs.GetInt(PlayerPrefKeys.IsDebugFlag, 0) == 1;
                // Create and save these settings immediately on this device!
                if (string.IsNullOrWhiteSpace(PlayerGuid))
                {
                    _playerData.PlayerGuid = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                }
                PlayerPrefs.Save();
            }

            public override string ToString()
            {
                return _playerData.ToString();
            }

            protected override void InternalSave()
            {
                PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                PlayerPrefs.SetInt(PlayerPrefKeys.LanguageCode, (int)_playerData.Language);
                PlayerPrefs.SetInt(PlayerPrefKeys.TermsOfServiceAccepted, IsTosAccepted ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsFirstTimePlaying, IsFirstTimePlaying ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsAccountVerified, IsAccountVerified ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsDebugFlag, IsDebugFlag ? 1 : 0);
                Debug.Log(_playerData.ToString());
            }

            protected override void Save()
            {
                if (_host == null)
                {
                    // Can not delay, using UNITY default functionality save on exit
                    return;
                }
                if (_delayedSave != null)
                {
                    // NOP - already delayed.
                    return;
                }
                // Save all changed player prefs on next frame.
                _delayedSave = _host.StartCoroutine(DelayedSave());
            }

            private IEnumerator DelayedSave()
            {
                // By default Unity writes preferences to disk during OnApplicationQuit().
                // - you can force them to disk using PlayerPrefs.Save().
                yield return null;
                Debug.Log("PlayerPrefs.Save");
                InternalSave();
                PlayerPrefs.Save();
                _delayedSave = null;
            }
        }
    }
}