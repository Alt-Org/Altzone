using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class ConmanCharacter : TricksterClassCharacter
    {
        public ConmanCharacter()
        {
            _id = CharacterID.Conman;
            _defaultAttack = 7;
            _defaultDefence = 5;
            _defaultHp = 2;
            _defaultResistance = 6;
            _defaultSpeed = 10;
            InitializeValues();
        }
    }
}
