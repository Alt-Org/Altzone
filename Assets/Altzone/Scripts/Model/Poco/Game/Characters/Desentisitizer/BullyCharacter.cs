using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/BullyCharacter", fileName = "BullyCharacterStats")]
    public class BullyCharacter : DesentisitizerClassCharacter
    {
        public BullyCharacter()
        {
            _id = CharacterID.Bully;
            _defaultAttack = 6;
            _defaultDefence = 12;
            _defaultHp = 1;
            _defaultCharacterSize = 8;
            _defaultSpeed = 2;
            InitializeValues();
        }
    }
}
