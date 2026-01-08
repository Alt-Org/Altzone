using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class TestClassCharacter : BaseCharacter
    {
        public override CharacterClassType ClassType => CharacterClassType.None;


        protected TestClassCharacter()
        {
            _attackStrength = ValueStrength.Medium;
            _characterSizeStrength = ValueStrength.Medium;
            _hpStrength = ValueStrength.Medium;
            _defenceStrength = ValueStrength.Medium;
            _speedStrength = ValueStrength.Medium;
            InitializeValues();
        }
    }
}
