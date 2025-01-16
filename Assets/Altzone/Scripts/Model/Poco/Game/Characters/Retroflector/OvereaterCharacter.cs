using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class OvereaterCharacter : RetroflectorClassCharacter
    {
        public OvereaterCharacter() : base()
        {
            _id = CharacterID.Overeater;
            _defaultAttack = 6;
            _defaultDefence = 8;
            _defaultHp = 4;
            _defaultResistance = 12;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
