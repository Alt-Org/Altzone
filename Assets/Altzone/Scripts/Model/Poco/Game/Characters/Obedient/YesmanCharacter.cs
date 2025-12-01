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
            _defaultAttack = 4;
            _defaultDefence = 4;
            _defaultHp = 4;
            _defaultCharacterSize = 3;
            _defaultSpeed = 3;
            InitializeValues();
        }
    }
}
