using System;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle0.Scripts
{
    /// <summary>
    /// Runtime game config variables that can be referenced from anywhere safely and optionally can be changed on the fly.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>RuntimeGameConfig</c> can be synchronized over network.
    /// </remarks>
    public interface IRuntimeGameConfig
    {
        GameFeatures Features { get; set; }
        GameConstraints GameConstraints { get; }
        GameVariables Variables { get; set; }
        BattleUiConfig BattleUi { get; }
        GamePrefabs Prefabs { get; }
        GameInput Input { get; }
    }

    #region RuntimeGameConfig "Parts"

    /// <summary>
    /// Game features that can be toggled on and off.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameFeatures
    {
        #region Battle Game

        /// <summary>
        /// Rotate game camera for upper team so they see their own game area in lower part of the screen.
        /// </summary>
        [Header("Battle Game"), Tooltip("Rotate game camera for upper team so they see their own game area in lower part of the screen")]
        public bool _isRotateGameCamera;

        /// <summary>
        /// Rotate game background for upper team so they see their own game area color in lower part of the screen.
        /// </summary>
        [Tooltip("Rotate game background for upper team so they see their own game area color in lower part of the screen")]
        public bool _isRotateGameBackground;

        /// <summary>
        /// Disable grid based player movement
        /// </summary>
        [Tooltip("Enable grid based player movement")]
        public bool _useBattleGridMovement;

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Is sounds enabled.
        /// </summary>
        [Header("Miscellaneous"), Tooltip("Disable (mute) all sounds")]
        public bool _isMuteAllSounds;

        #endregion

        #region Battle Testing

        /// <summary>
        /// Disable player <c>SetPlayMode</c> calls when ball goes over team's gameplay area.
        /// </summary>
        [Header("Battle Testing"), Tooltip("Disable player SetPlayMode calls when ball goes over team's gameplay area")]
        public bool _isDisablePlayModeChanges;

        /// <summary>
        /// Enable Frozen PlayMode.
        /// </summary>
        [Tooltip("Enable going to Frozen PlayMode when ball is on team's gameplay area")]
        public bool _isFrozenPlayModeOn;

        /// <summary>
        /// Disable player shield state changes when ball hits the shield.
        /// </summary>
        [Tooltip("Disable player shield state changes when ball hits the shield")]
        public bool _isDisableShieldStateChanges;

        /// <summary>
        /// Disable ball speed changes when ball collides with shield.
        /// </summary>
        [Tooltip("Disable ball speed changes when ball collides with shield")]
        public bool _isDisableBallSpeedChanges;

        /// <summary>
        /// Disable team forfeit when last team player leaves the room.
        /// </summary>
        [Tooltip("Disable team forfeit when last team player leaves the room")]
        public bool _isDisableTeamForfeit;

        /// <summary>
        /// Is shield always on when team has only one player.
        /// </summary>
        [Tooltip("Is shield always on when team has only one player")]
        public bool _isSinglePlayerShieldOn;

        /// <summary>
        /// Disable RAID gameplay from BATTLE.
        /// </summary>
        [Tooltip("Disable RAID gameplay from BATTLE")]
        public bool _isDisableRaid;

        /// <summary>
        /// Is bricks visible.
        /// </summary>
        [Tooltip("Disable bricks")]
        public bool _isDisableBricks;

        #endregion

        #region Battle Scene UI

        /// <summary>
        /// Settings for Battle Scene UI Grid Overlay.
        /// </summary>
        [Header("Battle Scene UI Grid"), Tooltip("Disable Grid Overlay on Battle Scene")]
        public bool _isDisableBattleUiGrid;

        #endregion

        public void CopyFrom(GameFeatures other)
        {
            PropertyCopier<GameFeatures, GameFeatures>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Game constraints that that control the workings of the game.
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
    /// <remarks>
    /// Note that these member variables can be serialized over network using our <c>BinarySerializer</c>.
    /// </remarks>
    [Serializable]
    public class GameVariables
    {
        [Header("Battle"), Min(1)] public int _battleRoomStartDelay;
        [Min(1)] public int _battleSlingshotDelay;
        [Min(0)] public int _battleHeadScoreToWin;
        [Min(1)] public int _battleUiGridWidth;
        [Min(1)] public int _battleUiGridHeight;

        [Header("Raid"), Min(0)] public float _raidMaxLootCapacity;

        [Header("Ball"), Min(0)] public float _ballMoveSpeedMultiplier;
        [Min(0)] public float _ballMinMoveSpeed;
        [Min(0)] public float _ballMaxMoveSpeed;
        [Min(0)] public float _ballLerpSmoothingFactor;
        [Min(0)] public float _ballTeleportDistance;
        [Min(0)] public float _ballSlingshotPower;
        [Min(0)] public float _ballIdleAccelerationStartDelay;
        [Min(0)] public float _ballIdleAccelerationInterval;
        [Min(0)] public float _ballIdleAccelerationMultiplier;

        [Header("Player"), Min(0)] public float _playerMoveSpeedMultiplier;
        [Min(0)] public float _playerAttackMultiplier;
        [Min(0), Tooltip("Stun duration in seconds after shield hit")] public float _playerShieldHitStunDuration;
        [Min(0), Tooltip("Delay in seconds when movement starts after movement request has been made")] public float _playerMovementNetworkDelay;

        [Header("Shield"), Min(0)] public float _shieldDistanceMultiplier;

        public void CopyFrom(GameVariables other)
        {
            PropertyCopier<GameVariables, GameVariables>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// Battle game UI configuration.
    /// </summary>
    [Serializable]
    public class BattleUiConfig
    {
        [Min(0)] public float _battleUiGridLineWidth;
        public Color _battleUiGridColor;
    }

    /// <summary>
    /// Well known prefabs for the game.
    /// </summary>
    [Serializable]
    public class GamePrefabs
    {
        [Header("Battle Player Prefabs")] public GameObject _playerForDes;
        public GameObject _playerForDef;
        public GameObject _playerForInt;
        public GameObject _playerForPro;
        public GameObject _playerForRet;
        public GameObject _playerForEgo;
        public GameObject _playerForCon;

        [Header("Battle Shield Prefabs")] public GameObject _shieldForDes;
        public GameObject _shieldForDef;
        public GameObject _shieldForInt;
        public GameObject _shieldForPro;
        public GameObject _shieldForRet;
        public GameObject _shieldForEgo;
        public GameObject _shieldForCon;

        public void CopyFrom(GamePrefabs other)
        {
            PropertyCopier<GamePrefabs, GamePrefabs>.CopyFields(other, this);
        }

        public GameObject GetPlayerPrefab(Defence defence)
        {
            switch (defence)
            {
                case Defence.Desensitisation:
                    return _playerForDes;
                case Defence.Deflection:
                    return _playerForDef;
                case Defence.Introjection:
                    return _playerForInt;
                case Defence.Projection:
                    return _playerForPro;
                case Defence.Retroflection:
                    return _playerForRet;
                case Defence.Egotism:
                    return _playerForEgo;
                case Defence.Confluence:
                    return _playerForCon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defence), defence, null);
            }
        }

        public GameObject GetShieldPrefab(Defence defence)
        {
            switch (defence)
            {
                case Defence.Desensitisation:
                    return _shieldForDes;
                case Defence.Deflection:
                    return _shieldForDef;
                case Defence.Introjection:
                    return _shieldForInt;
                case Defence.Projection:
                    return _shieldForPro;
                case Defence.Retroflection:
                    return _shieldForRet;
                case Defence.Egotism:
                    return _shieldForEgo;
                case Defence.Confluence:
                    return _shieldForCon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defence), defence, null);
            }
        }
    }

    /// <summary>
    /// New Input System Package for Player actions.
    /// </summary>
    [Serializable]
    public class GameInput
    {
        [Header("Player Input Actions")] public InputActionReference _clickInputAction;
        public InputActionReference _moveInputAction;
    }

    ///<summary>
    /// Character model attribute editing for Unity Editor
    /// </summary>    
    [Serializable]
    public class Characters
    {
        [Header("Character Model Attributes")] public string _name;
        public Defence _mainDefence;
        [Range(0, 10)] public int _speed;
        [Range(0, 10)] public int _resistance;
        [Range(0, 10)] public int _attack;
        [Range(0, 10)] public int _defence;
    }

    #endregion

    public class Battle0GameConfig : MonoBehaviour, IRuntimeGameConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static Battle0GameConfig _instance;

        public static IRuntimeGameConfig Get()
        {
            if (_instance == null)
            {
                _instance = UnitySingleton.CreateStaticSingleton<Battle0GameConfig>();
                LoadGameConfig(_instance);
                Debug.Log($"{_instance.name}");
            }
            return _instance;
        }

        public static bool IsFirstTimePlaying => PlayerPrefs.GetInt(PlayerPrefKeys.IsFirstTimePlaying, 1) == 1;

        public static void RemoveIsFirstTimePlayingStatus() => PlayerPrefs.SetInt(PlayerPrefKeys.IsFirstTimePlaying, 0);

        #region UNITY Editor

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameConstraints _permanentConstraints;
        [SerializeField] private GameVariables _permanentVariables;
        [SerializeField] private BattleUiConfig _battleUiConfig;
        [SerializeField] private GamePrefabs _permanentPrefabs;
        [SerializeField] private GameInput _gameInput;

        #endregion

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
        }

        /// <summary>
        /// Game features that can be toggled on and off.
        /// </summary>
        public GameFeatures Features
        {
            get => _permanentFeatures;
            set => _permanentFeatures.CopyFrom(value);
        }

        /// <summary>
        /// Unmodifiable Game constraints that that control the workings of the game.
        /// </summary>
        public GameConstraints GameConstraints => _permanentConstraints;

        /// <summary>
        /// Game variables that control game play somehow.
        /// </summary>
        public GameVariables Variables
        {
            get => _permanentVariables;
            set => _permanentVariables.CopyFrom(value);
        }

        /// <summary>
        /// Unmodifiable Battle UI config.
        /// </summary>
        public BattleUiConfig BattleUi => _battleUiConfig;

        /// <summary>
        /// Well known prefabs for the game.
        /// </summary>
        public GamePrefabs Prefabs
        {
            get => _permanentPrefabs;
            private set => _permanentPrefabs.CopyFrom(value);
        }

        /// <summary>
        /// New Input System Package for Player actions.
        /// </summary>
        public GameInput Input => _gameInput;

        private static void LoadGameConfig(Battle0GameConfig instance)
        {
            // Create default values
            instance._permanentFeatures = new GameFeatures();
            instance._permanentConstraints = new GameConstraints();
            instance._permanentVariables = new GameVariables();
            instance._battleUiConfig = new BattleUiConfig();
            instance._permanentPrefabs = new GamePrefabs();
            // Set persistent values
            var gameSettings = Resources.Load<PersistentGameSettings>(nameof(PersistentGameSettings));
            instance.Features = gameSettings._features;
            instance._permanentConstraints = gameSettings._constraints;
            instance.Variables = gameSettings._variables;
            instance._battleUiConfig = gameSettings._battleUiConfig;
            instance.Prefabs = gameSettings._prefabs;
            instance._gameInput = gameSettings._input;
        }
    }
}