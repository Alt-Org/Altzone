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
            _defaultAttack = 4;
            _defaultDefence = 4;
            _defaultHp = 4;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
