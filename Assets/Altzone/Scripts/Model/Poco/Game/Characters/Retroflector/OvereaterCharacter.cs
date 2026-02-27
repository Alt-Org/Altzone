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
            _defaultAttack = 3;
            _defaultDefence = 1;
            _defaultHp = 2;
            _defaultCharacterSize = 4;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
