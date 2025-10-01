using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/YesmanCharacter", fileName = "YesmanCharacterStats")]
    public class YesmanCharacter : ObedientClassCharacter
    {
        public YesmanCharacter() : base()
        {
            _id = CharacterID.Yesman;
            _defaultAttack = 10;
            _defaultDefence = 10;
            _defaultHp = 10;
            _defaultCharacterSize = 10;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
