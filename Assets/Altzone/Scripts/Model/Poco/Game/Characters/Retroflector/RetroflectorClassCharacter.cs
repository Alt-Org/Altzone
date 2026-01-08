using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class RetroflectorClassCharacter : BaseCharacter
    {


        public override CharacterClassType ClassType => CharacterClassType.Retroflector;


        protected RetroflectorClassCharacter()
        {
            _attackStrength = ValueStrength.SemiWeak;
            _characterSizeStrength = ValueStrength.VeryStrong;
            _hpStrength = ValueStrength.SemiStrong;
            _defenceStrength = ValueStrength.Strong;
            _speedStrength = ValueStrength.None;
            InitializeValues();
        }
    }
}
