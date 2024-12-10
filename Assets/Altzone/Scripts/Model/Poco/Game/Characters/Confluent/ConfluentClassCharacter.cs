using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ConfluentClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Confluent;

        //Enums should equal about 20.
        protected ConfluentClassCharacter()
        {
            _attackStrength = ValueStrength.VeryWeak;
            _resistanceStrength = ValueStrength.Strong;
            _hpStrength = ValueStrength.SemiWeak;
            _defenceStrength = ValueStrength.Strong;
            _speedStrength = ValueStrength.Medium;
            InitilizeValues();
        }
    }
}
