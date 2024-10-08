using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ObedientClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Obedient;


        protected ObedientClassCharacter()
        {
            _attackStrength = ValueStrength.Medium;
            _resistanceStrength = ValueStrength.Medium;
            _hpStrength = ValueStrength.Medium;
            _defenceStrength = ValueStrength.Medium;
            InitilizeValues();
        }
    }
}
