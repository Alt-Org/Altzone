using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/DesentisitizerTestCharacter", fileName = "DesentisitizerTestCharacterStats")]
    public class DesentisitizerTestCharacter : DesentisitizerClassCharacter
    {
        public DesentisitizerTestCharacter()
        {
            _id = CharacterID.DesensitizerTest;
            _defaultAttack = 2;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 2;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
