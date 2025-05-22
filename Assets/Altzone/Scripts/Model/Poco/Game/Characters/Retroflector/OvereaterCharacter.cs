using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/OvereaterCharacter", fileName = "OvereaterCharacterStats")]
    public class OvereaterCharacter : RetroflectorClassCharacter
    {
        public OvereaterCharacter() : base()
        {
            _id = CharacterID.Overeater;
            _defaultAttack = 6;
            _defaultDefence = 8;
            _defaultHp = 4;
            _defaultCharacterSize = 12;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
