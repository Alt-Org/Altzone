using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class PreacherCharacter : ObedientClassCharacter
    {
        public PreacherCharacter() : base()
        {
            _id = CharacterID.ObedientPreacher;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 3;
            _defaultResistance = 3;
            _speed = 3;
            InitilizeValues();
        }
    }
}
