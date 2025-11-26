using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/ConfluentTestCharacter", fileName = "ConfluentTestCharacterStats")]
    public class ConfluentTestCharacter : ConfluentClassCharacter
    {
        public ConfluentTestCharacter() : base()
        {
            _id = CharacterID.ConfluentTest;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
