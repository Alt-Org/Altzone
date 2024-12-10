using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class IntellectualizerClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Intellectualizer;

        //Enums should equal about 20.
        protected IntellectualizerClassCharacter()
        {
            _attackStrength = ValueStrength.Medium;
            _resistanceStrength = ValueStrength.Weak;
            _hpStrength = ValueStrength.Medium;
            _defenceStrength = ValueStrength.SemiWeak;
            _speedStrength = ValueStrength.SemiWeak;
            InitilizeValues();
        }
    }
}
