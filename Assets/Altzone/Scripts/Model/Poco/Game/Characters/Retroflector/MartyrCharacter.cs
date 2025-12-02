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
            _defaultAttack = 3;
            _defaultDefence = 3;
            _defaultHp = 1;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
