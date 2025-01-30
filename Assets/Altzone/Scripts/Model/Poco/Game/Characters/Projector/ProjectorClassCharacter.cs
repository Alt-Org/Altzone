using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ProjectorClassCharacter : BaseCharacter
    {


        public override CharacterClassID ClassID => CharacterClassID.Projector;


        protected ProjectorClassCharacter()
        {
            _attackStrength = ValueStrength.SemiStrong;
            _resistanceStrength = ValueStrength.Weak;
            _hpStrength = ValueStrength.SemiWeak;
            _defenceStrength = ValueStrength.Medium;
            _speedStrength = ValueStrength.Medium;
            InitializeValues();
        }
    }
}
