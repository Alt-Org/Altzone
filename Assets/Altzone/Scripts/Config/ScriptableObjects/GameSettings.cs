using System;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Editable persistent settings for the game.
    /// </summary>
    /// <remarks>
    /// Create these in <c>Resources</c> folder with name "GameSettings" so they can be loaded when needed first time.
    /// </remarks>
    // [CreateAssetMenu(menuName = "ALT-Zone/GameSettings", fileName = "GameSettings")]
    internal class GameSettings : ScriptableObject
    {
        private const string GameSettingsName = "GameSettings";

        [Header("Game Features")] public GameFeatures _features;

        [Header("Game Constants")] public GameConstants _constants;

        [Header("Game Variables")] public GameVariables _variables;

        [Header("Player Prefabs")] public PlayerPrefabs _playerPrefabs;

        [Header("Characters")] public Characters _characters;

        internal static GameSettings Load()
        {
            var gameSettings = Resources.Load<GameSettings>(GameSettingsName);
            Assert.IsNotNull(gameSettings, $"ASSET '{GameSettingsName}' NOT FOUND");
            return gameSettings;
        }
    }

    #region GameSettings "Parts"

    /// <summary>
    /// Game features that can be toggled on and off.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameFeatures
    {
    }

    /// <summary>
    /// Game constraints that that control the workings of the game.
    /// </summary>
    [Serializable]
    public class GameConstants
    {
        [Header("Furniture")] public string _furniturePrefabFolder;
    }

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameVariables
    {
    }

    /// <summary>
    /// Player prefabs in simple array.
    /// </summary>
    /// <remarks>
    /// Note that prefabId is the same as array index.
    /// </remarks>
    [Serializable]
    public class PlayerPrefabs
    {
        public GameObject[] _playerPrefabs;

        public GameObject GetPlayerPrefab(int prefabId)
        {
            Assert.IsTrue(prefabId >= 0 && prefabId < _playerPrefabs.Length);
            return _playerPrefabs[prefabId];
        }
    }

    ///<summary>
    /// Character model attribute editing for Unity Editor
    /// </summary>  
    [Serializable]
    public class Characters
    {
        public static string Koulukiusaaja;
        [Header("Koulukiusaaja")] public Defence _mainDefence1;
        [Range(0, 10)] public int _speed1;
        [Range(0, 10)] public int _resistance1;
        [Range(0, 10)] public int _attack1;
        [Range(0, 10)] public int _defence1;

        public static string Vitsiniekka;
        [Header("Vitsiniekka")] public Defence _mainDefence2;
        [Range(0, 10)] public int _speed2;
        [Range(0, 10)] public int _resistance2;
        [Range(0, 10)] public int _attack2;
        [Range(0, 10)] public int _defence2;

        public static string Pappi;
        [Header("Pappi")] public Defence _mainDefence3;
        [Range(0, 10)] public int _speed3;
        [Range(0, 10)] public int _resistance3;
        [Range(0, 10)] public int _attack3;
        [Range(0, 10)] public int _defence3;

        public static string Taiteilija;
        [Header("Taiteilija")] public Defence _mainDefence4;
        [Range(0, 10)] public int _speed4;
        [Range(0, 10)] public int _resistance4;
        [Range(0, 10)] public int _attack4;
        [Range(0, 10)] public int _defence4;

        public static string Hodariläski;
        [Header("Hodariläski")] public Defence _mainDefence5;
        [Range(0, 10)] public int _speed5;
        [Range(0, 10)] public int _resistance5;
        [Range(0, 10)] public int _attack5;
        [Range(0, 10)] public int _defence5;

        public static string Älykkö;
        [Header("Älykkö")] public Defence _mainDefence6;
        [Range(0, 10)] public int _speed6;
        [Range(0, 10)] public int _resistance6;
        [Range(0, 10)] public int _attack6;
        [Range(0, 10)] public int _defence6;

        public static string Tytöt;
        [Header("Tytöt")] public Defence _mainDefence7;
        [Range(0, 10)] public int _speed7;
        [Range(0, 10)] public int _resistance7;
        [Range(0, 10)] public int _attack7;
        [Range(0, 10)] public int _defence7;

        [Header("NewCharacter")] public string[] _name;
        public Defence[] _mainDefence8;
        [Range(0, 10)] public int[] _speed8;
        [Range(0, 10)] public int[] _resistance8;
        [Range(0, 10)] public int[] _attack8;
        [Range(0, 10)] public int[] _defence8;

        public void LoadKoulukiusaaja(int _Speed1, int _Resistance1, int _Attack1, int _Defence1)
        {
            Koulukiusaaja = "Koulukiusaaja";
            _mainDefence1 = Defence.Desensitisation;
            _speed1 = _Speed1;
            _resistance1 = _Resistance1;
            _attack1 = _Attack1;
            _defence1 = _Defence1;
        }

        public void LoadVitsiniekka(int _Speed2, int _Resistance2, int _Attack2, int _Defence2)
        {
            Vitsiniekka = "Vitsiniekka";
            _mainDefence2 = Defence.Deflection;
            _speed2 = _Speed2;
            _resistance2 = _Resistance2;
            _attack2 = _Attack2;
            _defence2 = _Defence2;
        }

        public void LoadPappi(int _Speed3, int _Resistance3, int _Attack3, int _Defence3)
        {
            Pappi = "Pappi";
            _mainDefence3 = Defence.Introjection;
            _speed3 = _Speed3;
            _resistance3 = _Resistance3;
            _attack3 = _Attack3;
            _defence3 = _Defence3;
        }

        public void LoadTaiteilija(int _Speed4, int _Resistance4, int _Attack4, int _Defence4)
        {
            Taiteilija = "Taiteilija";
            _mainDefence4 = Defence.Projection;
            _speed4 = _Speed4;
            _resistance4 = _Resistance4;
            _attack4 = _Attack4;
            _defence4 = _Defence4;
        }

        public void LoadHodariläski(int _Speed5, int _Resistance5, int _Attack5, int _Defence5)
        {
            Hodariläski = "Hodariläski";
            _mainDefence5 = Defence.Retroflection;
            _speed5 = _Speed5;
            _resistance5 = _Resistance5;
            _attack5 = _Attack5;
            _defence5 = _Defence5;
        }

        public void LoadÄlykkö(int _Speed6, int _Resistance6, int _Attack6, int _Defence6)
        {
            Älykkö = "Älykkö";
            _mainDefence6 = Defence.Egotism;
            _speed6 = _Speed6;
            _resistance6 = _Resistance6;
            _attack6 = _Attack6;
            _defence6 = _Defence6;
        }

        public void LoadTytöt(int _Speed7, int _Resistance7, int _Attack7, int _Defence7)
        {
            Tytöt = "Tytöt";
            _mainDefence7 = Defence.Confluence;
            _speed7 = _Speed7;
            _resistance7 = _Resistance7;
            _attack7 = _Attack7;
            _defence7 = _Defence7;
        }

        public void LoadNewCharacter(string[] _name, Defence[] _mainDefence8, int[] _Speed8, int[] _Resistance8, int[] _Attack8, int[] _Defence8)
        {
            /*_name[] = {"Koodari",};
            _mainDefence8[] ={Defence.Egotism,};
            _speed8 =_Speed8;
            _resistance8 = _Resistance8;
            _attack8 = _Attack8;
            _defence8 = _Defence8;*/
        }
    }

    #endregion
}