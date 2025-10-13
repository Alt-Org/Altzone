using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/ReflectorCharacter", fileName = "ReflectorCharacterStats")]
    public class ReflectorCharacter : ProjectorClassCharacter
    {
        public ReflectorCharacter() : base()
        {
            _id = CharacterID.Reflector;
            _defaultAttack = 7;
            _defaultDefence = 10;
            _defaultHp = 3;
            _defaultCharacterSize = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
