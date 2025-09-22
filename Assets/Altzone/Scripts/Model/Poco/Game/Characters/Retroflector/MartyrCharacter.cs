using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/MartyrCharacter", fileName = "MartyrCharacterStats")]
    public class MartyrCharacter : RetroflectorClassCharacter
    {
        public MartyrCharacter() : base()
        {
            _id = CharacterID.Martyr;
            _defaultAttack = 6;
            _defaultDefence = 8;
            _defaultHp = 1;
            _defaultCharacterSize = 12;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
