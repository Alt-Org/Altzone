using Altzone.Scripts.Config.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    public interface IRuntimeGameConfig
    {
        IPlayerDataCache PlayerDataCache { get; }
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