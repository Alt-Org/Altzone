using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public class RetroflectorClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Retroflector;


        protected RetroflectorClassCharacter()
        {
            _attackStrength = ValueStrength.SemiWeak;
            _resistanceStrength = ValueStrength.Strong;
            _hpStrength = ValueStrength.Strong;
            _defenceStrength = ValueStrength.Weak;
            InitilizeValues();
        }
    }
}
