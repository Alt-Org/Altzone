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

        public GameVariables Variables { get; }

        public PlayerPrefabs PlayerPrefabs { get; }

        public PlayerSettings PlayerSettings { get; }

        public Characters Characters { get; }

        private GameConfig()
        {
            PlayerSettings = new PlayerSettings();
            var settings = GameSettings.Load();
            Characters = settings._characters;
            Variables = CreateCopyFrom(settings._variables);
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
