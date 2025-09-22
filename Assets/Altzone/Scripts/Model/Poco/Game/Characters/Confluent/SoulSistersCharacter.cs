using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    //[CreateAssetMenu(menuName = "ALT-Zone/SoulSistersCharacter", fileName = "SoulSistersCharacterStats")]
    public class SoulSistersCharacter : ConfluentClassCharacter
    {
        public SoulSistersCharacter() : base()
        {
            _id = CharacterID.Soulsisters;
            _defaultAttack = 2;
            _defaultDefence = 11;
            _defaultHp = 2;
            _defaultCharacterSize = 12;
            _defaultSpeed = 1;
            InitializeValues();
        }
    }
}
