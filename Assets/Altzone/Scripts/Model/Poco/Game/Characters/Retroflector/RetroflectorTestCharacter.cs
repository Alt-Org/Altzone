using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/RetroflectorTestCharacter", fileName = "RetroflectorTestCharacterStats")]
    public class RetroflectorTestCharacter : RetroflectorClassCharacter
    {
        public RetroflectorTestCharacter() : base()
        {
            _id = CharacterID.RetroflectorTest;
            _defaultAttack = 6;
            _defaultDefence = 8;
            _defaultHp = 4;
            _defaultCharacterSize = 12;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
