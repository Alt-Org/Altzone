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
            _defaultAttack = 14;
            _defaultDefence = 12;
            _defaultHp = 14;
            _defaultCharacterSize = 8;
            _defaultSpeed = 4;
            InitializeValues();
        }
    }
}
