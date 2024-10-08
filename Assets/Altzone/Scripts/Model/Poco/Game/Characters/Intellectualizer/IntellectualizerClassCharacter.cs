using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class IntellectualizerClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Intellectualizer;

        //Enums should equal 16.
        protected IntellectualizerClassCharacter()
        {
            _attackStrength = ValueStrength.Weak;
            _resistanceStrength = ValueStrength.SemiStrong;
            _hpStrength = ValueStrength.Weak;
            _defenceStrength = ValueStrength.VeryStrong;
            InitilizeValues();
        }
    }
}
