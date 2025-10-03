using System;
using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model.Poco.Game;
using Prg.Scripts.EditorSupport.Attributes;
using UnityEngine;

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
        [SerializeField, Header("Väri Esimerkki"), ColorHtmlProperty] private Color _ihanMalliksiVaan;
    }

    #region GameSettings "Parts"

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameVariables
    {
        [Header("Battle")]
        [Min(0), Tooltip("The amount of time in seconds that players have for aiming the sling")] public float _slingAimingTimeSec;
        [Min(1), Tooltip("Ball movement and character turning is limited to certain angles")] public float _angleLimit;
        [Min(0), Tooltip("A delay in seconds that can be used to give network messages enough time to arrive")] public float _networkDelay;

        [Header("Battle Player")]
        [Min(0)] public float _playerMoveSpeedMultiplier;
        [Min(0)] public float _playerAttackMultiplier;
        [Min(0), Tooltip("How far from shield collision should raycast take place to compensate for high speed")] public float _ballSpeedCompensation;
        [Min(0), Tooltip("How many hits does a shield take before deforming")] public int _shieldResistance;
        [Min(0), Tooltip("Small delay after shield has been hit, before the shield deforms")] public float _shieldDeformDelay;
        [Min(0), Tooltip("Adds a small delay after shield has been hit, so hits from multiple colliders dont register")] public float _shieldHitDelay;
    }

    /// <summary>
    /// Player prefabs in simple array.
    /// </summary>
    /// <remarks>
    /// Note that prefabKey (string) must be the same as corresponding array index (int).
    /// </remarks>

    [Serializable]
    public class Characters
    {
        [Header("Player Characters")] public List<Class> _characters;

        public PlayerActorBase GetPlayerPrefab(CharacterID characterID)
        {
            CharacterClassType characterClass = CustomCharacter.GetClass(characterID);

            int classValue = (int)characterClass >> 8;
            classValue--;

            if (classValue < 0 || classValue >= _characters.Count)
            {
                return null;
            }
            Class classObject = _characters[classValue];

            int characterValue = CustomCharacter.GetInsideCharacterID(characterID);
            characterValue--;

            if (characterValue < 0 || characterValue >= classObject.characters.Count)
            {
                return null;
            }
            Character character = classObject.characters[characterValue];
            return character._battlePrefab;
        }
    }

    [Serializable]
    public class PlayerPrefabs
    {
        [Header("Battle Player Prefabs")] public PlayerActorBase[] _playerPrefabs;

        public PlayerActorBase GetPlayerPrefab(int prefabId)
        {

            if (prefabId < 0 || prefabId >= _playerPrefabs.Length)
            {
                return null;
            }
            return _playerPrefabs[prefabId];
        }
    }

    ///<summary>
    /// Character model attribute editing for Unity Editor
    /// </summary>
    [Serializable]
    public class Class
    {
        [Header("Class")] public CharacterClassType _mainDefence;
        public List<Character> characters = new List<Character>();

    }

    [Serializable]
    public class Character
    {
        public string Name;
        public CharacterID _character;
        public PlayerActorBase _battlePrefab;
        [Range(0, 10)] public int _hp;
        [Range(0, 10)] public int _speed;
        [Range(0, 10)] public int _characterSize;
        [Range(0, 10)] public int _attack;
        [Range(0, 10)] public int _defence;

        public void LoadNewCharacter(string name, CharacterID character, int hp, int Speed, int Resistance, int Attack, int Defence)
        {
            Name = name;
            _character = character;
            _hp = hp;
            _speed=Speed;
            _characterSize = Resistance;
            _attack = Attack;
            _defence = Defence;
        }
    }

    #endregion
}
