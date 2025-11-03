using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ComedianCharacter", fileName = "ComedianCharacterStats")]
    public class ComedianCharacter : TricksterClassCharacter
    {
        public ComedianCharacter()
        {
            _id = CharacterID.Comedian;
            _defaultAttack = 7;
            _defaultDefence = 5;
            _defaultHp = 2;
            _defaultCharacterSize = 6;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
