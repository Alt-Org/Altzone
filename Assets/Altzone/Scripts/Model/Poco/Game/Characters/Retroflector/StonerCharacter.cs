using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/StonerCharacter", fileName = "StonerCharacterStats")]
    public class StonerCharacter : RetroflectorClassCharacter
    {
        public StonerCharacter() : base()
        {
            _id = CharacterID.Stoner;
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 2;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
