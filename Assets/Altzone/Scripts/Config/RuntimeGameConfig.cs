using System;
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
        public bool isRotateGameCamera;

        /// <summary>
        /// Is local player team color always "blue" team color.
        /// </summary>
        public bool isLocalPLayerOnTeamBlue;

        /// <summary>
        /// Spawn mini ball aka diamonds.
        /// </summary>
        public bool isSPawnMiniBall;

        /// <summary>
        /// Is shield always on when team has only one player (for testing).
        /// </summary>
        public bool isSinglePlayerShieldOn;

        public void CopyFrom(GameFeatures other)
        {
            PropertyCopier<GameFeatures, GameFeatures>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    [Serializable]
    public class GameVariables
    {
        [Header("Battle"), Min(1)] public int roomStartDelay;

        [Header("Ball")] public float ballMoveSpeedMultiplier;
        public float ballLerpSmoothingFactor;
        public float ballTeleportDistance;
        public float minSlingShotDistance;
        public float maxSlingShotDistance;
        [Min(1)] public int ballRestartDelay;

        [Header("Player")] public float playerMoveSpeedMultiplier;
        public float playerSqrMinRotationDistance;
        public float playerSqrMaxRotationDistance;

        [Header("Shield")] public float shieldDistanceMultiplier;

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
        [Header("Battle")] public GameObject playerForDes;
        public GameObject playerForDef;
        public GameObject playerForInt;
        public GameObject playerForPro;
        public GameObject playerForRet;
        public GameObject playerForEgo;
        public GameObject playerForCon;

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
                if (_playerName != value)
                {
                    _playerName = value ?? "";
                    Save();
                }
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
                if (_characterModelId != value)
                {
                    _characterModelId = value;
                    Save();
                }
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
            new CharacterModel(-1, "Dummy", Defence.Desensitisation, 0, 0, 0, 0);

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
                if (_playerHandle != value)
                {
                    _playerHandle = value ?? "";
                    Save();
                }
            }
        }

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

        public override string ToString()
        {
            // This is required for actual implementation to detect changes in our properties!
            return $"Name:{PlayerName}, C-ModelId:{CharacterModelId}, GUID:{PlayerHandle}";
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
            if (_instance == null)
            {
                _instance = FindObjectOfType<RuntimeGameConfig>();
                if (_instance == null)
                {
                    _instance = UnityExtensions.CreateGameObjectAndComponent<RuntimeGameConfig>(nameof(RuntimeGameConfig), true);
                    LoadGameConfig();
                }
            }
            return _instance;
        }

        private static RuntimeGameConfig _instance;

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameVariables _permanentVariables;
        [SerializeField] private GamePrefabs _permanentPrefabs;
        [SerializeField] private PlayerDataCache _playerDataCache;

        public GameFeatures Features
        {
            get => _permanentFeatures;
            set => _permanentFeatures.CopyFrom(value);
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

        private static void LoadGameConfig()
        {
            // We can use models
            Storefront.Create();
            // Create default values
            _instance._permanentFeatures = new GameFeatures();
            _instance._permanentVariables = new GameVariables();
            _instance._permanentPrefabs = new GamePrefabs();
            // Set persistent values
            var gameSettings = Resources.Load<PersistentGameSettings>(nameof(PersistentGameSettings));
            _instance.Features = gameSettings._features;
            _instance.Variables = gameSettings._variables;
            _instance.Prefabs = gameSettings._prefabs;
            _instance._playerDataCache = LoadPlayerDataCache();
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

            private bool _isBatchSave;
            private string _currentState;

            public PlayerDataCacheLocal()
            {
                _playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
                if (string.IsNullOrWhiteSpace(_playerName))
                {
                    _playerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                    PlayerPrefs.SetString(PlayerNameKey, _playerName);
                }
                _characterModelId = PlayerPrefs.GetInt(CharacterModelIdKey, -1);
                _playerHandle = PlayerPrefs.GetString(PlayerHandleKey, string.Empty);
                if (string.IsNullOrWhiteSpace(PlayerHandle))
                {
                    _playerHandle = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                }
                _currentState = ToString();
            }

            public sealed override string ToString()
            {
                // https://www.jetbrains.com/help/rider/VirtualMemberCallInConstructor.html
                return base.ToString();
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
            }

            private void InternalSave()
            {
                if (_isBatchSave)
                {
                    return; // Defer saving until later
                }
                if (_currentState == ToString())
                {
                    return; // Skip saving when nothing has changed
                }
                PlayerPrefs.SetString(PlayerNameKey, PlayerName);
                PlayerPrefs.SetInt(CharacterModelIdKey, CharacterModelId);
                PlayerPrefs.SetString(PlayerHandleKey, PlayerHandle);
                _currentState = ToString();
            }
        }
    }
}