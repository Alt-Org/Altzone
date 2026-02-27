using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/SoulSistersCharacter", fileName = "SoulSistersCharacterStats")]
    public class SoulSistersCharacter : ConfluentClassCharacter
    {
        public SoulSistersCharacter() : base()
        {
            _id = CharacterID.Soulsisters;
            _defaultAttack = 1;
            _defaultDefence = 4;
            _defaultHp = 2;
            _defaultCharacterSize = 3;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
