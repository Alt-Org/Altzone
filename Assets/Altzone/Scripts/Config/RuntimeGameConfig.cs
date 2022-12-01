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

    public class RuntimeGameConfig : IRuntimeGameConfig
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
                _instance = new RuntimeGameConfig();
            }
            return _instance;
        }

        public GameVariables GameVariables
        {
            get => _permanentVariables;
            set => _permanentVariables.CopyFrom(value);
        }

        public IPlayerDataCache PlayerDataCache { get; private set; }

        public Characters Characters { get; private set; }

        #region Private serializable variables

        private GameFeatures _permanentFeatures;
        private GameConstraints _permanentConstraints;
        private readonly GameVariables _permanentVariables;
        private BattleUiConfig _battleUiConfig;
        private GamePrefabs _permanentPrefabs;
        private GameInput _gameInput;

        #endregion

        #region Data Store

        #endregion

        private RuntimeGameConfig()
        {
            PlayerDataCache = new PlayerDataCacheLocal();
            var setting = GameSettings.Load();
            Characters = setting._characters;
            _permanentVariables = setting._variables;
        }
    }
}