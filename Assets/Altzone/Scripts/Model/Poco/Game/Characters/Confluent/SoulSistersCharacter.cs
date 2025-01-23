using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class SoulSistersCharacter : ConfluentClassCharacter
    {
        public SoulSistersCharacter() : base()
        {
            _id = CharacterID.Soulsisters;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultResistance = 12;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
