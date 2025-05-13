using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/CapitalistCharacter", fileName = "CapitalistCharacterStats")]
    public class CapitalistCharacter : IntellectualizerClassCharacter
    {
        public CapitalistCharacter() : base()
        {
            _id = CharacterID.Capitalist;
            _defaultAttack = 10;
            _defaultDefence = 3;
            _defaultHp = 8;
            _defaultCharacterSize = 8;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
