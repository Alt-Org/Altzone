using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Game features that can be toggled on and off.
    /// </summary>
    [Serializable]
    public class GameFeatures
    {
        /// <summary>
        /// Rotate game camera for upper team so they see their own game area in lower part of the screen.
        /// </summary>
        public bool _isRotateGameCamera;

        /// <summary>
        /// Is local player team color always "blue" team color.
        /// </summary>
        public bool _isLocalPLayerOnTeamBlue;

        /// <summary>
        /// Spawn mini ball aka diamonds.
        /// </summary>
        public bool _isSPawnMiniBall;

        /// <summary>
        /// Is shield always on when team has only one player (for testing).
        /// </summary>
        public bool _isSinglePlayerShieldOn;

        public void CopyFrom(GameFeatures other)
        {
            PropertyCopier<GameFeatures, GameFeatures>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Game constraints that that control workings of the game.
    /// </summary>
    [Serializable]
    public class GameConstraints
    {
        [Header("UI"), Min(2)] public int _minPlayerNameLength = 2;
        [Min(3)] public int _maxPlayerNameLength = 16;

        public void CopyFrom(GameConstraints other)
        {
            PropertyCopier<GameConstraints, GameConstraints>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    [Serializable]
    public class GameVariables
    {
        [Header("Battle"), Min(1)] public int _roomStartDelay;

        [Header("Ball")] public float _ballMoveSpeedMultiplier;
        public float _ballLerpSmoothingFactor;
        public float _ballTeleportDistance;
        public float _minSlingShotDistance;
        public float _maxSlingShotDistance;
        [Min(1)] public int _ballRestartDelay;

        [Header("Player")] public float _playerMoveSpeedMultiplier;
        public float _playerSqrMinRotationDistance;
        public float _playerSqrMaxRotationDistance;

        [Header("Shield")] public float _shieldDistanceMultiplier;

        public void CopyFrom(GameVariables other)
        {
            PropertyCopier<GameVariables, GameVariables>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Well known prefabs for the game.
    /// </summary>
    [Serializable]
    public class GamePrefabs
    {
        [Header("Battle")] public GameObject _playerForDes;
        public GameObject _playerForDef;
        public GameObject _playerForInt;
        public GameObject _playerForPro;
        public GameObject _playerForRet;
        public GameObject _playerForEgo;
        public GameObject _playerForCon;

        public void CopyFrom(GamePrefabs other)
        {
            PropertyCopier<GamePrefabs, GamePrefabs>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Player data cache.
    /// </summary>
    /// <remarks>
    /// Common location for player related data that is persisted elsewhere.<br />
    /// As this class is visible in UNITY Editor it can not be <c>abstract</c> as it should be!
    /// </remarks>
    [Serializable]
    public class PlayerDataCache
    {
        [SerializeField] protected string _playerName;

        /// <summary>
        /// Player name.
        /// </summary>
        /// <remarks>
        /// This should be validated and sanitized before accepting a new value.
        /// </remarks>
        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value ?? string.Empty;
                Save();
            }
        }

        [SerializeField] protected int _characterModelId;

        /// <summary>
        /// Player character model id.
        /// </summary>
        public int CharacterModelId
        {
            get => _characterModelId;
            set
            {
                _characterModelId = value;
                Save();
            }
        }

        /// <summary>
        /// Player character model.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to be valid reference all the time even <c>CharacterModelId</c> is invalid.
        /// </remarks>
        public CharacterModel CharacterModel =>
            Storefront.Get().GetCharacterModel(_characterModelId) ??
            new CharacterModel(-1, "Ö", Defence.Desensitisation, 0, 0, 0, 0);

        [SerializeField] protected string _playerHandle;

        /// <summary>
        /// Unique string to identify this player across devices and systems.
        /// </summary>
        /// <remarks>
        /// When new player is detected this should be given and persisted in all external systems in order to identify this player unambiguously.
        /// </remarks>
        public string PlayerHandle
        {
            get => _playerHandle;
            set
            {
                _playerHandle = value ?? string.Empty;
                Save();
            }
        }

        [SerializeField] protected SystemLanguage _language;

        /// <summary>
        /// Player's UNITY language.
        /// </summary>
        public SystemLanguage Language
        {
            get => _language;
            set
            {
                _language = value;
                Save();
            }
        }

        public bool HasLanguageCode => _language != SystemLanguage.Unknown;

        [SerializeField] protected bool _isTosAccepted;

        /// <summary>
        /// Is Terms Of Service accepted?
        /// </summary>
        public bool IsTosAccepted
        {
            get => _isTosAccepted;
            set
            {
                _isTosAccepted = value;
                Save();
            }
        }

        /// <summary>
        /// Player is considered to be valid when it has non-empty name, valid language code and ToS accepted.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(PlayerName) && HasLanguageCode && IsTosAccepted;

        /// <summary>
        /// Protected <c>Save</c> method to handle single property change.
        /// </summary>
        protected virtual void Save()
        {
            // Placeholder for actual implementation in derived class.
        }

        /// <summary>
        /// Public <c>BatchSave</c> method to save several properties at once.
        /// </summary>
        /// <param name="saveSettings">The action to save all properties in one go.</param>
        public virtual void BatchSave(Action saveSettings)
        {
            // Placeholder for actual implementation in derived class.
        }

        [Conditional("UNITY_EDITOR")]
        public void DebugResetPlayer()
        {
            // Actually can not delete - just invalidate everything!
            BatchSave(() =>
            {
                PlayerName = string.Empty;
                CharacterModelId = -1;
                Language = SystemLanguage.Unknown;
                IsTosAccepted = false;
            });
        }

        public override string ToString()
        {
            // This is required for actual implementation to detect changes in our changeable properties!
            return
                $"Name:{PlayerName}, ModelId:{CharacterModelId}, Lang {Language}, ToS {(IsTosAccepted ? 1 : 0)}, " +
                $"Valid {(IsValid ? 1 : 0)}, Guid:{PlayerHandle}";
        }
    }

    /// <summary>
    /// Runtime game config variables that can be referenced from anywhere safely and optionally can be changed on the fly.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>RuntimeGameConfig</c> can be synchronized over network.
    /// </remarks>
    public class RuntimeGameConfig : MonoBehaviour
    {
        public static RuntimeGameConfig Get()
        {
            var instance = FindObjectOfType<RuntimeGameConfig>();
            if (instance == null)
            {
                instance = UnityExtensions.CreateGameObjectAndComponent<RuntimeGameConfig>(nameof(RuntimeGameConfig), true);
                LoadGameConfig(instance);
            }
            return instance;
        }

#if UNITY_EDITOR
        public static PlayerDataCache GetPlayerDataCacheInEditor() => LoadPlayerDataCache();
#endif

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameConstraints _permanentConstraints;
        [SerializeField] private GameVariables _permanentVariables;
        [SerializeField] private GamePrefabs _permanentPrefabs;
        [SerializeField] private PlayerDataCache _playerDataCache;

        public GameFeatures Features
        {
            get => _permanentFeatures;
            set => _permanentFeatures.CopyFrom(value);
        }

        public GameConstraints GameConstraints
        {
            get => _permanentConstraints;
            set => _permanentConstraints.CopyFrom(value);
        }

        public GameVariables Variables
        {
            get => _permanentVariables;
            set => _permanentVariables.CopyFrom(value);
        }

        public GamePrefabs Prefabs
        {
            get => _permanentPrefabs;
            private set => _permanentPrefabs.CopyFrom(value);
        }

        public PlayerDataCache PlayerDataCache => _playerDataCache;

        private static void LoadGameConfig(RuntimeGameConfig instance)
        {
            // We can use models
            Storefront.Create();
            // Create default values
            instance._permanentFeatures = new GameFeatures();
            instance._permanentConstraints = new GameConstraints();
            instance._permanentVariables = new GameVariables();
            instance._permanentPrefabs = new GamePrefabs();
            // Set persistent values
            var gameSettings = Resources.Load<PersistentGameSettings>(nameof(PersistentGameSettings));
            instance.Features = gameSettings._features;
            instance._permanentConstraints = gameSettings._constraints;
            instance.Variables = gameSettings._variables;
            instance.Prefabs = gameSettings._prefabs;
            instance._playerDataCache = LoadPlayerDataCache();
        }

        private static PlayerDataCache LoadPlayerDataCache()
        {
            return new PlayerDataCacheLocal();
        }

        private class PlayerDataCacheLocal : PlayerDataCache
        {
            private const string PlayerNameKey = "PlayerData.PlayerName";
            private const string PlayerHandleKey = "PlayerData.PlayerHandle";
            private const string CharacterModelIdKey = "PlayerData.CharacterModelId";
            private const string LanguageCodeKey = "PlayerData.LanguageCode";
            private const string TermsOfServiceKey = "PlayerData.TermsOfService";

            private bool _isBatchSave;

            public PlayerDataCacheLocal()
            {
                _playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
                _characterModelId = PlayerPrefs.GetInt(CharacterModelIdKey, -1);
                _playerHandle = PlayerPrefs.GetString(PlayerHandleKey, string.Empty);
                if (string.IsNullOrWhiteSpace(PlayerHandle))
                {
                    _playerHandle = CreatePlayerHandle();
                    PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                    // Writes all modified preferences to disk.
                    PlayerPrefs.Save();
                }
                _language = (SystemLanguage)PlayerPrefs.GetInt(LanguageCodeKey, (int)SystemLanguage.Unknown);
                _isTosAccepted = PlayerPrefs.GetInt(TermsOfServiceKey, 0) == 1;
            }

            private static string CreatePlayerHandle()
            {
                // Create same GUID for same device if possible
                // - guid can be used to identify third party cloud game services
                // - we want to keep it constant for single device even this data is wiped e.g. during testing
                var deviceId = SystemInfo.deviceUniqueIdentifier;
                if (deviceId == SystemInfo.unsupportedIdentifier)
                {
                    return Guid.NewGuid().ToString();
                }
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Encoding.Unicode.GetBytes(deviceId));
                    return new Guid(hash).ToString();
                }
            }

            protected override void Save()
            {
                InternalSave();
            }

            public override void BatchSave(Action saveSettings)
            {
                _isBatchSave = true;
                saveSettings?.Invoke();
                _isBatchSave = false;
                InternalSave();
                // Writes all modified preferences to disk.
                PlayerPrefs.Save();
            }

            private void InternalSave()
            {
                if (_isBatchSave)
                {
                    return; // Defer saving until later
                }
                // By default Unity writes preferences to disk during OnApplicationQuit().
                PlayerPrefs.SetString(PlayerNameKey, PlayerName);
                PlayerPrefs.SetInt(CharacterModelIdKey, CharacterModelId);
                PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                PlayerPrefs.SetInt(LanguageCodeKey, (int)Language);
                PlayerPrefs.SetInt(TermsOfServiceKey, IsTosAccepted ? 1 : 0);
            }
        }
    }
}