using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class OvereaterCharacter : RetroflectorClassCharacter
    {
        public OvereaterCharacter() : base()
        {
            _id = CharacterID.RetroflectorOvereater;
            _defaultAttack = 2;
            _defaultDefence = 0;
            _defaultHp = 4;
            _defaultResistance = 3;
            _speed = 2;
            InitilizeValues();
        }
    }
}
