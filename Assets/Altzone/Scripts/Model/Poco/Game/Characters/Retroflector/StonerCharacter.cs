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
            _defaultAttack = 8;
            _defaultDefence = 9;
            _defaultHp = 2;
            _defaultCharacterSize = 6;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
