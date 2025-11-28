using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    [CreateAssetMenu(menuName = "ALT-Zone/Characters/FashionismCharacter", fileName = "FashionismCharacterStats")]
    public class FashionismCharacter : ObedientClassCharacter
    {
        public FashionismCharacter() : base()
        {
            _id = CharacterID.Fashionism;
            _defaultAttack = 10;
            _defaultDefence = 10;
            _defaultHp = 10;
            _defaultCharacterSize = 10;
            _defaultSpeed = 5;
            InitializeValues();
        }
    }
}
