using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/RetroflectorTestCharacter", fileName = "RetroflectorTestCharacterStats")]
    public class RetroflectorTestCharacter : RetroflectorClassCharacter
    {
        public RetroflectorTestCharacter() : base()
        {
            _id = CharacterID.RetroflectorTest;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
