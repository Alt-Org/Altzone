using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacters/ObedientTestCharacter", fileName = "ObedientTestCharacterStats")]
    public class ObedientTestCharacter : ObedientClassCharacter
    {
        public ObedientTestCharacter() : base()
        {
            _id = CharacterID.ObedientTest;
            _defaultAttack = 10;
            _defaultDefence = 10;
            _defaultHp = 10;
            _defaultCharacterSize = 10;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
