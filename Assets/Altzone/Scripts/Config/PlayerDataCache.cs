using System;
using System.Collections;
using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Player data cache - a common storage for player related data that is persisted somewhere (locally).
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface IPlayerDataCache
    {
        string PlayerName { get; }
        string PlayerGuid { get; }
        int ClanId { get; }
        int CustomCharacterModelId { get; }
        SystemLanguage Language { get; set; }
        bool IsDebugFlag { get; set; }
        bool IsTosAccepted { get; set; }
        bool IsFirstTimePlaying { get; set; }
        bool IsAccountVerified { get; set; }

        bool HasPlayerName { get; }
        void SetPlayerName(string playerName);
        void SetPlayerGuid(string newPlayerGuid);
        void SetClanId(int clanId);
        void SetCustomCharacterModelId(int customCharacterModelId);

#if UNITY_EDITOR
        void DebugSavePlayer();
        void DebugResetPlayer();
#endif
    }

    /// <summary>
    /// Convenience class to keep all local storage related settings in one place.
    /// </summary>
    internal class PlayerData
    {
        public string PlayerName;
        public string PlayerGuid;
        public int ClanId;
        public int CustomCharacterModelId;
        public SystemLanguage Language;
        public bool IsTosAccepted;
        public bool IsFirstTimePlaying;
        public bool IsAccountVerified;
        public bool IsDebugFlag;

        public void ResetData(string dummyPlayerName, int dummyModelId, SystemLanguage defaultLanguage)
        {
            PlayerName = dummyPlayerName;
            PlayerGuid = string.Empty;
            ClanId = dummyModelId;
            CustomCharacterModelId = dummyModelId;
            Language = (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefKeys.LanguageCode, (int)defaultLanguage);
            IsTosAccepted = false;
            IsFirstTimePlaying = true;
            IsAccountVerified = false;
            IsDebugFlag = false;
        }

        public override string ToString()
        {
            return $"{nameof(PlayerName)}: {PlayerName}, {nameof(CustomCharacterModelId)}: {CustomCharacterModelId}, {nameof(ClanId)}: {ClanId}" +
                   $", {nameof(Language)}: {Language}, {nameof(IsTosAccepted)}: {IsTosAccepted}" +
                   $", {nameof(IsFirstTimePlaying)}: {IsFirstTimePlaying}, {nameof(IsAccountVerified)}: {IsAccountVerified}" +
                   $", {nameof(IsDebugFlag)}: {IsDebugFlag}, {nameof(PlayerGuid)}: {PlayerGuid}";
        }
    }

    /// <summary>
    /// <c>IPlayerDataCache</c> default implementation.
    /// </summary>
    internal class PlayerDataCache : IPlayerDataCache
    {
        internal static IPlayerDataCache Create()
        {
            return new PlayerDataCacheLocal();
        }

        /// <summary>
        /// Negative model IDs are considered invalid.
        /// </summary>
        private const int DummyModelId = -1;

        private const string DefaultPlayerName = "Player";
        private const SystemLanguage DefaultLanguage = SystemLanguage.Finnish;

        private readonly PlayerData _playerData = new();

        /// <summary>
        /// Player name.
        /// </summary>
        /// <remarks>
        /// This should be validated and sanitized before accepting a new value.
        /// </remarks>
        public string PlayerName
        {
            get => _playerData.PlayerName;
            private set
            {
                _playerData.PlayerName = value ?? string.Empty;
                Save();
            }
        }

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
        /// Player clan id.
        /// </summary>
        public int ClanId
        {
            get => _playerData.ClanId;
            private set
            {
                _playerData.ClanId = value;
                Save();
            }
        }

        /// <summary>
        /// Player custom character model id.
        /// </summary>
        public int CustomCharacterModelId
        {
            get => _playerData.CustomCharacterModelId;
            private set
            {
                _playerData.CustomCharacterModelId = value;
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

        /// <summary>
        /// Current battle character.
        /// </summary>
        public bool HasPlayerName => !string.IsNullOrWhiteSpace(PlayerName);

        public void SetPlayerName(string playerName)
        {
            PlayerName = playerName ?? DefaultPlayerName;
            LootLockerWrapper.SetPlayerName(_playerData.PlayerName);
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

        public void SetClanId(int clanId)
        {
            ClanId = clanId <= 0 ? DummyModelId : clanId;
        }

        public void SetCustomCharacterModelId(int customCharacterModelId)
        {
            CustomCharacterModelId = customCharacterModelId <= 0 ? DummyModelId : customCharacterModelId;
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
        public void DebugResetPlayer()
        {
            _playerData.ResetData(DefaultPlayerName, DummyModelId, DefaultLanguage);
        }

        public void DebugSavePlayer()
        {
            InternalSave();
        }
#endif

        public override string ToString()
        {
            // This is required for actual implementation to detect changes in our changeable properties!
            return
                $"Name {PlayerName}, Model {CustomCharacterModelId}, Clan {ClanId}, ToS {(IsTosAccepted ? 1 : 0)}, Lang {Language}, Guid {PlayerGuid}";
        }

        /// <summary>
        /// <c>PlayerDataCache</c> implementation using UNITY <c>PlayerPrefs</c> as backing storage.
        /// </summary>
        private class PlayerDataCacheLocal : PlayerDataCache
        {
            private readonly MonoBehaviour _host;
            private Coroutine _delayedSave;

            public PlayerDataCacheLocal()
            {
                _host = UnityMonoHelper.Instance;
                _playerData.PlayerName = PlayerPrefs.GetString(PlayerPrefKeys.PlayerName, string.Empty);
                _playerData.PlayerGuid = PlayerPrefs.GetString(PlayerPrefKeys.PlayerGuid, string.Empty);
                _playerData.ClanId = PlayerPrefs.GetInt(PlayerPrefKeys.ClanId, DummyModelId);
                _playerData.CustomCharacterModelId = PlayerPrefs.GetInt(PlayerPrefKeys.CharacterModelId, DummyModelId);
                _playerData.Language = (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefKeys.LanguageCode, (int)DefaultLanguage);
                _playerData.IsTosAccepted = PlayerPrefs.GetInt(PlayerPrefKeys.TermsOfServiceAccepted, 0) == 1;
                _playerData.IsFirstTimePlaying = PlayerPrefs.GetInt(PlayerPrefKeys.IsFirstTimePlaying, 1) == 1;
                _playerData.IsAccountVerified = PlayerPrefs.GetInt(PlayerPrefKeys.IsAccountVerified, 0) == 1;
                _playerData.IsDebugFlag = PlayerPrefs.GetInt(PlayerPrefKeys.IsDebugFlag, 0) == 1;
                if (!string.IsNullOrWhiteSpace(PlayerGuid) && !string.IsNullOrWhiteSpace(_playerData.PlayerName))
                {
                    Debug.Log(_playerData.ToString());
                    return;
                }
                // Create and save these settings immediately on this device!
                if (string.IsNullOrWhiteSpace(PlayerGuid))
                {
                    _playerData.PlayerGuid = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                }
                if (string.IsNullOrWhiteSpace(_playerData.PlayerName))
                {
                    _playerData.PlayerName = DefaultPlayerName;
                    PlayerPrefs.SetString(PlayerPrefKeys.PlayerName, PlayerName);
                }
                PlayerPrefs.Save();
            }

            public override string ToString()
            {
                return _playerData.ToString();
            }

            protected override void InternalSave()
            {
                PlayerPrefs.SetString(PlayerPrefKeys.PlayerName, PlayerName);
                PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                PlayerPrefs.SetInt(PlayerPrefKeys.ClanId, ClanId);
                PlayerPrefs.SetInt(PlayerPrefKeys.CharacterModelId, CustomCharacterModelId);
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