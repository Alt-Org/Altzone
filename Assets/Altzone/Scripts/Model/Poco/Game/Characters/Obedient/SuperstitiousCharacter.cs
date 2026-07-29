using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/SuperstitiousCharacter", fileName = "SuperstitiousCharacterStats")]
    public class SuperstitiousCharacter : ObedientClassCharacter
    {
        public SuperstitiousCharacter() : base()
        {
            _id = CharacterID.Superstitious;
            _defaultAttack = 5;
            _defaultDefence = 3;
            _defaultHp = 2;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
