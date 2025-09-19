using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/AlcoholicCharacter", fileName = "AlcoholicCharacterStats")]
    public class AlcoholicCharacter : RetroflectorClassCharacter
    {
        public AlcoholicCharacter(): base()
        {
            _id = CharacterID.Alcoholic;
            _defaultAttack = 9;
            _defaultDefence = 8;
            _defaultHp = 1;
            _defaultCharacterSize = 10;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
