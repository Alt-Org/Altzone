using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class BodybuilderCharacter : DesentisitizerClassCharacter
    {
        public BodybuilderCharacter()
        {
            _id = CharacterID.Bodybuilder;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
