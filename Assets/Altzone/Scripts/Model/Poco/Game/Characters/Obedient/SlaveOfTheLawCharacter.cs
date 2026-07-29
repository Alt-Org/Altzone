using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/SlaveOfTheLawCharacter", fileName = "SlaveOfTheLawCharacterStats")]
    public class SlaveOfTheLawCharacter : ObedientClassCharacter
    {
        public SlaveOfTheLawCharacter() : base()
        {
            _id = CharacterID.SlaveOfTheLaw;
            _defaultAttack = 4;
            _defaultDefence = 4;
            _defaultHp = 4;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
