using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/TestCharacter", fileName = "TestCharacterStats")]
    public class TestCharacter : TestClassCharacter
    {
        public TestCharacter() : base()
        {
            _id = CharacterID.Test;
            _defaultAttack = 7;
            _defaultDefence = 7;
            _defaultHp = 7;
            _defaultCharacterSize = 7;
            _defaultSpeed = 7;
            InitializeValues();
        }
    }
}
