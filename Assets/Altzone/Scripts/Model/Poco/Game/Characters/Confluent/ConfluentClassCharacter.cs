using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ConfluentClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Confluent;

        //Enums should equal 16.
        protected ConfluentClassCharacter()
        {
            _attackStrength = ValueStrength.Weak;
            _resistanceStrength = ValueStrength.SemiWeak;
            _hpStrength = ValueStrength.Medium;
            _defenceStrength = ValueStrength.VeryStrong;
            InitilizeValues();
        }
    }
}
