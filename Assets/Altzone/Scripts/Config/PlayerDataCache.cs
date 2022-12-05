using System;
using System.Collections;
using Altzone.Scripts.Model;
using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Player data cache - a common storage for player related data that is persisted somewhere (locally).
    /// </summary>
    public interface IPlayerDataCache
    {
        string PlayerName { get; set; }
        string PlayerGuid { get; }
        int ClanId { get; set; }
        int CustomCharacterModelId { get; set; }
        SystemLanguage Language { get; set; }
        bool IsDebugFlag { get; set; }
        bool IsTosAccepted { get; set; }
        bool IsFirstTimePlaying { get; set; }
        bool IsAccountVerified { get; set; }

        IBattleCharacter CurrentBattleCharacter { get; }
        ClanModel Clan { get; }

        bool HasPlayerName { get; }
        void UpdatePlayerGuid(string newPlayerGuid);

#if UNITY_EDITOR
        void DebugSavePlayer();
        void DebugResetPlayer();
#endif
    }

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

        public void ResetData(int dummyModelId, SystemLanguage defaultLanguage)
        {
            PlayerName = string.Empty;
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
        public static IPlayerDataCache Create()
        {
            return new PlayerDataCacheLocal();
        }

        private const string DefaultPlayerName = "Player";
        private const int DummyModelId = int.MaxValue;
        private const SystemLanguage DefaultLanguage = SystemLanguage.Finnish;

        private readonly PlayerData PlayerData = new();

        /// <summary>
        /// Player name.
        /// </summary>
        /// <remarks>
        /// This should be validated and sanitized before accepting a new value.
        /// </remarks>
        public string PlayerName
        {
            get => PlayerData.PlayerName;
            set
            {
                PlayerData.PlayerName = value ?? string.Empty;
                LootLockerWrapper.SetPlayerName(PlayerData.PlayerName);
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
            get => PlayerData.PlayerGuid;
            private set
            {
                PlayerData.PlayerGuid = value ?? string.Empty;
                Save();
            }
        }

        /// <summary>
        /// Player clan id.
        /// </summary>
        public int ClanId
        {
            get => PlayerData.ClanId;
            set
            {
                PlayerData.ClanId = value;
                Save();
            }
        }

        /// <summary>
        /// Player custom character model id.
        /// </summary>
        public int CustomCharacterModelId
        {
            get => PlayerData.CustomCharacterModelId;
            set
            {
                PlayerData.CustomCharacterModelId = value;
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
            get => PlayerData.Language;
            set
            {
                PlayerData.Language = value;
                Save();
            }
        }

        /// <summary>
        /// Is Terms Of Service accepted?
        /// </summary>
        public bool IsTosAccepted
        {
            get => PlayerData.IsTosAccepted;
            set
            {
                PlayerData.IsTosAccepted = value;
                Save();
            }
        }

        /// <summary>
        /// Is this first time game is started?
        /// </summary>
        public bool IsFirstTimePlaying
        {
            get => PlayerData.IsFirstTimePlaying;
            set
            {
                PlayerData.IsFirstTimePlaying = value;
                Save();
            }
        }

        /// <summary>
        /// Is player's third party account verified?
        /// </summary>
        public bool IsAccountVerified
        {
            get => PlayerData.IsAccountVerified;
            set
            {
                PlayerData.IsAccountVerified = value;
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
            get => PlayerData.IsDebugFlag;
            set
            {
                PlayerData.IsDebugFlag = value;
                Save();
            }
        }

        /// <summary>
        /// Current battle character.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to be valid reference all the time even <c>CharacterModelId</c> is invalid.
        /// </remarks>
        public IBattleCharacter CurrentBattleCharacter => Storefront.Get().GetBattleCharacter(PlayerData.CustomCharacterModelId);

        public ClanModel Clan => Storefront.Get().GetClanModel(PlayerData.ClanId) ?? new ClanModel(DummyModelId, string.Empty, string.Empty);

        public bool HasPlayerName => !string.IsNullOrWhiteSpace(PlayerName);

        public void UpdatePlayerGuid(string newPlayerGuid)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(newPlayerGuid), "!string.IsNullOrWhiteSpace(newPlayerGuid)");
            Assert.AreNotEqual(PlayerData.PlayerGuid, newPlayerGuid);

            PlayerGuid = newPlayerGuid;
        }

        public string GetPlayerInfoLabel()
        {
            var characterModelName = CurrentBattleCharacter.Name;
            if (ClanId > 0)
            {
                var clan = Storefront.Get().GetClanModel(ClanId);
                if (clan != null)
                {
                    return $"{PlayerName}[{clan.Tag}] {characterModelName}";
                }
            }
            return $"{PlayerName} {characterModelName}";
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
            PlayerData.ResetData(DummyModelId, DefaultLanguage);
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
                PlayerData.PlayerName = PlayerPrefs.GetString(PlayerPrefKeys.PlayerName, string.Empty);
                PlayerData.PlayerGuid = PlayerPrefs.GetString(PlayerPrefKeys.PlayerGuid, string.Empty);
                PlayerData.ClanId = PlayerPrefs.GetInt(PlayerPrefKeys.ClanId, DummyModelId);
                PlayerData.CustomCharacterModelId = PlayerPrefs.GetInt(PlayerPrefKeys.CharacterModelId, DummyModelId);
                PlayerData.Language = (SystemLanguage)PlayerPrefs.GetInt(PlayerPrefKeys.LanguageCode, (int)DefaultLanguage);
                PlayerData.IsTosAccepted = PlayerPrefs.GetInt(PlayerPrefKeys.TermsOfServiceAccepted, 0) == 1;
                PlayerData.IsFirstTimePlaying = PlayerPrefs.GetInt(PlayerPrefKeys.IsFirstTimePlaying, 1) == 1;
                PlayerData.IsAccountVerified = PlayerPrefs.GetInt(PlayerPrefKeys.IsAccountVerified, 0) == 1;
                PlayerData.IsDebugFlag = PlayerPrefs.GetInt(PlayerPrefKeys.IsDebugFlag, 0) == 1;
                if (!string.IsNullOrWhiteSpace(PlayerGuid) && !string.IsNullOrWhiteSpace(PlayerData.PlayerName))
                {
                    Debug.Log(PlayerData.ToString());
                    return;
                }
                // Create and save these settings immediately on this device!
                if (string.IsNullOrWhiteSpace(PlayerGuid))
                {
                    PlayerData.PlayerGuid = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                }
                if (string.IsNullOrWhiteSpace(PlayerData.PlayerName))
                {
                    PlayerData.PlayerName = DefaultPlayerName;
                    PlayerPrefs.SetString(PlayerPrefKeys.PlayerName, PlayerName);
                }
                PlayerPrefs.Save();
            }

            public override string ToString()
            {
                return PlayerData.ToString();
            }

            protected override void InternalSave()
            {
                PlayerPrefs.SetString(PlayerPrefKeys.PlayerName, PlayerName);
                PlayerPrefs.SetString(PlayerPrefKeys.PlayerGuid, PlayerGuid);
                PlayerPrefs.SetInt(PlayerPrefKeys.ClanId, ClanId);
                PlayerPrefs.SetInt(PlayerPrefKeys.CharacterModelId, CustomCharacterModelId);
                PlayerPrefs.SetInt(PlayerPrefKeys.LanguageCode, (int)PlayerData.Language);
                PlayerPrefs.SetInt(PlayerPrefKeys.TermsOfServiceAccepted, IsTosAccepted ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsFirstTimePlaying, IsFirstTimePlaying ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsAccountVerified, IsAccountVerified ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefKeys.IsDebugFlag, IsDebugFlag ? 1 : 0);
                Debug.Log(PlayerData.ToString());
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