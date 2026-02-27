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
            _defaultAttack = 1;
            _defaultDefence = 4;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
