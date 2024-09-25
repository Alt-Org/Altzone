using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class BodybuilderCharacter : DesentisitizerClassCharacter
    {
        public BodybuilderCharacter()
        {
            _id = CharacterID.DesensitizerBodybuilder;
            _defaultAttack = 4;
            _defaultDefence = 0;
            _defaultHp = 2;
            _defaultResistance = 3;
            _speed = 2;
            InitilizeValues();
        }
    }
}
