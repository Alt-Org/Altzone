using System;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Util;
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

        [Header("Game Variables")] public GameVariables _variables;

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
        public static string Koulukiusaaja;
        [Header ("Koulukiusaaja")] public Defence _mainDefence1;
        [Range(0, 10)] public int _speed1;
        [Range(0, 10)] public int _resistance1;
        [Range(0, 10)] public int _attack1;
        [Range(0, 10)] public int _defence1;

        public static string Vitsiniekka;
        [Header ("Vitsiniekka")]public Defence _mainDefence2;
        [Range(0, 10)] public int _speed2;
        [Range(0, 10)] public int _resistance2;
        [Range(0, 10)] public int _attack2;
        [Range(0, 10)] public int _defence2;
        
        public static string Pappi;
        [Header ("Pappi")] public Defence _mainDefence3;
        [Range(0, 10)] public int _speed3;
        [Range(0, 10)] public int _resistance3;
        [Range(0, 10)] public int _attack3;
        [Range(0, 10)] public int _defence3;
    
        public static string Taiteilija;
        [Header ("Taiteilija")] public Defence _mainDefence4;
        [Range(0, 10)] public int _speed4;
        [Range(0, 10)] public int _resistance4;
        [Range(0, 10)] public int _attack4;
        [Range(0, 10)] public int _defence4;

        public static string Hodariläski;
        [Header ("Hodariläski")]public Defence _mainDefence5;
        [Range(0, 10)] public int _speed5;
        [Range(0, 10)] public int _resistance5;
        [Range(0, 10)] public int _attack5;
        [Range(0, 10)] public int _defence5;
        
        public static string Älykkö;
        [Header ("Älykkö")] public Defence _mainDefence6;
        [Range(0, 10)] public int _speed6;
        [Range(0, 10)] public int _resistance6;
        [Range(0, 10)] public int _attack6;
        [Range(0, 10)] public int _defence6;
        
        public static string Tytöt;
        [Header ("Tytöt")] public Defence _mainDefence7;
        [Range(0, 10)] public int _speed7;
        [Range(0, 10)] public int _resistance7;
        [Range(0, 10)] public int _attack7;
        [Range(0, 10)] public int _defence7; 
   }

    #endregion
}
