using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/IntellectualizerTestCharacter", fileName = "IntellectualizerTestCharacterStats")]
    public class IntellectualizerTestCharacter : IntellectualizerClassCharacter
    {
        public IntellectualizerTestCharacter() : base()
        {
            _id = CharacterID.IntellectualizerTest;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
