using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class AlcoholicCharacter : RetroflectorClassCharacter
    {
        public AlcoholicCharacter(): base()
        {
            _id = CharacterID.RetroflectorAlcoholic;
            _defaultAttack = 3;
            _defaultDefence = 0;
            _defaultHp = 3;
            _defaultResistance = 3;
            _speed = 2;
            InitilizeValues();
        }
    }
}
