using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/Character/JokerCharacter", fileName = "JokerCharacterStats")]
    public class JokerCharacter : TricksterClassCharacter
    {
        public JokerCharacter()
        {
            _id = CharacterID.Joker;
            _defaultAttack = 8;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultCharacterSize = 4;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
