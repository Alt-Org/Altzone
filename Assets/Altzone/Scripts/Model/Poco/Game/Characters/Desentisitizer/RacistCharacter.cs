using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class RacistCharacter : DesentisitizerClassCharacter
    {
        public RacistCharacter()
        {
            _id = CharacterID.Racist;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultResistance = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
