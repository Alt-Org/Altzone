using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ConmanCharacter", fileName = "ConmanCharacterStats")]
    public class ConmanCharacter : TricksterClassCharacter
    {
        public ConmanCharacter()
        {
            _id = CharacterID.Conman;
            _defaultAttack = 7;
            _defaultDefence = 5;
            _defaultHp = 2;
            _defaultCharacterSize = 6;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
