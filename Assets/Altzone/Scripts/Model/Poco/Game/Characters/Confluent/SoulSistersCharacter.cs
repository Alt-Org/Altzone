using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class SoulSistersCharacter : ConfluentClassCharacter
    {
        public SoulSistersCharacter() : base()
        {
            _id = CharacterID.ConfluentBesties;
            _defaultAttack = 2;
            _defaultDefence = 3;
            _defaultHp = 2;
            _defaultResistance = 2;
            _speed = 3;
            InitilizeValues();
        }
    }
}