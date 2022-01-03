using System;
using Altzone.Scripts.Config.ScriptableObjects;
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
        /// Is local player team color always "blue" team side.
        /// </summary>
        public bool _isRotateGamePlayArea;

        /// <summary>
        /// Spawn mini ball aka diamonds.
        /// </summary>
        public bool _isSPawnMiniBall;

        /// <summary>
        /// Is shield always on when team has only one player (for testing).
        /// </summary>
        public bool _isSinglePlayerShieldOn;

        /// <summary>
        /// Is bricks visible.
        /// </summary>
        public bool _isBricksVisible;

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
        public int _headScoreToWin;
        public int _wallScoreToWin;

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
    /// Runtime game config variables that can be referenced from anywhere safely and optionally can be changed on the fly.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>RuntimeGameConfig</c> can be synchronized over network.
    /// </remarks>
    public class RuntimeGameConfig : MonoBehaviour
    {
        internal const string IsFirsTimePlayingKey = "PlayerData.IsFirsTimePlaying";

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

        public static bool IsFirsTimePlaying => PlayerPrefs.GetInt(IsFirsTimePlayingKey, 1) == 1;

        public static void RemoveIsFirsTimePlayingStatus() => PlayerPrefs.SetInt(IsFirsTimePlayingKey, 0);

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
    }
}