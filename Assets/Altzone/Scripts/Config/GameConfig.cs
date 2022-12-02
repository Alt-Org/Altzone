using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Runtime <c>IGameConfig</c> variables that can be referenced from anywhere safely and
    /// optionally can be changed on the fly without any side effects.
    /// </summary>
    /// <remarks>
    /// Note that some parts of <c>GameConfig</c> can be synchronized over network thus requiring a setter to update its local state.<br />
    /// Network synchronization can only work for selected data types with public properties. See <c>BinarySerializer</c> for more.
    /// </remarks>
    public interface IGameConfig
    {
        GameVariables GameVariables { get; }
        IPlayerDataCache PlayerDataCache { get; }
        Characters Characters { get; }
    }

    public class GameConfig : IGameConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static GameConfig _instance;

        public static IGameConfig Get()
        {
            if (_instance == null)
            {
                _instance = new GameConfig();
            }
            return _instance;
        }

        public GameVariables GameVariables
        {
            get => _permanentVariables;
            set => UpdateFrom(value, _permanentVariables);
        }

        public IPlayerDataCache PlayerDataCache { get; }

        public Characters Characters { get; }

        #region Private serializable variables

        private GameFeatures _permanentFeatures;
        private GameConstraints _permanentConstraints;
        private readonly GameVariables _permanentVariables;
        private BattleUiConfig _battleUiConfig;
        private GamePrefabs _permanentPrefabs;
        private GameInput _gameInput;

        #endregion

        private GameConfig()
        {
            PlayerDataCache = Altzone.Scripts.Config.PlayerDataCache.Create();
            var setting = GameSettings.Load();
            Characters = setting._characters;
            _permanentVariables = CreateCopyFrom(setting._variables);
        }

        private static T CreateCopyFrom<T>(T source) where T : class, new()
        {
            var target = new T();
            PropertyCopier<T, T>.CopyFields(source, target);
            return target;
        }

        private static void UpdateFrom<T>(T source, T target) where T : class
        {
            PropertyCopier<T, T>.CopyFields(source, target);
        }
    }
}