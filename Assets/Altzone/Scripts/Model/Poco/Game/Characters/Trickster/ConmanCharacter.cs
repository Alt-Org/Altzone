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
            _defaultAttack = 2;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 2;
            _defaultSpeed = 6;
            InitializeValues();
        }
    }
}
