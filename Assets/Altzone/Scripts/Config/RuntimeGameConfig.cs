using Altzone.Scripts.Config.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Runtime game config variables that can be referenced from anywhere safely and
    /// optionally can be changed on the fly without any side effects.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>RuntimeGameConfig</c> can be synchronized over network thus requiring a setter.
    /// </remarks>
    public interface IRuntimeGameConfig
    {
        GameVariables GameVariables { get; }
        IPlayerDataCache PlayerDataCache { get; }
        Characters Characters { get; }
    }

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

        public GameVariables GameVariables
        {
            get => _permanentVariables;
            set => _permanentVariables.CopyFrom(value);
        }

        public IPlayerDataCache PlayerDataCache => _playerDataCache;

        #region Data Store

        private IPlayerDataCache _playerDataCache;

        #endregion
        public Characters Characters 
        { 
            get => _characters;
        }
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

        private static void LoadGameConfig(RuntimeGameConfig instance)
        {
            instance._characters = new Characters();
            var setting = GameSettings.Load();
            instance._permanentVariables = setting._variables;
            instance._playerDataCache = LoadPlayerDataCache(instance);
        }

        private static PlayerDataCache LoadPlayerDataCache(MonoBehaviour host)
        {
            return new PlayerDataCacheLocal(host);
        }
    }
}
