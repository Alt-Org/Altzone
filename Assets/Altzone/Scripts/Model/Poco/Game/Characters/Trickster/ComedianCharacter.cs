using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class ComedianCharacter : TricksterClassCharacter
    {
        public ComedianCharacter()
        {
            _id = CharacterID.TricksterComedian;
            _defaultAttack = 8;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultResistance = 4;
            _defaultSpeed = 11;
            InitilizeValues();
        }
    }
}
