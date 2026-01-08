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
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 2;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
