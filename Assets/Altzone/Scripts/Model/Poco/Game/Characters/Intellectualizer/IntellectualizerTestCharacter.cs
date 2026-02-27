using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/IntellectualizerTestCharacter", fileName = "IntellectualizerTestCharacterStats")]
    public class IntellectualizerTestCharacter : IntellectualizerClassCharacter
    {
        public IntellectualizerTestCharacter() : base()
        {
            _id = CharacterID.IntellectualizerTest;
            _defaultAttack = 2;
            _defaultDefence = 1;
            _defaultHp = 2;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
