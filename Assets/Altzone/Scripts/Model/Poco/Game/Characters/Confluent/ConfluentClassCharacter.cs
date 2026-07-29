using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ConfluentClassCharacter : BaseCharacter
    {


        public override CharacterClassType ClassType => CharacterClassType.Confluent;

        //Enums should equal about 20.
        protected ConfluentClassCharacter()
        {
            _attackStrength = ValueStrength.VeryWeak;
            _characterSizeStrength = ValueStrength.Strong;
            _hpStrength = ValueStrength.SemiWeak;
            _defenceStrength = ValueStrength.Strong;
            _speedStrength = ValueStrength.None;
            InitializeValues();
        }
    }
}
