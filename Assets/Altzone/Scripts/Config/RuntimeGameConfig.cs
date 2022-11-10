using System;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    public interface IRuntimeGameConfig
    {
        IPlayerDataCache PlayerDataCache { get; }
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
    }

    /// <summary>
    /// Well known prefabs for the game.
    /// </summary>
    [Serializable]
    public class GamePrefabs
    {
        public void CopyFrom(GamePrefabs other)
        {
            PropertyCopier<GamePrefabs, GamePrefabs>.CopyFields(other, this);
        }
    }

    /// <summary>
    /// New Input System Package for Player actions.
    /// </summary>
    [Serializable]
    public class GameInput
    {
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

    public class RuntimeGameConfig : MonoBehaviour, IRuntimeGameConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static RuntimeGameConfig _instance;

        public static IRuntimeGameConfig Get()
        {
            if (_instance == null)
            {
                _instance = UnitySingleton.CreateStaticSingleton<RuntimeGameConfig>();
                LoadGameConfig(_instance);
                Debug.Log($"{_instance.name}");
            }
            return _instance;
        }

        public IPlayerDataCache PlayerDataCache => _playerDataCache;

        #region Data Store

        private IPlayerDataCache _playerDataCache;

        #endregion

        #region UNITY Editor

        [SerializeField] private GameFeatures _permanentFeatures;
        [SerializeField] private GameConstraints _permanentConstraints;
        [SerializeField] private GameVariables _permanentVariables;
        [SerializeField] private BattleUiConfig _battleUiConfig;
        [SerializeField] private GamePrefabs _permanentPrefabs;
        [SerializeField] private GameInput _gameInput;
        [SerializeField] private Characters _characters;

        #endregion

        private void Awake()
        {
            Debug.Log($"{name}");
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Used by Editor classes with <c>MenuItem</c> to pre-load <c>PlayerDataCache</c> as it is not otherwise available.
        /// </summary>
        public static IPlayerDataCache GetPlayerDataCacheInEditor() => Get().PlayerDataCache;
#endif

        private static void LoadGameConfig(RuntimeGameConfig instance)
        {
            instance._characters = new Characters();
            instance._playerDataCache = LoadPlayerDataCache(instance);
        }

        private static PlayerDataCache LoadPlayerDataCache(MonoBehaviour host)
        {
            return new PlayerDataCacheLocal(host);
        }
    }
}