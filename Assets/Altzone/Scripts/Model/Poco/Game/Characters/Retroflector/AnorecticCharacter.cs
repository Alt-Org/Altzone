using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/AnorecticCharacter", fileName = "AnorecticCharacterStats")]
    public class AnorecticCharacter : RetroflectorClassCharacter
    {
        public AnorecticCharacter() : base()
        {
            _id = CharacterID.Anorectic;
            _defaultAttack = 3;
            _defaultDefence = 2;
            _defaultHp = 1;
            _defaultCharacterSize = 1;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
