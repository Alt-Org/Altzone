using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public abstract class ProjectorClassCharacter : BaseCharacter
    {


        public override CharacterClassType ClassType => CharacterClassType.Projector;


        protected ProjectorClassCharacter()
        {
            _attackStrength = ValueStrength.SemiStrong;
            _characterSizeStrength = ValueStrength.Weak;
            _hpStrength = ValueStrength.SemiWeak;
            _defenceStrength = ValueStrength.Medium;
            _speedStrength = ValueStrength.None;
            InitializeValues();
        }
    }
}
