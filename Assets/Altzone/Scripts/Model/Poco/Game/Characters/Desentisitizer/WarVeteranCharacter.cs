using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/WarVeteranCharacter", fileName = "WarVeteranCharacterStats")]
    public class WarVeteranCharacter : DesentisitizerClassCharacter
    {
        public WarVeteranCharacter()
        {
            _id = CharacterID.WarVeteran;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
