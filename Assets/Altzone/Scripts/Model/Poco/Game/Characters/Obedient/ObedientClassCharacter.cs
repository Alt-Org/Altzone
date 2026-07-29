using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ObedientClassCharacter : BaseCharacter
    {
        public override CharacterClassType ClassType => CharacterClassType.Obedient;


        protected ObedientClassCharacter()
        {
            _attackStrength = ValueStrength.None;
            _characterSizeStrength = ValueStrength.None;
            _hpStrength = ValueStrength.None;
            _defenceStrength = ValueStrength.None;
            _speedStrength = ValueStrength.None;
            InitializeValues();
        }
    }
}
