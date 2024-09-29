using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class DesentisitizerClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Desensitizer;


        protected DesentisitizerClassCharacter()
        {
            _attackStrength = ValueStrength.Strong;
            _resistanceStrength = ValueStrength.VeryStrong;
            _hpStrength = ValueStrength.SemiWeak;
            _defenceStrength = ValueStrength.VeryWeak;
            InitilizeValues();
        }
    }
}
