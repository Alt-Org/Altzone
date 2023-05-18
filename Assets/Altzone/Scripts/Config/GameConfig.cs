using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Settings;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    public class GameConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static GameConfig _instance;

        public static GameConfig Get() => _instance ??= new GameConfig();

        public GameVariables Variables
        {
            get => _gameVariables;
            set => UpdateFrom(value, _gameVariables);
        }

        public PlayerPrefabs PlayerPrefabs { get; }

        public IPlayerSettings PlayerSettings { get; }

        public Characters Characters { get; }

        #region Private serializable variables

        private readonly GameVariables _gameVariables;

        #endregion

        private GameConfig()
        {
            PlayerSettings = Settings.PlayerSettings.Create();
            var settings = GameSettings.Load();
            Characters = settings._characters;
            _gameVariables = CreateCopyFrom(settings._variables);
            PlayerPrefabs = settings._playerPrefabs;
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
