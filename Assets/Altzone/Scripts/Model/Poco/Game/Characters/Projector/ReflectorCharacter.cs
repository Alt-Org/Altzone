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
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 6;
            _defaultCharacterSize = 2;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
