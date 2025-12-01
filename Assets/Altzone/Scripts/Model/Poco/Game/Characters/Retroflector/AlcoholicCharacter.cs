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
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
