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
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 2;
            _defaultCharacterSize = 2;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
