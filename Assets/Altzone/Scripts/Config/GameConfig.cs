using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Settings;
using UnityEngine;

namespace Altzone.Scripts.Config
{
    public enum VersionType
    {
        None,
        Standard,
        Education
    }

    public class GameConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        private static GameConfig _instance;

        private VersionType _gameVersionType = VersionType.None;

        public static GameConfig Get() => _instance ??= new GameConfig();

        public VersionType GameVersionType { get => _gameVersionType;
            set
            {
                //if(_gameVersionType == VersionType.None)
                _gameVersionType = value;
                PlayerPrefs.SetInt("Version", (int)_gameVersionType);
            }
        }

        public GameVariables Variables => throw new UnityException("GameVariables is obsolete");

        public PlayerPrefabs PlayerPrefabs => throw new UnityException("PlayerPrefabs is obsolete");

        public PlayerSettings PlayerSettings { get; }

        public Characters Characters => throw new UnityException("Characters is obsolete");

        private GameConfig()
        {
            PlayerSettings = new PlayerSettings();
            _gameVersionType = (VersionType)PlayerPrefs.GetInt("Version", 2);
        }
    }
}
