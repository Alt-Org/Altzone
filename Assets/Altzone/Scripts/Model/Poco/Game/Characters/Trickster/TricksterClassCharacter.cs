using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class TricksterClassCharacter : BaseCharacter
    {
        public override CharacterClassID ClassID => CharacterClassID.Trickster;

        //Enums should equal 16.
        protected TricksterClassCharacter()
        {
            _attackStrength = ValueStrength.SemiWeak;
            _resistanceStrength = ValueStrength.Weak;
            _hpStrength = ValueStrength.Medium;
            _defenceStrength = ValueStrength.VeryWeak;
            _speedStrength = ValueStrength.Strong;
            InitializeValues();
        }
    }
}
