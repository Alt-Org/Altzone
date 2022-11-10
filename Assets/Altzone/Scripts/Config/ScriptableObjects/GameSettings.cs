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
        private static string GameSettingsName = "GameSettings";

        [Header("Game Variables")] public GameVariables _variables;

        internal static GameSettings Load()
        {
            var gameSettings = Resources.Load<GameSettings>(GameSettingsName);
            Assert.IsNotNull(gameSettings, $"ASSET '{GameSettingsName}' NOT FOUND");
            return gameSettings;
        }
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
}